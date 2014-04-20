using System;

namespace EmbeddedWebserver.Core
{
    public enum HttpErrorCodes
    {
        ChangesNotAllowedWhileListening = 20,
        HttpRequestParseError = 30,
    }

    public class HttpException : Exception
    {
        #region Public members

        public HttpErrorCodes ErrorCode { get; private set; }

        #endregion

        #region Constructors

        public HttpException(HttpErrorCodes pErrorCode) : this(pErrorCode, null, null) { }

        public HttpException(HttpErrorCodes pErrorCode, string pMessage) : this(pErrorCode, pMessage, null) { }

        public HttpException(HttpErrorCodes pErrorCode, string pMessage, Exception pInnerException)
            : base(pMessage, pInnerException)
        {
            ErrorCode = pErrorCode;
        }

        #endregion
    }
}
