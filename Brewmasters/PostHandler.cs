using EmbeddedWebserver.Core;
using EmbeddedWebserver.Core.Handlers.Abstract;
using EmbeddedWebserver.Core.Helpers;

namespace Brewmasters
{
    class PostHandler : HandlerBase
    {
        #region Non-public members

        protected override void ProcessRequestWorker(HttpContext pContext)
        {
            StringDictionary dict = pContext.Request.GetPostRequestParameters();
            StringBuilder builder = new StringBuilder();
            DebugHelper.Print("--- params -----");
            dict.DebugPrint();

            DebugHelper.Print(pContext.Request.RequestBody);
            //DebugHelper.Print(pContext.Request.Context.Request.RequestBody);
            DebugHelper.Print("--- params end ----");
            builder.Append("{ \"key\": \"");
            builder.Append(dict.ContainsKey("key") ? dict["key"] : "null");
            builder.Append("\", ");
            builder.Append("\"value\": \"");
            builder.Append(dict.ContainsKey("value") ? dict["value"] : "null");
            builder.Append("\" }");
            pContext.Response.ResponseBody = builder.ToString();
            pContext.Response.ContentType = "application/json";
        }

        #endregion

        #region Constructors

        public PostHandler() : base(HttpMethods.POST) { }

        #endregion
    }
}
