using System;
using System.IO;
using System.Reflection;

namespace EmbeddedWebserver.Core.Helpers
{
    public static class PathHelper
    {
        #region Public members

        public static string ResolveToAbsolutePath(string pRelativePath)
        {
            if (pRelativePath.IsNullOrEmpty())
            {
                throw new ArgumentNullException("pRelativePath");
            }
            string retval = pRelativePath;
//#if !MF_FRAMEWORK_VERSION_V4_1
//            if (!Path.IsPathRooted(pRelativePath))
//            {
//                Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase), pRelativePath));
//                retval = uri.LocalPath;
//            }
//#endif
            return retval;
        }

        #endregion
    }
}
