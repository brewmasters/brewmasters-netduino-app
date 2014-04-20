using System;
using System.Collections;

namespace EmbeddedWebserver.Core.Helpers
{
    public sealed class StringBuilder
    {
        #region Non-public members

        private char[] _buffer;

        private int _bufferPosition;

        private int _bufferSizeStep = 100;

        private void _ensureSize(int pRemainingCharacterCount)
        {
            int targetedBufferSize = _bufferPosition + pRemainingCharacterCount;
            if (_buffer == null)
            {
                _buffer = new char[Math.Max(targetedBufferSize, _bufferSizeStep)];
            }
            else if (_buffer.Length < targetedBufferSize)
            {
#if MF_FRAMEWORK_VERSION_V4_1
                Microsoft.SPOT.Debug.GC(true);
#endif
                char[] newBuffer = new char[_buffer.Length * 2];
                if (_bufferPosition > 0)
                {
                    Array.Copy(_buffer, newBuffer, _bufferPosition);
                }
                _buffer = newBuffer;
            }
        }

        #endregion

        #region Public members

        public void Append(string pValue)
        {
            Char[] charArray = pValue.ToCharArray();
            Append(charArray, 0, charArray.Length);
        }

        public void AppendLine()
        {
            Append("\r\n");
        }

        public void AppendLine(string pValue)
        {
            Append(pValue);
            AppendLine();
        }

        public void Append(char[] pValue, int pStartIndex, int pCharCount)
        {
            _ensureSize(pCharCount);
            Array.Copy(pValue, pStartIndex, _buffer, _bufferPosition, pCharCount);
            _bufferPosition += pCharCount;
        }

        public int Length
        {
            get
            {
                return _bufferPosition;
            }
        }

        public int Capacity
        {
            get
            {
                return (_buffer != null) ? _buffer.Length : 0;
            }
        }

        public override string ToString()
        {
            return (_buffer != null) ? new string(_buffer, 0, _bufferPosition) : null;
        }

        #endregion

        #region Constructors

        public StringBuilder() { }

        public StringBuilder(int pInitialCapacity) 
        {
            _ensureSize(pInitialCapacity);
        }

        public StringBuilder(string pValue)
            : base()
        {
            Append(pValue);
        }

        public StringBuilder(string pValue, int pInitialCapacity): this(pInitialCapacity)
        {
            Append(pValue);
        }

        #endregion
    }
}
