using System;
using System.IO;
using System.Net.Sockets;
using EmbeddedWebserver.Core.Helpers;

namespace EmbeddedWebserver.Core
{
    public sealed class HttpResponse : HttpObjectBase
    {
        #region Non-public members

        private string _responseBody = null;

        private Stream _responseStream = null;

        internal void DisposeResponseStream()
        {
            if (_responseStream != null)
            {
                _responseStream.Close();
                _responseStream.Dispose();
            }
            _responseBody = null;
        }

        #endregion

        #region Public members

        public HttpStatusCodes StatusCode { get; set; }

        public string ResponseBody
        {
            get
            {
                return _responseBody;
            }
            set
            {
                DisposeResponseStream();
                _responseBody = value;
            }
        }

        public Stream ResponseStream
        {
            get
            {
                return _responseStream;
            }
            set
            {
                DisposeResponseStream();
                _responseBody = null;
                _responseStream = value;
            }
        }

        public bool HasResponse { get { return !(ResponseBody.IsNullOrEmpty() && ResponseStream == null); } }

        public bool BufferedResponse { get { return ResponseStream == null; } }

        public string ContentType { get; set; }

        public StringDictionary ResponseHeaders { get; private set; }

        public void Redirect(string pRedirectUrl)
        {
            if (pRedirectUrl.IsNullOrEmpty())
            {
                throw new ArgumentNullException("pRedirectUrl");
            }
            StatusCode = HttpStatusCodes.Redirect;
            ResponseBody = null;
            ResponseHeaders.Clear();
            ResponseHeaders.Add("Location", Context.Server.UrlEncode(pRedirectUrl));
        }

        #endregion

        #region Constructors

        public HttpResponse(Socket pRequestSocket): base(pRequestSocket)
        {
            ResponseHeaders = new StringDictionary();
            StatusCode = HttpStatusCodes.OK;
            ContentType = "text/html";
        }

        #endregion
    }
}
