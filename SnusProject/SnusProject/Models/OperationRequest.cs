using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SnusProject.Models
{
    public class OperationRequest
    {
        public int ClientId { get; set; }
        public string Operation { get; set; }

        public OperationRequest(int clientId, string operation)
        {
            ClientId = clientId;
            Operation = operation;
        }
    }
}