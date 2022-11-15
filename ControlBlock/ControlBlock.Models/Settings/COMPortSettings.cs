using System.IO.Ports;

namespace ControlBlock.Models.Settings
{
    public class COMPortSettings
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake Handshake { get; set; }
        public bool RtsEnable { get; set; } 
        public int ReadBufferSize { get; set; }        
        public int WriteBufferSize { get; set; }
        public int WriteTimeout { get; set; }
        public int ReadTimeout { get; set; }
        public int ReceivedBytesThreshold { get; set; }
        public int ReciveAnswerTimeOut { get; set; }
    }
}