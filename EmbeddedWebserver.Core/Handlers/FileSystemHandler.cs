using System.Collections;
using System.IO;
using EmbeddedWebserver.Core.Handlers.Abstract;
using EmbeddedWebserver.Core.Helpers;

namespace EmbeddedWebserver.Core.Handlers
{
    internal sealed class FileSystemHandler : HandlerBase
    {
        #region Non-public members

        private static readonly Hashtable _supportedTypes = new Hashtable() 
        {
            { ".htm", null },
            { ".html", null },
            { ".js", null },
            { ".css", null },
            { ".gif", null },
            { ".jpg", null },
            { ".png", null },
            { ".bmp", null }
        };

        private static string _buildTargetUrl(HttpContext pContext)
        {
            return pContext.Server.MapPath(pContext.Request.Url.TrimStart('/'));
        }

        internal static bool CanHandleRequest(HttpContext pContext)
        {
            if (pContext.Server.ApplicationConfiguration.Path.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                string targetUri = _buildTargetUrl(pContext);
                return ((pContext.Server.ApplicationConfiguration.FileSystemHandlerEnableDirectoryBrowsing && Directory.Exists(targetUri)) || (_supportedTypes.Contains(Path.GetExtension(targetUri).ToLower()) && File.Exists(_buildTargetUrl(pContext))));
            }
        }

        private static void _serveFileContent(string pTargetUri, HttpContext pContext)
        {
            pContext.Response.ResponseStream = File.Open(_buildTargetUrl(pContext), FileMode.Open, FileAccess.Read);
            pContext.Response.ContentType = pContext.Server.MapContentType(Path.GetExtension(pTargetUri));
        }

        private static void _buildDirectoryContentItem(StringBuilder pResponseBuilder, string pTargetUri, string pRequestUrl, FileSystemInfo pFileInfo, bool pIsDirectory, IHttpServerUtility pServerUtility)
        {
            pResponseBuilder.Append(pFileInfo.LastWriteTime.ToString("yyyy.MM.dd.    hh.mm."));
            pResponseBuilder.Append("        ");
            pResponseBuilder.Append((pIsDirectory ? StringHelper.PadRight("&lt;dir&gt;", 19) : StringHelper.PadRight(((FileInfo)pFileInfo).Length.ToString(), 13)));
            pResponseBuilder.Append(" <a href=\"");
            pResponseBuilder.Append(pServerUtility.UrlEncode(pRequestUrl + "/" + pFileInfo.Name));
            pResponseBuilder.Append("\">");
            pResponseBuilder.Append(pFileInfo.Name);
            pResponseBuilder.Append("</a><br>");
        }

        private static void _serveDirectoryContent(string pTargetUri, HttpContext pContext)
        {
            DirectoryInfo baseDir = new DirectoryInfo(pTargetUri);
            string renderPath = (pContext.Request.Url.IsNullOrEmpty() ? "" : "/") + pContext.Request.Url;
            StringBuilder responseBuilder = new StringBuilder(HandlerBase.HtmlDoctype);
            responseBuilder.Append("<html><head><title>");
            responseBuilder.Append(renderPath);
            responseBuilder.Append("/</title></head><body><h1>");
            responseBuilder.Append(renderPath);
            responseBuilder.Append("/</h1><hr><pre>");
            if (baseDir.FullName.Trim('\\') != pContext.Server.ApplicationConfiguration.Path.Trim('\\'))
            {
                int parentSeparatorIndex = pContext.Request.Url.LastIndexOf('/');
                string parentUri = "";
                if (parentSeparatorIndex > 0)
                {
                    parentUri = pContext.Request.Url.Substring(0, parentSeparatorIndex);
                }
                responseBuilder.Append("<a href=\"/");
                responseBuilder.Append(pContext.Server.UrlEncode(parentUri));
                responseBuilder.Append("\">[To Parent Directory]</a><br>");
            }
            foreach (DirectoryInfo dir in baseDir.GetDirectories())
            {
                _buildDirectoryContentItem(responseBuilder, pTargetUri, pContext.Request.Url, dir, true, pContext.Server);
            }
            foreach (FileInfo file in baseDir.GetFiles())
            {
                _buildDirectoryContentItem(responseBuilder, pTargetUri, pContext.Request.Url, file, false, pContext.Server);
            }
            responseBuilder.Append("</pre><hr></body></html>");
            pContext.Response.ResponseBody = responseBuilder.ToString();
        }

        protected override void ProcessRequestWorker(HttpContext pContext)
        {
            string targetUri = _buildTargetUrl(pContext);

            if (Directory.Exists(targetUri))
            {
                _serveDirectoryContent(targetUri, pContext);
            }
            else
            {
                _serveFileContent(targetUri, pContext);
            }
        }

        #endregion

        #region Constructors

        public FileSystemHandler() : base(HttpMethods.GET) { }

        #endregion
    }
}
