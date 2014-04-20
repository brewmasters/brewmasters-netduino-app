using System;
using System.IO;
using EmbeddedWebserver.Core.Helpers;


namespace EmbeddedWebserver.Core.Configuration.Abstract
{
    public abstract class ConfigurationParserBase : StringDictionary
    {
        #region Non-public members

        private const char _commentPrefix = '#';

        private static readonly char[] _valueSeparators = new char[] { '=' };

        private static string _prunestring(string pSourcestring)
        {
            return pSourcestring.Trim();
        }

        private static string _resolveDefaultConfigurationPath(string pBasePath)
        {
            return Path.Combine(pBasePath, "config.ini");
        }

        private void _assertConfiguration(string pConfigurationKey, string pDefaultValue, bool pSetDefaultValue, out string pCurrentValue)
        {
            if (!this.ContainsKey(pConfigurationKey))
            {
                if (!pSetDefaultValue)
                {
                    throw new ConfigurationSyntaxErrorException(pConfigurationKey, "Missing required configuration key");
                }
                else
                {
                    this.Add(pConfigurationKey, (string)pDefaultValue);
                }
            }
            pCurrentValue = this[pConfigurationKey];
        }

        protected readonly string ConfigurationFileBasePath = null;

        protected void AssertConfiguration(string pConfigurationKey, out string pCurrentValue)
        {
            _assertConfiguration(pConfigurationKey, null, false, out pCurrentValue);
        }

        protected void AssertConfiguration(string pConfigurationKey, string pDefaultValue, out string pCurrentValue)
        {
            _assertConfiguration(pConfigurationKey, pDefaultValue, true, out pCurrentValue);
        }

        protected void AssertConfiguration(string pConfigurationKey, out int pCurrentValue)
        {
            string currentValue = null;
            _assertConfiguration(pConfigurationKey, null, false, out currentValue);
            try
            {
                pCurrentValue = int.Parse(currentValue);
            }
            catch (Exception)
            {
                throw new ConfigurationSyntaxErrorException(pConfigurationKey, "Invalid configuration value");
            }
        }

        protected void AssertConfiguration(string pConfigurationKey, int pDefaultValue, out int pCurrentValue)
        {
            string currentValue = null;
            _assertConfiguration(pConfigurationKey, pDefaultValue.ToString(), true, out currentValue);
            try
            {
                pCurrentValue = int.Parse(currentValue);
            }
            catch (Exception)
            {
                throw new ConfigurationSyntaxErrorException(pConfigurationKey, "Invalid configuration value");
            }
        }

        protected void AssertConfiguration(string pConfigurationKey, out double pCurrentValue)
        {
            string currentValue = null;
            _assertConfiguration(pConfigurationKey, null, false, out currentValue);
            try
            {
                pCurrentValue = double.Parse(currentValue);
            }
            catch (Exception)
            {
                throw new ConfigurationSyntaxErrorException(pConfigurationKey, "Invalid configuration value");
            }
        }

        protected void AssertConfiguration(string pConfigurationKey, double pDefaultValue, out double pCurrentValue)
        {
            string currentValue = null;
            _assertConfiguration(pConfigurationKey, pDefaultValue.ToString(), true, out currentValue);
            try
            {
                pCurrentValue = int.Parse(currentValue);
            }
            catch (Exception)
            {
                throw new ConfigurationSyntaxErrorException(pConfigurationKey, "Invalid configuration value");
            }
        }

        protected void AssertConfiguration(string pConfigurationKey, out bool pCurrentValue)
        {
            string currentValue = null;
            _assertConfiguration(pConfigurationKey, null, false, out currentValue);
            try
            {
                pCurrentValue = currentValue.ToLower() == bool.TrueString.ToLower() ? true : false;
            }
            catch (Exception)
            {
                throw new ConfigurationSyntaxErrorException(pConfigurationKey, "Invalid configuration value");
            }
        }

        protected void AssertConfiguration(string pConfigurationKey, bool pDefaultValue, out bool pCurrentValue)
        {
            string currentValue = null;
            _assertConfiguration(pConfigurationKey, pDefaultValue.ToString(), true, out currentValue);
            try
            {
                pCurrentValue = currentValue.ToLower() == bool.TrueString.ToLower() ? true : false;
            }
            catch (Exception)
            {
                throw new ConfigurationSyntaxErrorException(pConfigurationKey, "Invalid configuration value");
            }
        }

        protected virtual void ValidateConfiguration()
        {
            return;
        }

        #endregion

        #region Public members

        public bool HasFile { get { return File.Exists(_resolveDefaultConfigurationPath(ConfigurationFileBasePath)); } }

        public void ReadConfiguration()
        {
            using (FileStream fileStream = File.Open(_resolveDefaultConfigurationPath(ConfigurationFileBasePath), FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    string currentLine = null;
                    while (! (currentLine = reader.ReadLine()).IsNullOrEmpty())
                    {
                        if (currentLine[0] != _commentPrefix)
                        {
                            string[] values = currentLine.Split(_valueSeparators, 2);
                            if (values == null || values.Length != 2)
                            {
                                throw new ConfigurationSyntaxErrorException(currentLine, "Error reading configuration");
                            }
                            string key = _prunestring(values[0]);
                            if (key.IsNullOrEmpty())
                            {
                                throw new ConfigurationSyntaxErrorException(key, "Invalid configuration key");
                            }
                            if (this.ContainsKey(key))
                            {
                                throw new ConfigurationSyntaxErrorException(key, "Duplicate configuration key");
                            }
                            this.Add(key, _prunestring(values[1]));
                        }
                    }
                }
            }
            ValidateConfiguration();
        }

        #endregion

        #region Constructors

        public ConfigurationParserBase()
        {
            ConfigurationFileBasePath = null;
        }

        public ConfigurationParserBase(string pConfigurationFilePath)
        {
            if (pConfigurationFilePath.IsNullOrEmpty())
            {
                throw new ArgumentNullException("pConfigurationFilePath");
            }
            ConfigurationFileBasePath = pConfigurationFilePath;
            if (! HasFile)
            {
                throw new ArgumentException("Missing configuration file", "pConfigurationFilePath");
            }            
        }

        #endregion
    }
}
