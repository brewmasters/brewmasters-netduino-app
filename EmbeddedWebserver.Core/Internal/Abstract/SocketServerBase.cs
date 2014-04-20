using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using EmbeddedWebserver.Core.Helpers;

namespace EmbeddedWebserver.Core.Internal.Abstract
{
    public abstract class SocketServerBase : IDisposable
    {
        #region Non-public members

        private ushort _listenerPort;

        private ushort _maxWorkerThreadCount;

        private Socket _listenerSocket = null;

        private Queue _requestQueue = new Queue();

        private ArrayList _workerThreadPool = new ArrayList();

        private Thread _listenerThread = null;

        private DateTime _startTime = default(DateTime);

        private int _droppedRequestCount = 0;

        private int _servicedRequestCount = 0;

        private void _workerThreadMethod()
        {
            int retryCounter = 0;
            while (_requestQueue.Count > 0 || retryCounter < 10)
            {
                Socket requestSocket = null;
                lock (_requestQueue)
                {
                    if (_requestQueue.Count > 0)
                    {
                        requestSocket = (Socket)_requestQueue.Dequeue();
                        retryCounter = 0;
                    }
                    else
                    {
                        retryCounter++;
                    }
                }

                if (requestSocket != null)
                {
                    if (requestSocket.Available > 0)
                    {
                        DebugHelper.Print("Processing request");
                        try
                        {
                            ProcessRequest(requestSocket);
                        }
                        catch (Exception) { }
                        finally
                        {
                            Interlocked.Increment(ref _servicedRequestCount);
                        }
                    }
                    else
                    {
                        Interlocked.Increment(ref _droppedRequestCount);
                    }

                    requestSocket.Close();
                    requestSocket = null;
                }
                Thread.Sleep(100);
            }

            lock (_workerThreadPool)
            {
                _workerThreadPool.Remove(Thread.CurrentThread);
            }
            DebugHelper.Print("Worker thread stopped");
        }

        private void _listenerThreadMethod()
        {
            while (true)
            {
                Socket requestSocket = null;
                try
                {
                    requestSocket = _listenerSocket.Accept();
                    if (requestSocket == null)
                    {
                        continue;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                lock (_requestQueue)
                {
                    _requestQueue.Enqueue(requestSocket);
                }
                if (_workerThreadPool.Count < _maxWorkerThreadCount)
                {
                    lock (_workerThreadPool)
                    {
                        if (_workerThreadPool.Count < _maxWorkerThreadCount)
                        {
                            Thread workerThread = new Thread(new ThreadStart(_workerThreadMethod));
                            _workerThreadPool.Add(workerThread);
                            workerThread.Start();
                            DebugHelper.Print("Worker thread started");
                        }
                    }
                }
            }
        }

        protected int WorkerThreadCount { get { return _workerThreadPool.Count; } }

        protected TimeSpan GetUptime()
        {
            if (IsListening)
            {
                return DateTime.Now - _startTime;
            }
            else
            {
                return TimeSpan.FromTicks(0);
            }
        }

        protected int DroppedRequestCount { get { return _droppedRequestCount; } }

        protected int ServicedRequestCount { get { return _servicedRequestCount; } }

        protected abstract void ProcessRequest(Socket pRequestSocket);

        #endregion

        #region Public members

        #region IDisposable members

        protected virtual void DisposeWorker() { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool pDisposing)
        {
            if (pDisposing)
            {
                StopListening();
                DisposeWorker();
            }
        }

        public bool IsListening { get; private set; }

        #endregion

        public void StartListening()
        {
            DebugHelper.Print("Socket server starting");
            _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenerSocket.Bind(new IPEndPoint(IPAddress.Any, _listenerPort));
            DebugHelper.Print("Listener socket bound to port: " + _listenerPort);
            _listenerSocket.Listen(30);
            DebugHelper.Print("Socket listening");

            _listenerThread = new Thread(new ThreadStart(_listenerThreadMethod));
            _startTime = DateTime.Now;
            _servicedRequestCount = 0;
            _droppedRequestCount = 0;
            _listenerThread.Start();
            DebugHelper.Print("Listener thread started");
            IsListening = true;
        }

        public void StopListening()
        {
            DebugHelper.Print("Server stopping");
            if (_listenerThread != null)
            {
                try
                {
                    _listenerThread.Abort();
                }
                catch (ThreadAbortException) { }
                _listenerThread = null;
            }

            IsListening = false;
            _startTime = default(DateTime);

            if (_workerThreadPool != null)
            {
                lock (_workerThreadPool)
                {
                    for (int i = 0; i < _workerThreadPool.Count; i++)
                    {
                        try
                        {
                            ((Thread)(_workerThreadPool[i])).Abort();
                        }
                        catch (ThreadAbortException) { }
                        _workerThreadPool[i] = null;
                    }
                }

                _workerThreadPool = null;
            }

            if (_listenerSocket != null)
            {
                _listenerSocket.Close();
                _listenerSocket = null;
            }

            if (_requestQueue != null && _requestQueue.Count > 0)
            {
                while (_requestQueue.Count > 0)
                {
                    Socket requestSocket = (Socket)_requestQueue.Dequeue();
                    requestSocket.Close();
                    requestSocket = null;
                }
            }
        }

        #endregion

        #region Constuctors

        public SocketServerBase(ushort pListenerPort, ushort pMaxWorkerThreadCount)
        {
            IsListening = false;
            _listenerPort = pListenerPort;
            _maxWorkerThreadCount = pMaxWorkerThreadCount;
        }

        ~SocketServerBase()
        {
            Dispose(false);
        }

        #endregion
    }
}
