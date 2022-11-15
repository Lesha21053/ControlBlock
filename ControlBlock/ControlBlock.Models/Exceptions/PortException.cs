using System;
using ControlBlock.Models.Enums;

namespace ControlBlock.Models.Exceptions
{
    public class PortException: Exception
    {
        public PortStatusCode  StatusCode { get; }
        public PortException(string message,  PortStatusCode statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}