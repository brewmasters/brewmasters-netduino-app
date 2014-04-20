using System;
using System.IO;
using System.Net.Sockets;
using EmbeddedWebserver.Core.Helpers;
using EmbeddedWebserver.Core.Internal;

namespace EmbeddedWebserver.Core
{
    public sealed class HttpRequest : HttpObjectBase
    {
        #region Non-public members

        internal byte[] BytesPrefix = null;

        #endregion

        #region Public members

        public HttpMethods Method { get; internal set; }

        public string Url { get; internal set; }

        public string UserHostAddress { get; internal set; }

        public string RequestBody { get; internal set; }

        public StringDictionary RequestHeaders { get; private set; }

        public StringDictionary QueryString { get; private set; }

        public StringDictionary GetPostRequestParameters()
        {
            if (!BufferedRequest)
            {
                throw new NotSupportedException("Post parameter parsing is not supported in case of bufferless requests");
            }
            return HttpRequestParser.ParseParameters(RequestBody, Context.Server);
        }

        public bool BufferedRequest { get; private set; }

        public Stream GetBufferlessInputStream()
        {
            return new RequestStream(RequestSocket, BytesPrefix);
        }

        #endregion

        #region Constructors

        public HttpRequest(Socket pRequestSocket) : this(pRequestSocket, true) { }

        public HttpRequest(Socket pRequestSocket, bool pBufferedRequest)
            : base(pRequestSocket)
        {
            BufferedRequest = pBufferedRequest;
            RequestHeaders = new StringDictionary();
            QueryString = new StringDictionary();
        }

        #endregion
    }
}
