using System;

namespace EmbeddedWebserver.Core.Modules.Interfaces
{
    public interface IModule
    {
        #region Public members

        void OnAuthenticateRequest(HttpContext pContext, HttpEventArguments pEventArguments);

        void OnPreProcessRequest(HttpContext pContext, HttpEventArguments pEventArguments);

        void OnPostProcessRequest(HttpContext pContext, HttpEventArguments pEventArguments);

        void OnError(HttpContext pContext, Exception pException);

        #endregion
    }
}
