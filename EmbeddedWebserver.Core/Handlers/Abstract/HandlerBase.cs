using EmbeddedWebserver.Core.Handlers.Interfaces;

namespace EmbeddedWebserver.Core.Handlers.Abstract
{
    public abstract class HandlerBase : IHandler
    {
        #region Non-public members

        public HttpMethods SupportedMethods { get; private set; }

        protected abstract void ProcessRequestWorker(HttpContext pContext);

        protected const string HtmlDoctype = "<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";

        #endregion

        #region Public members

        #region IHandler members

        public void ProcessRequest(HttpContext pContext)
        {
            if ((pContext.Request.Method & SupportedMethods) != 0)
            {
                pContext.Response.ResponseHeaders.Add("Cache-Control", "no-cache, max-age=0");
                ProcessRequestWorker(pContext);
                pContext.Response.StatusCode = HttpStatusCodes.OK;
            }
            else
            {
                pContext.Response.StatusCode = HttpStatusCodes.MethodNotAllowed;
                pContext.Response.ResponseBody = null;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public HandlerBase(HttpMethods pSupportedMethods)
        {
            SupportedMethods = pSupportedMethods;
        }

        #endregion
    }
}
