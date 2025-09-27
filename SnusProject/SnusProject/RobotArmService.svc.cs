using SnusProject.Data;
using SnusProject.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SnusProject
{
    public class RobotArmService : IRobotArmService
    {
        private static RobotArm robotArm = new RobotArm();

        private static List<ClientInfo> clients = new List<ClientInfo>
        {
            new ClientInfo(1, "Client 1", ClientPermission.FullAccess, 1),
            new ClientInfo(2, "Client 2", ClientPermission.MoveOnly, 2),
            new ClientInfo(3, "Client 3", ClientPermission.RotateOnly, 2)
        };

        private DatabaseContext _db;

        private static List<OperationRequest> operationQueue = new List<OperationRequest>();

        private static Timer _queueTimer;

        public RobotArmService()
        {
            _db = new DatabaseContext();

            if (_queueTimer == null)
            {
                _queueTimer = new Timer(
                    _ => ProcessQueue(),
                    null,
                    0,
                    1000);
            }
        }

        public void EnqueueMoveLeft(int clientId) => EnqueueOperation(clientId, "MoveLeft");
        public void EnqueueMoveRight(int clientId) => EnqueueOperation(clientId, "MoveRight");
        public void EnqueueMoveUp(int clientId) => EnqueueOperation(clientId, "MoveUp");
        public void EnqueueMoveDown(int clientId) => EnqueueOperation(clientId, "MoveDown");
        public void EnqueueRotate(int clientId) => EnqueueOperation(clientId, "Rotate");

        public string GetCurrentState() => robotArm.ToString();

        private void EnqueueOperation(int clientId, string operation)
        {
            var client = clients.Find(c => c.ClientId == clientId);
            if (client == null) return;

            if (!IsOperationAllowed(client.Permission, operation))
            {
                LogOperation(clientId, operation, false);
                return;
            }

            operationQueue.Add(new OperationRequest(clientId, operation));
        }

        private void ProcessQueue()
        {
            operationQueue = operationQueue
                .OrderBy(r => clients.Find(c => c.ClientId == r.ClientId).Priority)
                .ToList();

            while (operationQueue.Count > 0)
            {
                var request = operationQueue[0];
                ExecuteOperation(request.ClientId, request.Operation);
                operationQueue.RemoveAt(0);
            }
        }

        private bool ExecuteOperation(int clientId, string operation)
        {
            var client = clients.Find(c => c.ClientId == clientId);
            if (client == null)
                return false;

            bool success = false;

            switch (operation)
            {
                case "MoveLeft":
                    success = robotArm.MoveLeft();
                    break;
                case "MoveRight":
                    success = robotArm.MoveRight();
                    break;
                case "MoveUp":
                    success = robotArm.MoveUp();
                    break;
                case "MoveDown":
                    success = robotArm.MoveDown();
                    break;
                case "Rotate":
                    success = robotArm.Rotate();
                    break;
                default:
                    success = false;
                    break;
            }

            LogOperation(clientId, operation, success);
            return success;
        }

        private bool IsOperationAllowed(ClientPermission permission, string operation)
        {
            switch (permission)
            {
                case ClientPermission.FullAccess:
                    return true;
                case ClientPermission.MoveOnly:
                    return operation != "Rotate";
                case ClientPermission.RotateOnly:
                    return operation == "Rotate";
                default:
                    return false;
            }
        }

        private void LogOperation(int clientId, string operation, bool success)
        {
            var log = new OperationLog(clientId, operation, success);
            _db.OperationLogs.Add(log);
            _db.SaveChanges();
        }
    }
}
