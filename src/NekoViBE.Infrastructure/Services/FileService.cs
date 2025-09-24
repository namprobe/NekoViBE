using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileService> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _maxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string _baseUrl;

        public FileService(
            IWebHostEnvironment webHostEnvironment,
            ILogger<FileService> logger,
            IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _configuration = configuration;
            _baseUrl = _configuration["BaseUrl"] ?? string.Empty;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string destinationPath, CancellationToken cancellationToken)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file provided for upload to {DestinationPath}", destinationPath);
                    throw new ArgumentException("File is null or empty");
                }

                if (file.Length > _maxFileSize)
                {
                    throw new ArgumentException("File size exceeds the maximum allowed size (10MB)");
                }

                // Create unique file name
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, destinationPath);

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                // Return relative file path
                var relativePath = $"/{Path.Combine(destinationPath, fileName).Replace(Path.DirectorySeparatorChar, '/')}";
                _logger.LogInformation("File uploaded successfully to {FilePath}, relative path: {RelativePath}", filePath, relativePath);

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to {DestinationPath}", destinationPath);
                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string destinationPath)
        {
            return await UploadFileAsync(file, destinationPath, CancellationToken.None);
        }

        public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.LogWarning("No file path provided for deletion");
                    return;
                }

                var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("File deleted successfully: {FullPath}", fullPath);
                }
                else
                {
                    _logger.LogWarning("File not found for deletion: {FullPath}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task DeleteFileAsync(string filePath)
        {
            await DeleteFileAsync(filePath, CancellationToken.None);
        }

        public string GetFileUrl(string relativeFilePath)
        {
            if (string.IsNullOrEmpty(relativeFilePath))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(_baseUrl))
            {
                return relativeFilePath;
            }

            return $"{_baseUrl.TrimEnd('/')}/{relativeFilePath.TrimStart('/')}";
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
            return File.Exists(fullPath);
        }
    }
}
