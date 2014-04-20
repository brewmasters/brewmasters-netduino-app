using System;
using System.Net.Sockets;

namespace EmbeddedWebserver.Core
{
    public abstract class HttpObjectBase
    {
        #region Non-public members

        internal Socket RequestSocket { get; private set; }

        #endregion

        #region Public members

        public HttpContext Context { get; internal set; }

        #endregion

        #region Constructors

        public HttpObjectBase(Socket pRequestSocket)
        {
            if (pRequestSocket == null)
            {
                throw new ArgumentNullException("pRequestSocket");
            }
            RequestSocket = pRequestSocket;
        }

        #endregion
    }
}
