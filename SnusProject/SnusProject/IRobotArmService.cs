using SnusProject.Models;
using System.ServiceModel;

namespace SnusProject
{
    [ServiceContract]
    public interface IRobotArmService
    {
        [OperationContract]
        void EnqueueMoveLeft(int clientId);

        [OperationContract]
        void EnqueueMoveRight(int clientId);

        [OperationContract]
        void EnqueueMoveUp(int clientId);

        [OperationContract]
        void EnqueueMoveDown(int clientId);

        [OperationContract]
        void EnqueueRotate(int clientId);

        [OperationContract]
        RobotArmState GetCurrentState();
    }
}
