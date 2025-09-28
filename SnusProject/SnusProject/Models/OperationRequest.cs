using SnusProject.Models;
using System.Threading.Tasks;

public class OperationRequest
{
    public int ClientId { get; }
    public string Operation { get; }
    public string Hmac { get; }
    public TaskCompletionSource<OperationResult> Completion { get; }

    public OperationRequest(int clientId, string operation, string hmac)
    {
        ClientId = clientId;
        Operation = operation;
        Hmac = hmac;
        Completion = new TaskCompletionSource<OperationResult>();
    }
}
