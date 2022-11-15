using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;

namespace LIDARDataViewer
{
    public class DataListener
    {
        private readonly int _port;
        private TcpListener _listener;
        private readonly string _ip;
        private bool IsRun;


        public delegate void PackageReceivedEventHandler(string data);

        public event PackageReceivedEventHandler PackageReceived;

        public DataListener(string ip, int port)
        {
            _port = port;
            _ip = ip;
        }

        public void Run(CancellationToken cts)
        {
         
                    try
                    {
                        _listener = new TcpListener(IPAddress.Parse(_ip), _port);
                        _listener.Start();


                        var data = new byte[64];

                        while (!cts.IsCancellationRequested)
                        {
                            var client = _listener.AcceptTcpClient();
                            var stream = client.GetStream();

                            var builder = new StringBuilder();
                            var bytes = 0;
                            do
                            {
                                bytes = stream.Read(data, 0, data.Length);
                                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                            } while (stream.DataAvailable);

                            var message = builder.ToString();

                            PackageReceived?.Invoke(message);

                            stream.Close();
                            client.Close();

                        }
                    }
                   
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        if (_listener != null)
                            _listener.Stop();
                    }
            }
           
        

    }
}
