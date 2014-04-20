using System.Collections;
using EmbeddedWebserver.Core.Handlers.Abstract;
using EmbeddedWebserver.Core.Helpers;

namespace EmbeddedWebserver.Core.Handlers
{
    internal sealed class DiagnosticsHandler : HandlerBase
    {
        #region Non-public members

        private static void _serializeToRow(StringBuilder pResponseBuilder, string pKey, string pValue)
        {
            pResponseBuilder.Append("<tr><td>");
            pResponseBuilder.Append(pKey);
            pResponseBuilder.Append(":</td><td>");
            pResponseBuilder.Append(pValue);
            pResponseBuilder.Append("</td></tr>");
        }

        private static void _serializeToTable(StringBuilder pResponseBuilder, StringDictionary pSourceData, string pUrlPrefix)
        {
            if (pSourceData != null && pSourceData.Count > 0)
            {
                foreach (string key in pSourceData.Keys)
                {
                    _serializeToRow(pResponseBuilder, pUrlPrefix.IsNullOrEmpty() ? key : "<a href=\"" + pUrlPrefix + key + "\">" + key + "</a>", pSourceData[key]);
                }
            }
        }

        private static void _serializeToTable(StringBuilder pResponseBuilder, StringDictionary pSourceData)
        {
            _serializeToTable(pResponseBuilder, pSourceData, null);
        }

        private static void _serializeToTable(StringBuilder pResponseBuilder, IEnumerable pSourceData)
        {
            if (pSourceData != null)
            {
                int index = 1;
                foreach (var item in pSourceData)
                {
                    _serializeToRow(pResponseBuilder, "#" + index.ToString(), (string)item);
                    index++;
                }
            }
        }

        protected override void ProcessRequestWorker(HttpContext pContext)
        {
            WebserverDiagnostics diag = pContext.Server.GetServerDiagnostics();
            StringBuilder responseBuilder = new StringBuilder(HandlerBase.HtmlDoctype, 1700);
            responseBuilder.Append("<html><head><title>Webserver diagnostics</title></head><body><h1>Webserver diagnostics</h1><table><tr><td colspan=\"2\"><b>Server information</b></td></tr>");
            _serializeToRow(responseBuilder, "Version", diag.Version);
            _serializeToRow(responseBuilder, "Worker thread count", diag.WorkerThreadCount.ToString());
            _serializeToRow(responseBuilder, "Uptime", diag.Uptime.ToString());
            responseBuilder.Append("<tr><td colspan=\"2\"><b>Application configuration</b></td></tr>");
            _serializeToTable(responseBuilder, diag.Configuration);
            responseBuilder.Append("<tr><td colspan=\"2\"><b>Application statistics</b></td></tr>");
            _serializeToRow(responseBuilder, "Serviced request count", diag.ServicedRequestCount.ToString());
            _serializeToRow(responseBuilder, "Dropped request count", diag.DroppedRequestCount.ToString());
            responseBuilder.Append("<tr><td colspan=\"2\"><b>Registered modules</b></td></tr>");
            _serializeToTable(responseBuilder, diag.Modules);
            responseBuilder.Append("<tr><td colspan=\"2\"><b>Registered handlers</b></td></tr>");
            _serializeToTable(responseBuilder, diag.Handlers, "/");
            responseBuilder.Append("</table></body></html>");
            pContext.Response.ResponseBody = responseBuilder.ToString();
        }

        #endregion

        #region Constructors

        public DiagnosticsHandler() : base(HttpMethods.GET) { }

        #endregion
    }
}
