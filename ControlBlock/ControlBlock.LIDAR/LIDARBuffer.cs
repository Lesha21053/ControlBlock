using System;
using ControlBlock.Models.Exceptions;

namespace ControlBlock.LIDAR
{
    public class LIDARBuffer
    {
        private readonly int _byfferSize;
        private  Memory<byte> _buffer;
        private bool _circleFlag;
 

        public int ReadPointerPosition { get; set; } 
        public int WritePointerPosition { get; private set; }
        public Memory<byte> Buffer => _buffer;
        public LIDARBuffer(int byfferSize)
        {
            _buffer = new Memory<byte>(new byte[byfferSize]);
            _byfferSize = byfferSize;
        }
        
        public byte ReadByte()
        {
            while (ReadPointerPosition >= WritePointerPosition && !_circleFlag)
            {
            }

            var data = _buffer.Span[ReadPointerPosition];
            ReadPointerPosition = PointerIncrement(ReadPointerPosition);

            return data;
        }
        
        public void WriteByte(byte data)
        {
            _buffer.Span[WritePointerPosition] = data;
            WritePointerPosition = PointerIncrement(WritePointerPosition);
            if (WritePointerPosition >= ReadPointerPosition && _circleFlag)
                throw new BufferOverflowException("Buffer overflow" , _byfferSize);
        }

        public void Write(Memory<byte> dataArray)
        {
            for (var i = 0; i < dataArray.Length; i++)
                WriteByte(dataArray.Span[i]);
        }

        public void Read(Memory<byte> readBuffer, int startPosition, int count)
        {
            ReadPointerPosition = startPosition;
            for (var i = 0; i < count; i++)
                readBuffer.Span[i] = ReadByte();
        }

        public void Read(Memory<byte> readBuffer, int startTarget, int startSource, int count)
        {
            for (var i = 0; i < count; i++) readBuffer.Span[startTarget + i] = ReadByte();
        }

        public void Discard()
        {
            ReadPointerPosition = 0;
            WritePointerPosition = 0;
            _circleFlag = false;
            _buffer = new Memory<byte>(new byte[_byfferSize]);
        }
        
        private int PointerIncrement(int pointer)
        {
            if (pointer == _byfferSize - 1)
            {
                pointer = 0;
                _circleFlag = !_circleFlag;
            }
            else
            {
                pointer++;
            }

            return pointer;
        }
    }
}