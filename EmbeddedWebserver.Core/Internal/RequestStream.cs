using System;
using System.IO;
using System.Net.Sockets;

namespace EmbeddedWebserver.Core.Internal
{
    internal sealed class RequestStream : Stream
    {
        #region Non-public members

        private long _position = 0;

        private readonly long _length = 0;

        private readonly byte[] _bytesPrefix = null;

        private readonly Socket _requestSocket = null;

        #endregion

        #region Public members

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            return;
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] pBuffer, int pOffset, int pCount)
        {
            if (_position >= _length)
            {
                return 0;
            }

            int readBytes = 0;
            if (_bytesPrefix != null && _position < _bytesPrefix.Length)
            {
                int copyBytesFromPrefix = _bytesPrefix.Length - (int)_position;
                copyBytesFromPrefix = Math.Min(copyBytesFromPrefix, pCount);
                Array.Copy(_bytesPrefix, (int)_position, pBuffer, 0, copyBytesFromPrefix);
                _position += copyBytesFromPrefix;
                readBytes += copyBytesFromPrefix;
            }

            if (readBytes < pCount && _position < _length)
            {
                int copyBytesFromSocket = _requestSocket.Receive(pBuffer, readBytes, pCount - readBytes, SocketFlags.None);
                _position += copyBytesFromSocket;
                readBytes += copyBytesFromSocket;
            }

            return readBytes;
        }

        public override long Seek(long pOffset, SeekOrigin pOrigin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long pValue)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] pBuffer, int pOffset, int pCount)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Constructors

        public RequestStream(Socket pRequestSocket) : this(pRequestSocket, null) { }

        public RequestStream(Socket pRequestSocket, byte[] pBytesPrefix)
        {
            if (pRequestSocket == null)
            {
                throw new ArgumentNullException("pRequestSocket");
            }
            _requestSocket = pRequestSocket;
            _length = _requestSocket.Available;

            if (pBytesPrefix != null)
            {
                _bytesPrefix = pBytesPrefix;
                _length += pBytesPrefix.Length;
            }
        }

        #endregion
    }
}
