using System;

namespace EmbeddedWebserver.Core
{
    [Flags]
    public enum HttpMethods
    {
        OPTIONS = 1,
        GET = 2,
        HEAD = 4,
        POST = 8,
        PUT = 16,
        DELETE = 32,
        TRACE = 64,
        CONNECT = 128
    }
}