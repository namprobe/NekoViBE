using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Infrastructure.Factories
{
    public class FileServiceFactory : IFileServiceFactory
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileService> _fileServiceLogger;

        public FileServiceFactory(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ILogger<FileService> fileServiceLogger)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _fileServiceLogger = fileServiceLogger;
        }

        public IFileService CreateFileService(string storageType)
        {
            switch (storageType.ToLower())
            {
                case "local":
                    return new FileService(_webHostEnvironment, _fileServiceLogger, _configuration);
                default:
                    throw new NotSupportedException($"Storage type {storageType} is not supported.");
            }
        }
    }
}
