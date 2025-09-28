using System.ServiceModel;
using SnusProject.Models;

namespace SnusProject
{
    [ServiceContract]
    public interface IRobotArmCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnStateChanged(RobotArmState state);
    }
}
