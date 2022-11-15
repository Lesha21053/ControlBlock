using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ControlBlock.MessageBus
{
    public class MessageBusClient
    {
        private readonly string _ip;
        private readonly int _port;

        public MessageBusClient(string ipServer, int port)
        {
            _ip = ipServer;
            _port = port;
        }

        public void SendMessage(string data)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(_ip, _port);
                var stream = client.GetStream();
                var codeBytes = Encoding.Default.GetBytes(data);
                var unicodeBytes = Encoding.Convert(Encoding.Default, Encoding.Unicode, codeBytes);
                stream.Write(unicodeBytes, 0, unicodeBytes.Length);
                stream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (client != null)
                    client.Close();
            }
        }
    }
}