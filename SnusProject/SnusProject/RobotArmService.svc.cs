using SnusProject.Data;
using SnusProject.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        private static ConcurrentQueue<OperationRequest> highPriorityQueue = new ConcurrentQueue<OperationRequest>();
        private static ConcurrentQueue<OperationRequest> normalQueue = new ConcurrentQueue<OperationRequest>();

        private static Timer _queueTimer;

        public RobotArmService()
        {
            if (_queueTimer == null)
            {
                _queueTimer = new Timer(_ => ProcessQueue(), null, 0, 100);
            }
        }

        public Task<OperationResult> EnqueueMoveLeftAsync(int clientId) => EnqueueOperationAsync(clientId, "MoveLeft");
        public Task<OperationResult> EnqueueMoveRightAsync(int clientId) => EnqueueOperationAsync(clientId, "MoveRight");
        public Task<OperationResult> EnqueueMoveUpAsync(int clientId) => EnqueueOperationAsync(clientId, "MoveUp");
        public Task<OperationResult> EnqueueMoveDownAsync(int clientId) => EnqueueOperationAsync(clientId, "MoveDown");
        public Task<OperationResult> EnqueueRotateAsync(int clientId) => EnqueueOperationAsync(clientId, "Rotate");

        private Task<OperationResult> EnqueueOperationAsync(int clientId, string operation)
        {
            var client = clients.Find(c => c.ClientId == clientId);
            if (client == null)
            {
                return Task.FromResult(new OperationResult(operation, false));
            }

            if (!IsOperationAllowed(client.Permission, operation))
            {
                LogOperation(clientId, operation, false);
                return Task.FromResult(new OperationResult(operation, false));
            }

            var request = new OperationRequest(clientId, operation);

            if (client.Priority == 1)
                highPriorityQueue.Enqueue(request);
            else
                normalQueue.Enqueue(request);

            return request.Completion.Task;
        }

        private void ProcessQueue()
        {
            OperationRequest request;

            while (highPriorityQueue.TryDequeue(out request))
            {
                bool success = ExecuteOperation(request.ClientId, request.Operation);
                request.Completion.SetResult(new OperationResult(request.Operation, success));
            }

            while (normalQueue.TryDequeue(out request))
            {
                bool success = ExecuteOperation(request.ClientId, request.Operation);
                request.Completion.SetResult(new OperationResult(request.Operation, success));
            }
        }

        public RobotArmState GetCurrentState()
        {
            return new RobotArmState
            {
                X = robotArm.X,
                Y = robotArm.Y,
                Angle = robotArm.Angle
            };
        }

        private bool ExecuteOperation(int clientId, string operation)
        {
            var client = clients.Find(c => c.ClientId == clientId);
            if (client == null) return false;

            bool success = false;

            switch (operation)
            {
                case "MoveLeft": success = robotArm.MoveLeft(); break;
                case "MoveRight": success = robotArm.MoveRight(); break;
                case "MoveUp": success = robotArm.MoveUp(); break;
                case "MoveDown": success = robotArm.MoveDown(); break;
                case "Rotate": success = robotArm.Rotate(); break;
            }

            LogOperation(clientId, operation, success);
            return success;
        }

        private bool IsOperationAllowed(ClientPermission permission, string operation)
        {
            switch (permission)
            {
                case ClientPermission.FullAccess: return true;
                case ClientPermission.MoveOnly: return operation != "Rotate";
                case ClientPermission.RotateOnly: return operation == "Rotate";
                default: return false;
            }
        }

        private void LogOperation(int clientId, string operation, bool success)
        {
            using (var db = new DatabaseContext())
            {
                db.OperationLogs.Add(new OperationLog(clientId, operation, success));
                db.SaveChanges();
            }
        }
    }
}