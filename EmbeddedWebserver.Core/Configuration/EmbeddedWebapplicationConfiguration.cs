using EmbeddedWebserver.Core.Configuration.Abstract;
using EmbeddedWebserver.Core.Helpers;

namespace EmbeddedWebserver.Core.Configuration
{
    public sealed class EmbeddedWebapplicationConfiguration : ConfigurationParserBase
    {
        #region Non-public members

        private const string ConfigurationKey_Path = "local.application.path";
        private const string ConfigurationKey_Port = "local.application.port";
        private const string ConfigurationKey_MaxWorkerThreadCount = "local.application.maxworkerthreadcount";
        private const string ConfigurationKey_FileSystemHandlerEnableDirectoryBrowsing = "local.filesystemhandler.enabledirectorybrowsing";
        private const string ConfigurationKey_RequestFilterModuleEnabled = "local.requestfiltermodule.enabled";
        private const string ConfigurationKey_RequestFilterModuleAllowMask = "local.requestfiltermodule.allowmask";

        private string _path;
        private ushort _port;
        private ushort _maxWorkerThreadCount;
        private bool _fileSystemHandlerEnableDirectoryBrowsing;
        private bool _requestFilterModuleEnabled;
        private string _requestFilterModuleAllowMask;

        protected override void ValidateConfiguration()
        {
            int port = 0;
            AssertConfiguration(ConfigurationKey_Port, 80, out port);
            Port = (ushort)port;

            int threadCount = 0;
            AssertConfiguration(ConfigurationKey_MaxWorkerThreadCount, 5, out threadCount);
            MaxWorkerThreadCount = (ushort)threadCount;

            bool enableDirectoryBrowsing = false;
            AssertConfiguration(ConfigurationKey_FileSystemHandlerEnableDirectoryBrowsing, false, out enableDirectoryBrowsing);
            FileSystemHandlerEnableDirectoryBrowsing = enableDirectoryBrowsing;

            bool _requestFilterModuleEnabled = false;
            AssertConfiguration(ConfigurationKey_RequestFilterModuleEnabled, false, out _requestFilterModuleEnabled);
            RequestFilterModuleEnabled = _requestFilterModuleEnabled;

            string _requestFilterModuleAllowMask = null;
            AssertConfiguration(ConfigurationKey_RequestFilterModuleAllowMask, "*.*.*.*", out _requestFilterModuleAllowMask);
            RequestFilterModuleAllowMask = _requestFilterModuleAllowMask;
        }

        #endregion

        #region Public members

        public string Path
        {
            get { return _path; }
            set
            {
                this[ConfigurationKey_Path] = value.ToString();
                _path = value;
            }
        }

        public ushort Port
        {
            get { return _port; }
            set
            {
                this[ConfigurationKey_Port] = value.ToString();
                _port = value;
            }
        }

        public ushort MaxWorkerThreadCount
        {
            get { return _maxWorkerThreadCount; }
            set
            {
                this[ConfigurationKey_MaxWorkerThreadCount] = value.ToString();
                _maxWorkerThreadCount = value;
            }
        }

        public bool FileSystemHandlerEnableDirectoryBrowsing
        {
            get { return _fileSystemHandlerEnableDirectoryBrowsing; }
            set
            {
                this[ConfigurationKey_FileSystemHandlerEnableDirectoryBrowsing] = value.ToString();
                _fileSystemHandlerEnableDirectoryBrowsing = value;
            }
        }

        public bool RequestFilterModuleEnabled
        {
            get { return _requestFilterModuleEnabled; }
            set
            {
                this[ConfigurationKey_RequestFilterModuleEnabled] = value.ToString();
                _requestFilterModuleEnabled = value;
            }
        }

        public string RequestFilterModuleAllowMask
        {
            get { return _requestFilterModuleAllowMask; }
            set
            {
                this[ConfigurationKey_RequestFilterModuleAllowMask] = value.ToString();
                _requestFilterModuleAllowMask = value;
            }
        }

        #endregion

        #region Constructors

        public EmbeddedWebapplicationConfiguration() : base() { }

        public EmbeddedWebapplicationConfiguration(string pEmbedddWebapplicationBasePath)
            : base(PathHelper.ResolveToAbsolutePath(pEmbedddWebapplicationBasePath))
        {
            Path = ConfigurationFileBasePath;
        }

        #endregion
    }
}