using System;
using EmbeddedWebserver.Core.Modules.Interfaces;

namespace EmbeddedWebserver.Core.Modules.Abstract
{
    public abstract class ModuleBase : IModule
    {
        #region IMiniModule members

        public virtual void OnAuthenticateRequest(HttpContext pContext, HttpEventArguments pEventArguments)
        {
            return;
        }

        public virtual void OnPreProcessRequest(HttpContext pContext, HttpEventArguments pEventArguments)
        {
            return;
        }

        public virtual void OnPostProcessRequest(HttpContext pContext, HttpEventArguments pEventArguments)
        {
            return;
        }

        public virtual void OnError(HttpContext pContext, Exception pException)
        {
            return;
        }

        #endregion
    }
}
