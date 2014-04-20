using System;
using Microsoft.SPOT;


using EmbeddedWebserver.Core;
using EmbeddedWebserver.Core.Handlers.Abstract;

namespace Brewmasters
{
    class RandomHandler : HandlerBase
    {
        #region Non-public members

        private static Random _generator = new Random();

        protected override void ProcessRequestWorker(HttpContext pContext)
        {
            int retval = _generator.Next();
            pContext.Response.ResponseBody = "{ \"value\": " + retval + "}";
            pContext.Response.ContentType = "application/json";
        }

        #endregion

        #region Constructors

        public RandomHandler() : base(HttpMethods.GET) { }

        #endregion
    }
}
