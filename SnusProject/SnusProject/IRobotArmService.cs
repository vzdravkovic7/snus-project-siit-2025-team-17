using SnusProject.Models;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SnusProject
{
    [ServiceContract(CallbackContract = typeof(IRobotArmCallback))]
    public interface IRobotArmService
    {
        [OperationContract]
        Task<OperationResult> EnqueueMoveLeftAsync(int clientId, string hmac);

        [OperationContract]
        Task<OperationResult> EnqueueMoveRightAsync(int clientId, string hmac);

        [OperationContract]
        Task<OperationResult> EnqueueMoveUpAsync(int clientId, string hmac);

        [OperationContract]
        Task<OperationResult> EnqueueMoveDownAsync(int clientId, string hmac);

        [OperationContract]
        Task<OperationResult> EnqueueRotateAsync(int clientId, string hmac);

        [OperationContract]
        RobotArmState GetCurrentState();

        [OperationContract]
        void Subscribe();

        [OperationContract]
        void Unsubscribe();
    }
}
