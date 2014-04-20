using System;

namespace EmbeddedWebserver.Core.Configuration
{    
    public sealed class ConfigurationSyntaxErrorException : Exception
    {
        #region Non-public members

        public string IllegalValue { get; private set; }
        
        #endregion

        #region Constructors

        public ConfigurationSyntaxErrorException(string pIllegalValue) { }

        public ConfigurationSyntaxErrorException(string pIllegalValue, string pMessage) : this(pIllegalValue, pMessage, null) { }

        public ConfigurationSyntaxErrorException(string pIllegalValue, string pMessage, Exception pInnerException) : base(pMessage, pInnerException) 
        {
            IllegalValue = pIllegalValue;
        }

        #endregion
    }
}
