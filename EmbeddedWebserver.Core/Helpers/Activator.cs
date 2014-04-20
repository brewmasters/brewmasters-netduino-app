using System;
using System.Reflection;

namespace EmbeddedWebserver.Core.Helpers
{
    public static class Activator
    {
        #region Public members

        public static object CreateInstance(Type pTargetType)
        {
            if (pTargetType == null)
            {
                throw new ArgumentNullException("pTargetType");
            }
#if MF_FRAMEWORK_VERSION_V4_1
            ConstructorInfo constructor = (ConstructorInfo)(MethodBase)pTargetType.GetMethod(".ctor", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance);
#else
            ConstructorInfo constructor = pTargetType.GetConstructor(new Type[0]);
#endif


            object retval = constructor.Invoke(null);
            return retval;
        }

        #endregion
    }
}
