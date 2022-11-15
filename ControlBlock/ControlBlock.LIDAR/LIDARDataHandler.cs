using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ControlBlock.Interfaces;
using ControlBlock.Models;
using ControlBlock.Models.Exceptions;

namespace ControlBlock.LIDAR
{
    public class LIDARDataHandler : IDisposable
    {
        private const byte frameHeader = 0xAA;
        private const byte dataHeader = 0xAD;
        
       

        private const int firstAngleBytePosition = 11;
        private const int firstDistBytePosition = 14;
        private const int distanceSizeBytePosition = 7;

        private readonly IPort _port;
        private readonly int _inputBufferSize ;
        private readonly LIDARBuffer _inputBuffer;
        private readonly Memory<byte> _packageBuffer;
        private Memory<LIDARMeasurements> measurementsBuffer;

        public delegate void PackageReceivedEventHandler(Memory<LIDARMeasurements> measurements);

        public event PackageReceivedEventHandler PackageReceived;

        public LIDARDataHandler(IPort port, int bufferSize=64000)
        {
            _port = port;
            _port.DataReceived += Datareciver;
            _inputBufferSize = bufferSize;
            _inputBuffer = new LIDARBuffer(_inputBufferSize);
            _packageBuffer = new Memory<byte>(new byte[1024]);
        }
        public void Run()
        {
            _port.InputIsListen = true;

            while (_port.InputIsListen)
                try
                {
                    ReadPackage();
                }
            
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
        }

        public void Stop()
        {
            _port.InputIsListen = false;
        }
        
        private void Datareciver(byte[] data)
        {
            _inputBuffer.Write(data);
        }
        
        public void Dispose()
        {
            _port?.Dispose();
        }
        private void ReadPackage()
        {
            SearchPreamble();
            var packageSize = GetPackageByPreamble() + 2;
            _inputBuffer.Read(_packageBuffer, 8, _inputBuffer.ReadPointerPosition, packageSize - 8);
            GetMeasurements(_packageBuffer.Slice(0, packageSize));
        }
        private ushort GetPackageByPreamble()
        {
            return BitConverter.ToUInt16(new[] { _packageBuffer.Span[2], _packageBuffer.Span[1] });
        }
        private void SearchPreamble()
        {
            do
            {
                if (_inputBuffer.ReadByte() != frameHeader) continue;
                if (_inputBuffer.Buffer.Span[_inputBuffer.ReadPointerPosition + 4] == dataHeader &&
                    _inputBuffer.Buffer.Span[_inputBuffer.ReadPointerPosition + 3] == 97)
                {
                    _inputBuffer.Read(_packageBuffer, _inputBuffer.ReadPointerPosition - 1, 8);
                    break;
                }
            } while (true);
        }
        private void GetMeasurements(Memory<byte> data)
        {
            
            if (!CheckSumIsTrue(data))
            {
                _inputBuffer.ReadPointerPosition = _inputBuffer.ReadPointerPosition - data.Length + 8;
                return;
            };

            double startAngle = BitConverter.ToUInt16(new []{data.Span[firstAngleBytePosition+1], data.Span[firstAngleBytePosition]}) * 0.01;
            int measurementsSize = (data.Span[distanceSizeBytePosition] - 5) / 3;
            measurementsBuffer = new Memory<LIDARMeasurements>(new LIDARMeasurements[measurementsSize]);
            
           
            for (int n = 0; n < measurementsSize; n++)
            {
                if (measurementsBuffer.Span[n] == null)
                    measurementsBuffer.Span[n] = new LIDARMeasurements();
                
                measurementsBuffer.Span[n].Angle = startAngle + 22.5 * (n-1) /measurementsSize;
                measurementsBuffer.Span[n].Distance = BitConverter.ToUInt16(new byte[]{data.Span[firstDistBytePosition+n*3+1], data.Span[firstDistBytePosition+n*3]});
                double angle = startAngle + 22.5 * (n-1) / measurementsSize;
  
            }
            
            PackageReceived?.Invoke(measurementsBuffer);
            
        }
        private bool CheckSumIsTrue(Memory<byte> data)
        {
            int sum=0;
            for(int i =0 ; i<data.Length-2; i++)
                sum += data.Span[i];
            var st = GetCheckSum(data);
            return  sum == GetCheckSum(data);

        }
        private int GetCheckSum(Memory<byte> data)
        {
            return BitConverter.ToUInt16(new byte[]{data.Span[data.Length-1], data.Span[data.Length-2]});
        }
    }
}