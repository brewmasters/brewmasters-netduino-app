
namespace EmbeddedWebserver.Core
{
    public enum HttpStatusCodes
    {
        OK = 200,
        Redirect = 302,
        BadRequest = 400,
        UnAuthorized = 401,
        Forbidden = 403,
        NotFound = 404, 
        MethodNotAllowed = 405,
        InternalServerError = 500,
        ServiceUnavailable = 503,
    }
}
