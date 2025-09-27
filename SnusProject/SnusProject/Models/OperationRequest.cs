using System.Threading.Tasks;

namespace SnusProject.Models
{
    public class OperationRequest
    {
        public int ClientId { get; set; }
        public string Operation { get; set; }
        public TaskCompletionSource<OperationResult> Completion { get; set; }

        public OperationRequest(int clientId, string operation)
        {
            ClientId = clientId;
            Operation = operation;
            Completion = new TaskCompletionSource<OperationResult>();
        }
    }
}
