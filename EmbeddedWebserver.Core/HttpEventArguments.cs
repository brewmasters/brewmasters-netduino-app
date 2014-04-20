
namespace EmbeddedWebserver.Core
{
    public sealed class HttpEventArguments
    {
        #region Public members

        public bool CancelPipeline { get; set; }

        #endregion

        #region Constructors

        public HttpEventArguments()
        {
            CancelPipeline = false;
        }

        #endregion
    }
}
