using SnusProject.Data;
using SnusProject.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace SnusProject
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RobotArmService : IRobotArmService
    {
        private static List<IRobotArmCallback> subscribers = new List<IRobotArmCallback>();
        private static readonly object subscriberLock = new object();

        private static RobotArm robotArm = new RobotArm();
        private static readonly object robotLock = new object();

        private static List<ClientInfo> clients = new List<ClientInfo>
        {
            new ClientInfo(1, "Client 1", ClientPermission.FullAccess, 1),
            new ClientInfo(2, "Client 2", ClientPermission.MoveOnly, 2),
            new ClientInfo(3, "Client 3", ClientPermission.RotateOnly, 2)
        };

        private static BlockingCollection<OperationRequest> highPriorityQueue = new BlockingCollection<OperationRequest>();
        private static BlockingCollection<OperationRequest> normalQueue = new BlockingCollection<OperationRequest>();

        private static CancellationTokenSource cts = new CancellationTokenSource();

        public RobotArmService()
        {
            Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        }

        #region Enqueue Methods
        public Task<OperationResult> EnqueueMoveLeftAsync(int clientId, string hmac) => EnqueueOperationAsync(clientId, "MoveLeft", hmac);
        public Task<OperationResult> EnqueueMoveRightAsync(int clientId, string hmac) => EnqueueOperationAsync(clientId, "MoveRight", hmac);
        public Task<OperationResult> EnqueueMoveUpAsync(int clientId, string hmac) => EnqueueOperationAsync(clientId, "MoveUp", hmac);
        public Task<OperationResult> EnqueueMoveDownAsync(int clientId, string hmac) => EnqueueOperationAsync(clientId, "MoveDown", hmac);
        public Task<OperationResult> EnqueueRotateAsync(int clientId, string hmac) => EnqueueOperationAsync(clientId, "Rotate", hmac);

        private Task<OperationResult> EnqueueOperationAsync(int clientId, string operation, string hmacFromClient)
        {
            var client = clients.Find(c => c.ClientId == clientId);
            if (client == null) return Task.FromResult(new OperationResult(operation, false));

            if (!IsOperationAllowed(client.Permission, operation))
            {
                LogOperation(clientId, operation, false);
                return Task.FromResult(new OperationResult(operation, false));
            }

            string message = $"{clientId}:{operation}";
            if (!SecurityHelper.VerifyHmac(message, hmacFromClient))
            {
                LogOperation(clientId, operation, false);
                return Task.FromResult(new OperationResult(operation, false));
            }

            var request = new OperationRequest(clientId, operation, hmacFromClient);

            if (client.Priority == 1)
                highPriorityQueue.Add(request);
            else
                normalQueue.Add(request);

            return request.Completion.Task;
        }
        #endregion

        #region Queue Processing
        private void ProcessQueue()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                OperationRequest request = null;

                if (!highPriorityQueue.TryTake(out request, 1000, cts.Token))
                {
                    normalQueue.TryTake(out request, 1000, cts.Token);
                }

                if (request != null)
                {
                    bool success = ExecuteOperation(request.ClientId, request.Operation);
                    request.Completion.SetResult(new OperationResult(request.Operation, success));

                    NotifyClients();
                }
            }
        }
        #endregion

        #region Robot Operations
        public RobotArmState GetCurrentState()
        {
            lock (robotLock)
            {
                return new RobotArmState
                {
                    X = robotArm.X,
                    Y = robotArm.Y,
                    Angle = robotArm.Angle
                };
            }
        }

        private bool ExecuteOperation(int clientId, string operation)
        {
            lock (robotLock)
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
        #endregion

        #region Database Logging
        private void LogOperation(int clientId, string operation, bool success)
        {
            using (var db = new DatabaseContext())
            {
                db.OperationLogs.Add(new OperationLog(clientId, operation, success));
                db.SaveChanges();
            }
        }
        #endregion

        #region Callback (Observer)
        public void Subscribe()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IRobotArmCallback>();
            lock (subscriberLock)
            {
                if (!subscribers.Contains(callback))
                    subscribers.Add(callback);
            }
        }

        public void Unsubscribe()
        {
            var callback = OperationContext.Current.GetCallbackChannel<IRobotArmCallback>();
            lock (subscriberLock)
            {
                if (subscribers.Contains(callback))
                    subscribers.Remove(callback);
            }
        }

        private void NotifyClients()
        {
            var state = GetCurrentState();
            lock (subscriberLock)
            {
                foreach (var sub in subscribers.ToArray())
                {
                    try
                    {
                        sub.OnStateChanged(state);
                    }
                    catch
                    {
                        subscribers.Remove(sub);
                    }
                }
            }
        }
        #endregion
    }
}
