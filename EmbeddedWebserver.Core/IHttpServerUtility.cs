using EmbeddedWebserver.Core.Configuration;

namespace EmbeddedWebserver.Core
{
    public interface IHttpServerUtility
    {
        #region Public members

        EmbeddedWebapplicationConfiguration ApplicationConfiguration { get; }

        string MapPath(string pRelativePath);

        string MapContentType(string pExtension);

        string UrlEncode(string pSourceString);

        string UrlDecode(string pSourceString);

        string HtmlEncode(string pSourceString);

        string HtmlDecode(string pSourceString);

        WebserverDiagnostics GetServerDiagnostics();

        #endregion
    }
}
