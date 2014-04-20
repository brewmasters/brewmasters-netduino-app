using System;
using EmbeddedWebserver.Core.Helpers;
using EmbeddedWebserver.Core.Modules.Abstract;

namespace EmbeddedWebserver.Core.Modules
{
    internal sealed class RequestFilterModule : ModuleBase
    {
        #region Non-public members

        private static readonly char[] _hostSeparatorCharacter = new char[] { '.' };

        private const string _wildcardString = "*";

        private string[] _allowMask = null;

        #endregion

        #region Public members

        public override void OnAuthenticateRequest(HttpContext pContext, HttpEventArguments pEventArguments)
        {
            string[] requestAddressTokens = pContext.Request.UserHostAddress.Split(_hostSeparatorCharacter, 4);
            for (int i = 0; i < 4; i++)
            {
                if (_allowMask[i] != _wildcardString && requestAddressTokens[i] != _allowMask[i])
                {
                    pContext.Response.ResponseBody = null;
                    pContext.Response.StatusCode = HttpStatusCodes.Forbidden;
                    pEventArguments.CancelPipeline = true;
                    break;
                }
            }
            return;
        }

        #endregion

        #region Constructors

        public RequestFilterModule(string pAllowMask)
        {
            if (pAllowMask.IsNullOrEmpty())
            {
                throw new ArgumentNullException("pAllowMask");
            }
            string[] allowMaskTokens = pAllowMask.Split(_hostSeparatorCharacter, 4);
            if (allowMaskTokens.Length != 4)
            {
                throw new ArgumentException("Invalid allow mask", "pAllowMask");
            }
            _allowMask = allowMaskTokens;
        }

        #endregion
    }
}
