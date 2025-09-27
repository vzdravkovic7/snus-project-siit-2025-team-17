namespace ClientApp
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Operation { get; set; }

        public OperationResult() { }

        public OperationResult(string operation, bool success)
        {
            Operation = operation;
            Success = success;
        }
    }
}
