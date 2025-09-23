using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Infrastructure.Services;

namespace NekoViBE.Infrastructure.Factories
{
    public class FileServiceFactory : IFileServiceFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _fileServiceLogger;

        public FileServiceFactory(IConfiguration configuration, ILogger<FileService> localFileServiceLogger)
        {
            _configuration = configuration;
            _fileServiceLogger = localFileServiceLogger;
        }

        public IFileService CreateFileService(string storageType)
        {
            switch (storageType.ToLower())
            {
                case "local":
                    return new FileService(_fileServiceLogger);
                default:
                    throw new NotSupportedException($"Storage type {storageType} is not supported.");
            }
        }
    }
}
