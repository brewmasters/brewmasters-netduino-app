using System;
using Microsoft.SPOT;

using EmbeddedWebserver.Core;
using EmbeddedWebserver.Core.Handlers.Abstract;

namespace Brewmasters
{
    class TestHandler : HandlerBase
    {
        #region Non-public members

        protected override void ProcessRequestWorker(HttpContext pContext)
        {
            pContext.Response.ResponseBody = "Hello world!";
        }

        #endregion

        #region Constructors

        public TestHandler() : base(HttpMethods.GET) { }

        #endregion
    }
}
