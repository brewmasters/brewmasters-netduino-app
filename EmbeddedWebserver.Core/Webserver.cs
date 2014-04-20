using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using EmbeddedWebserver.Core.Configuration;
using EmbeddedWebserver.Core.Handlers;
using EmbeddedWebserver.Core.Handlers.Interfaces;
using EmbeddedWebserver.Core.Helpers;
using EmbeddedWebserver.Core.Internal;
using EmbeddedWebserver.Core.Internal.Abstract;
using EmbeddedWebserver.Core.Modules;
using EmbeddedWebserver.Core.Modules.Interfaces;

namespace EmbeddedWebserver.Core
{
    public sealed class Webserver : SocketServerBase, IHttpServerUtility
    {
        #region Non-public members

        private const string _poweredByHeader = "EmbeddedWebserver/1.0.2a";

        private static string _mapStatusCodeToReason(HttpStatusCodes pStatusCode)
        {
            string retval = "";
            switch (pStatusCode)
            {
                case HttpStatusCodes.OK:
                    retval = "OK";
                    break;
                case HttpStatusCodes.Redirect:
                    retval = "Found";
                    break;
                case HttpStatusCodes.BadRequest:
                    retval = "Bad request";
                    break;
                case HttpStatusCodes.UnAuthorized:
                    retval = "Unauthorized";
                    break;
                case HttpStatusCodes.Forbidden:
                    retval = "Forbidden";
                    break;
                case HttpStatusCodes.NotFound:
                    retval = "Not found";
                    break;
                case HttpStatusCodes.MethodNotAllowed:
                    retval = "Method not allowed";
                    break;
                case HttpStatusCodes.InternalServerError:
                    retval = "Internal server error";
                    break;
                case HttpStatusCodes.ServiceUnavailable:
                    retval = "Service unavailable";
                    break;
            }
            return retval;
        }

        private static FileSystemHandler _defaultHandler = new FileSystemHandler();

        private readonly ArrayList _modules = new ArrayList();

        private readonly Hashtable _handlers = new Hashtable();

