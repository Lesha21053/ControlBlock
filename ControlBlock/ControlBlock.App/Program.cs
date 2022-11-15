using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.IO.Ports;
using System.Linq;
using ControlBlock.COM;
using ControlBlock.Interfaces;
using ControlBlock.LIDAR;
using ControlBlock.MessageBus;
using ControlBlock.Models;
using ControlBlock.Models.Settings;

namespace ControlBlock.App
{
    class Program
    {
       static  MessageBusClient bus = new("127.0.0.1", 5000);
       static COMPortSettings comsettings = new COMPortSettings()
        {
            PortName = "COM4",
            BaudRate = 115200,
            Parity = 0,
            DataBits = 8,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            RtsEnable = false,
            ReadBufferSize = 64000,
            WriteBufferSize = 4096,
            WriteTimeout = 1000,
            ReadTimeout = 1000,
            ReceivedBytesThreshold = 4096,
            ReciveAnswerTimeOut = 200
        };
        static void Main(string[] args)
        {
           
            Console.WriteLine("Cтарт");
            

          
            try
            {
                LIDARDataHandler lidar = new LIDARDataHandler(new COMPort(comsettings));
                lidar.PackageReceived += GetDataFromLidar;
                lidar.Run();
                
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            Console.ReadLine();
        }

        public static void GetDataFromLidar(Memory<LIDARMeasurements> data)
        {
           // Console.WriteLine("Старт пакета");
           string dataStr = string.Empty;
         
            foreach (var m in  data.Span)
            {
                dataStr += $"{m.Angle} {m.Distance};";
                Console.WriteLine($"Угол {m.Angle} Дистанция {m.Distance}");
            }
           bus.SendMessage(dataStr);
            //Console.WriteLine(dataStr);
           // File.AppendAllLines("mes.txt", mes.ToArray());

        }
    }
}