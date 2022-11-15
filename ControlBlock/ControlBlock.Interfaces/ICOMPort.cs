
using System;

namespace ControlBlock.Interfaces
{
    public interface IPort :IDisposable
    {
        delegate void PortDataRecived(byte[] data);
        event PortDataRecived DataReceived;

        bool Ready {  get; }
        bool InputIsListen { get; set; }
        public byte[] Send(byte[] data, bool answerIsRequired = false);
    }
}