        private IHandler _resolveHandler(HttpContext pContext)
        {
            if (FileSystemHandler.CanHandleRequest(pContext))
            {
                return _defaultHandler;
            }
            else
            {
                string targetUri = pContext.Request.Url;
                if (_handlers.Contains(targetUri))
                {
                    object handlerItem = _handlers[targetUri];
                    if (handlerItem is Type)
                    {
                        return (IHandler)EmbeddedWebserver.Core.Helpers.Activator.CreateInstance((Type)_handlers[targetUri]);
                    }
                    else
                    {
                        return (IHandler)handlerItem;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        private void _registerHandler(string pAccessUri, object pHandlerItem)
        {
            if (pAccessUri.IsNullOrEmpty())
            {
                throw new ArgumentNullException("pAccessUri");
            }
            if (IsListening)
            {
                throw new HttpException(HttpErrorCodes.ChangesNotAllowedWhileListening);
            }

            _handlers.Add(pAccessUri.Trim('/').ToLower(), pHandlerItem);
        }

        protected override void DisposeWorker()
        {

        }

        protected override void ProcessRequest(Socket pRequestSocket)
        {
#if MF_FRAMEWORK_VERSION_V4_1
            Microsoft.SPOT.Debug.GC(true);
#endif

            HttpRequest request = null;
            bool badRequest = false;
            try
            {
                request = HttpRequestParser.ParseRequest(pRequestSocket, this);
            }
            catch (Exception)
            {
                badRequest = true;
            }
            finally
            {
                if (badRequest)
                {
                    request = new HttpRequest(pRequestSocket);
                }
            }
            HttpContext context = new HttpContext(request, this);
            context.Request.Context = context;
            context.Response.Context = context;

            if (badRequest)
            {
                context.Response.StatusCode = HttpStatusCodes.BadRequest;
                context.Response.ResponseBody = null;
            }
            else
            {
                try
                {
                    IHandler handler = _resolveHandler(context);
                    if (handler == null)
                    {
                        context.Response.StatusCode = HttpStatusCodes.NotFound;
                    }
                    else
                    {
                        HttpEventArguments eventArguments = new HttpEventArguments();

                        // Modules: Authenticate
                        foreach (IModule module in _modules)
                        {
                            module.OnAuthenticateRequest(context, eventArguments);
                            if (eventArguments.CancelPipeline)
                            {
                                break;
                            }
                        }

                        if (!eventArguments.CancelPipeline)
                        {
                            // Modules: Pre process
                            foreach (IModule module in _modules)
                            {
                                module.OnPreProcessRequest(context, eventArguments);
                                if (eventArguments.CancelPipeline)
                                {
                                    break;
                                }
                            }

                            // Handler: Process
                            if (!eventArguments.CancelPipeline)
                            {
                                handler.ProcessRequest(context);
                            }

                            // Modules: Post process
                            foreach (IModule module in _modules)
                            {
                                module.OnPostProcessRequest(context, eventArguments);
                                if (eventArguments.CancelPipeline)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = HttpStatusCodes.InternalServerError;
                    context.Response.ResponseBody = null;
                    foreach (IModule module in _modules)
                    {
                        module.OnError(context, ex);
                    }
                }
            }

            // Response header
            StringBuilder headerBuilder = new StringBuilder("HTTP/1.1 ", 200);
            headerBuilder.Append(((int)context.Response.StatusCode).ToString());
            headerBuilder.Append(" ");
            headerBuilder.AppendLine(_mapStatusCodeToReason(context.Response.StatusCode));
            context.Response.ResponseHeaders[HttpRequestParser.HEADER_ContentType] = context.Response.ContentType;
            bool hasResponse = context.Response.HasResponse;
            long responseLength = 0;
            byte[] sendBuffer = null;
            if (hasResponse)
            {
                if (context.Response.BufferedResponse)
                {
                    sendBuffer = System.Text.Encoding.UTF8.GetBytes(context.Response.ResponseBody);
                }
                responseLength = context.Response.BufferedResponse ? sendBuffer.Length : context.Response.ResponseStream.Length;
            }
            context.Response.ResponseHeaders["Content-Length"] = responseLength.ToString();
            context.Response.ResponseHeaders["Connection"] = "close";
            context.Response.ResponseHeaders["X-Powered-By"] = _poweredByHeader;

            if (context.Response.ResponseHeaders.Count > 0)
            {
                foreach (var responseHeaderKey in context.Response.ResponseHeaders.Keys)
                {
                    headerBuilder.Append((string)responseHeaderKey);
                    headerBuilder.Append(": ");
                    headerBuilder.AppendLine(context.Response.ResponseHeaders[(string)responseHeaderKey]);
                }
            }
            headerBuilder.AppendLine();
            pRequestSocket.Send(System.Text.Encoding.UTF8.GetBytes(headerBuilder.ToString()), headerBuilder.Length, SocketFlags.None);

            // Response content
            if (hasResponse)
            {
                // Buffered response
                if (context.Response.BufferedResponse)
                {
                    pRequestSocket.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
                }
                // Streamed response                
                else
                {
                    sendBuffer = new byte[512];
                    int sendBytes = 0;
                    while ((sendBytes = context.Response.ResponseStream.Read(sendBuffer, 0, sendBuffer.Length)) > 0)
                    {
                        pRequestSocket.Send(sendBuffer, sendBytes, SocketFlags.None);
                    }
                    context.Response.DisposeResponseStream();
                }
            }

#if MF_FRAMEWORK_VERSION_V4_1
            Microsoft.SPOT.Debug.GC(true);
#endif
        }

        #endregion

        #region Public members

        #region IHttpServerUtility members

        public EmbeddedWebapplicationConfiguration ApplicationConfiguration
        {
            get;
            private set;
        }

        public string MapPath(string pRelativePath)
        {
            return Path.Combine(ApplicationConfiguration.Path, pRelativePath.Replace("/", "\\"));
        }

        public string MapContentType(string pExtension)
        {
            String retval = "application/octet-stream";
            if (!pExtension.IsNullOrEmpty())
            {
                switch (pExtension.ToLower())
                {
                    case ".htm":
                    case ".html":
                        retval = "text/html";
                        break;
                    case ".js":
                        retval = "text/javascript";
                        break;
                    case ".css":
                        retval = "text/css";
                        break;
                    case ".gif":
                        retval = "image/gif";
                        break;
                    case ".jpg":
                        retval = "image/jpeg";
                        break;
                    case ".png":
                        retval = "image/png";
                        break;
                    case ".bmp":
                        retval = "image/bmp";
                        break;
                }
            }
            return retval;
        }

        public string UrlEncode(string pSourceString)
        {
            return pSourceString.Replace(" ", "%20");
        }

        public string UrlDecode(string pSourceString)
        {
            return pSourceString.Replace("%20", " ");
        }

        public string HtmlEncode(string pSourceString)
        {
            string retval = pSourceString.Replace("/", "%2F");
            retval = retval.Replace(" ", "+");
            return retval;
        }

        public string HtmlDecode(string pSourceString)
        {
            string retval = pSourceString.Replace("%2F", "/");
            retval = retval.Replace("+", " ");
            return retval;
        }

        public WebserverDiagnostics GetServerDiagnostics()
        {
            WebserverDiagnostics retval = new WebserverDiagnostics();
            retval.Version = _poweredByHeader;
            retval.WorkerThreadCount = base.WorkerThreadCount;
            retval.Uptime = base.GetUptime();
            retval.Configuration = ApplicationConfiguration;
            retval.DroppedRequestCount = base.DroppedRequestCount;
            retval.ServicedRequestCount = base.ServicedRequestCount;
            retval.Handlers = new StringDictionary();
            foreach (string handlerUrl in _handlers.Keys)
            {
                object handlerItem = _handlers[handlerUrl];
                retval.Handlers.Add(handlerUrl, ((handlerItem is Type) ? (Type)handlerItem : handlerItem.GetType()).FullName);
            }
            retval.Modules = new string[_modules.Count];
            for (int moduleIndex = 0; moduleIndex < _modules.Count; moduleIndex++)
            {
                retval.Modules[moduleIndex] = _modules[moduleIndex].GetType().FullName;
            }

            return retval;
        }

        #endregion

        public void RegisterModule(IModule pModule)
        {
            if (pModule == null)
            {
                throw new ArgumentNullException("pModule");
            }
            if (IsListening)
            {
                throw new HttpException(HttpErrorCodes.ChangesNotAllowedWhileListening);
            }
            _modules.Add(pModule);
        }

        public void RegisterHandler(string pAccessUri, Type pHandlerType)
        {
            if (pHandlerType == null)
            {
                throw new ArgumentNullException("pHandler");
            }

            bool validHandlerType = false;
            Type parentType = pHandlerType;
            while (parentType != null)
            {
                foreach (Type interfaceType in parentType.GetInterfaces())
                {
                    if (interfaceType == typeof(IHandler))
                    {
                        validHandlerType = true;
                        break;
                    }
                }
                if (validHandlerType)
                {
                    break;
                }
                else
                {
                    parentType = parentType.BaseType;
                }
            }

            if (!validHandlerType)
            {
                throw new ArgumentException("Invalid handler", "pHandlerType");
            }
            _registerHandler(pAccessUri, pHandlerType);
        }

        public void RegisterThreadSafeHandler(string pAccessUri, IHandler pHandlerInstance)
        {
            if (pHandlerInstance == null)
            {
                throw new ArgumentNullException("pHandlerInstance");
            }
            _registerHandler(pAccessUri, pHandlerInstance);
        }

        #endregion

        #region Constructors

        public Webserver(EmbeddedWebapplicationConfiguration pConfiguration)
            : base(pConfiguration.Port, pConfiguration.MaxWorkerThreadCount)
        {
            ApplicationConfiguration = pConfiguration;
            RegisterThreadSafeHandler("diag", new DiagnosticsHandler());
            if (ApplicationConfiguration.RequestFilterModuleEnabled)
            {
                RegisterModule(new RequestFilterModule(ApplicationConfiguration.RequestFilterModuleAllowMask));
            }
        }

        #endregion
    }
}
