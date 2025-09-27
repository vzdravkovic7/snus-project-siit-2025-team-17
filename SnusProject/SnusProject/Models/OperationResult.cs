using System.Runtime.Serialization;

namespace SnusProject.Models
{
    [DataContract]
    public class OperationResult
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string Operation { get; set; }

        public OperationResult(string operation, bool success)
        {
            Operation = operation;
            Success = success;
        }
        public OperationResult() { }
    }
}
