
namespace EmbeddedWebserver.Core.Handlers.Interfaces
{
    public interface IHandler
    {
        #region Public members

        void ProcessRequest(HttpContext pContext);

        #endregion
    }
}
