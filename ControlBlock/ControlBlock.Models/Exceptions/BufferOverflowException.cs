using System;

namespace ControlBlock.Models.Exceptions
{
    public class BufferOverflowException : Exception
    {
        public  int BufferSize   { get; }
        public BufferOverflowException(string message, int bufferSize)
            : base(message)
        {
            BufferSize = bufferSize;
        }
    }
}