using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnusProject.Models
{
    public class OperationLog
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Operation { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }

        public OperationLog() { }

        public OperationLog(int clientId, string operation, bool success)
        {
            ClientId = clientId;
            Operation = operation;
            Success = success;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp}] Client {ClientId}: {Operation} -> {(Success ? "OK" : "Failed")}";
        }
    }
}