using System;
using EmbeddedWebserver.Core.Helpers;

namespace EmbeddedWebserver.Core
{
    public sealed class WebserverDiagnostics
    {
        #region Public members

        public string Version { get; internal set; }

        public int WorkerThreadCount { get; internal set; }

        public TimeSpan Uptime { get; internal set; }

        public StringDictionary Configuration { get; internal set; }

        public int DroppedRequestCount { get; internal set; }

        public int ServicedRequestCount { get; internal set; }

        public StringDictionary Handlers { get; internal set; }

        public string[] Modules { get; internal set; }

        #endregion
    }
}
