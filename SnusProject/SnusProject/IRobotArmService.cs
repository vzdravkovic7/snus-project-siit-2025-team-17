using SnusProject.Models;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SnusProject
{
    [ServiceContract]
    public interface IRobotArmService
    {
        [OperationContract]
        Task<OperationResult> EnqueueMoveLeftAsync(int clientId);

        [OperationContract]
        Task<OperationResult> EnqueueMoveRightAsync(int clientId);

        [OperationContract]
        Task<OperationResult> EnqueueMoveUpAsync(int clientId);

        [OperationContract]
        Task<OperationResult> EnqueueMoveDownAsync(int clientId);

        [OperationContract]
        Task<OperationResult> EnqueueRotateAsync(int clientId);

        [OperationContract]
        RobotArmState GetCurrentState();
    }
}
