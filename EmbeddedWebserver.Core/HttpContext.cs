using System;

namespace EmbeddedWebserver.Core
{
    public sealed class HttpContext
    {
        #region Members

        public HttpRequest Request { get; private set; }

        public HttpResponse Response { get; private set; }

        public IHttpServerUtility Server { get; private set; }

        #endregion

        #region Constructors
        
        internal HttpContext(HttpRequest pRequest, IHttpServerUtility pServer)
        {
            if (pRequest == null)
            {
                throw new ArgumentNullException("pRequest");
            }
            if (pServer == null)
            {
                throw new ArgumentNullException("pServer");
            }            
            Request = pRequest;
            Server = pServer;
            Response = new HttpResponse(pRequest.RequestSocket);
        }

        #endregion
    }
}
