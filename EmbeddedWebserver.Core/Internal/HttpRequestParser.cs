using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using EmbeddedWebserver.Core.Helpers;

namespace EmbeddedWebserver.Core.Internal
{
    internal static class HttpRequestParser
    {
        #region Non-public members

        private const int _maxBufferedRequestLength = 1024;

        private static readonly char[] _queryStringTokenSeparator = new char[] { '=' };

        private static readonly char[] _requestTokenSeparator = new char[] { ' ' };

        private static readonly char[] _headerTokenSeparator = new char[] { ':' };

        private static HttpMethods _parseMethod(string pMethodString)
        {
            switch (pMethodString)
            {
                case "OPTIONS":
                    return HttpMethods.OPTIONS;
                case "GET":
                    return HttpMethods.GET;
                case "HEAD":
                    return HttpMethods.HEAD;
                case "POST":
                    return HttpMethods.POST;
                case "PUT":
                    return HttpMethods.PUT;
                case "DELETE":
                    return HttpMethods.DELETE;
                case "TRACE":
                    return HttpMethods.TRACE;
                case "CONNECT":
                    return HttpMethods.CONNECT;
                default:
                    throw new HttpException(HttpErrorCodes.HttpRequestParseError);
            }
        }

        private static void _parseRequestParameters(string pParameterString, StringDictionary pTargetCollection, IHttpServerUtility pServerUtility, bool pHttpDecode)
        {
            if (pTargetCollection == null)
            {
                throw new ArgumentNullException("pTargetCollection");
            }
            if (pServerUtility == null)
            {
                throw new ArgumentNullException("pServerUtility");
            }
            if (pParameterString.IsNullOrEmpty())
            {
                return;
            }
            string[] tokens = pParameterString.Split('&');
            foreach (string token in tokens)
            {
                string[] tokenParts = token.Split(_queryStringTokenSeparator, 2);
                string value = tokenParts.Length == 2 ? tokenParts[1] : null;
                if (value != null && pHttpDecode)
                {
                    value = pServerUtility.HtmlDecode(value);
                }
                pTargetCollection.Add(tokenParts[0], value);
            }
        }

        internal static StringDictionary ParseParameters(string pParameterString, IHttpServerUtility pServerUtility)
        {
            StringDictionary retval = new StringDictionary();
            _parseRequestParameters(pParameterString, retval, pServerUtility, true);
            return retval;
        }

        #endregion

        #region Public members

        public const string HEADER_ContentType = "Content-Type";

        public static HttpRequest ParseRequest(Socket pRequestSocket, IHttpServerUtility pServerUtility)
        {
            if (pServerUtility == null)
            {
                throw new ArgumentNullException("pServerUtility");
            }

            // Get request
            int bytesReceived = pRequestSocket.Available;
            bool doBuffered = (bytesReceived <= _maxBufferedRequestLength);
            HttpRequest retval = new HttpRequest(pRequestSocket, doBuffered);

            // User host
            IPEndPoint clientIP = pRequestSocket.RemoteEndPoint as IPEndPoint;
            retval.UserHostAddress = clientIP.Address.ToString();

            // Get read buffer
            byte[] readBuffer = new byte[_maxBufferedRequestLength];
            int readBufferLength = pRequestSocket.Receive(readBuffer, readBuffer.Length, SocketFlags.None);

            using (Helpers.StringReader requestStreamReader = new Helpers.StringReader(readBuffer, readBufferLength))
            {
                // Read method + url + query string
                string currentLine = requestStreamReader.ReadLine();

                string[] splitLine = currentLine.Split(_requestTokenSeparator, 3);
                retval.Method = _parseMethod(splitLine[0]);

                string fullUrl = splitLine[1].Trim();
                string trimmedUrl = fullUrl;
                int queryStartIndex = trimmedUrl.IndexOf('?');
                if (queryStartIndex > 0)
                {
                    trimmedUrl = trimmedUrl.Substring(0, queryStartIndex);
                    _parseRequestParameters(fullUrl.Substring(queryStartIndex + 1), retval.QueryString, pServerUtility, false);
                }
                retval.Url = pServerUtility.UrlDecode(trimmedUrl.Trim('/').ToLower());

                // Read headers
                string headerName = null;
                string headerValue = null;
                while (! (currentLine = requestStreamReader.ReadLine()).IsNullOrEmpty())
                {
                    splitLine = currentLine.Split(_headerTokenSeparator, 2);
                    headerName = splitLine[0].Trim();
                    headerValue = splitLine[1].Trim();
                    retval.RequestHeaders.Add(headerName, headerValue);
                }

                if (doBuffered)
                {
                    // Read content
                    retval.RequestBody = requestStreamReader.ReadToEnd();
                }
                else
                {
                    retval.BytesPrefix = requestStreamReader.ReadRemainingBytes();
                }
            }

            return retval;
        }

        #endregion
    }
}
