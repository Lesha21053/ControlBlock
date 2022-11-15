using System;
using System.IO.Ports;
using System.Threading;
using System.Timers;
using ControlBlock.Interfaces;
using ControlBlock.Models.Enums;
using ControlBlock.Models.Exceptions;
using ControlBlock.Models.Settings;

namespace ControlBlock.COM
{
    public class COMPort:IPort
    {
        private SerialPort _comPort;
        private COMPortSettings _settings;
        private System.Timers.Timer _timeOutTimer;
        private bool _inputListen = false;
        private bool _timeoutFlag = false;
        private bool _ready = false;
        private readonly object portLock = new object();
        private IPort _portImplementation;


        
        public event IPort.PortDataRecived DataReceived;

        public bool InputIsListen
        {
            get { return _inputListen; }
            set
            {
                if (value)
                    _comPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                else
                    _comPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);

                _inputListen = value;
            }
        }

        public bool Ready
        {
            get
            {
                return _ready;
            }
        }
        public COMPort(COMPortSettings settings)
        {
            _settings = settings;
            SerialPortInit();
        }

        public byte[] Send(byte[] data, bool answerIsRequired = false)
            {
                lock (portLock)
                {
                    try

                    {
                        BuffersDiscard();
                        _comPort.Write(data, 0, data.Length);

                        if (answerIsRequired)
                        {
                            byte[] rxDataBuffer = new byte[_settings.ReceivedBytesThreshold];
                            _timeOutTimer.Start();
                            while (_comPort.BytesToRead < data.Length)
                            {
                                Thread.Sleep(10);
                                if (_timeoutFlag)
                                    throw new TimeoutException("No response received");
                            }
                            _timeOutTimer.Stop();
                            _comPort.Read(rxDataBuffer, 0, rxDataBuffer.Length);
                        }
                    }
                    catch (TimeoutException)
                    {
                        _timeoutFlag = false;
                       
                        throw new PortException($"COM Port: Transmission error TimeOut", PortStatusCode.RxAnswerTimeOut);
                    }
                    catch (Exception ex)
                    {
                        
                        throw new PortException($"COM Port: Transmission error {ex.Message}", PortStatusCode.TxError);
                    }

                    return null;
                }
            }
            
        private void DataReceivedHandler(
            object sender,
            SerialDataReceivedEventArgs e)
        {
            lock (portLock)
            {
                try
                {
                    SerialPort port = (SerialPort)sender;
                    var reciveBuffer = new byte[port.ReceivedBytesThreshold];
                    if (port.BytesToRead > port.ReceivedBytesThreshold)
                    {
                        port.Read(reciveBuffer, 0, reciveBuffer.Length);
                        DataReceived?.Invoke(reciveBuffer);
                    }
                }
                catch (Exception ex)
                {
                    throw new PortException($"COM Port {_comPort.PortName}: Recive error {ex.Message}", PortStatusCode.RxError);
                }
            }
        }
        private void SerialPortInit()
        {
            Dispose();

            _comPort = new SerialPort()
            {
                PortName = _settings.PortName,
                BaudRate = _settings.BaudRate,
                Parity = _settings.Parity,
                DataBits = _settings.DataBits,
                StopBits = _settings.StopBits,
                Handshake = _settings.Handshake,
                RtsEnable = _settings.RtsEnable,
                ReadBufferSize = _settings.ReadBufferSize,
                WriteBufferSize = _settings.WriteBufferSize,
                ReadTimeout = _settings.ReadTimeout,
                WriteTimeout = _settings.WriteTimeout,
                ReceivedBytesThreshold = _settings.ReceivedBytesThreshold
            };
            _timeOutTimer = new System.Timers.Timer(_settings.ReciveAnswerTimeOut);
            _timeOutTimer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
           

            try
            {
                _comPort.Open();
                _ready = true;
            }
            catch (Exception ex)
            {
                throw new PortException($"COM Port {_comPort} Initialization error: {ex.Message}", PortStatusCode.InitError);
            }
        }
        public void Dispose()
        {
            if (_comPort != null)
            {
                _ready = false;
                _comPort.Dispose();
            }
        }
        private void BuffersDiscard()
        {
            _comPort.DiscardOutBuffer();
            _comPort.DiscardInBuffer();
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timeoutFlag = true;
        }

    }
}