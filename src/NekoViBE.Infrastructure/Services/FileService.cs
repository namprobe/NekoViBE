using Microsoft.AspNetCore.Http;
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
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
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

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), destinationPath, fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                var relativePath = $"/{Path.Combine(destinationPath, fileName).Replace(Path.DirectorySeparatorChar, '/')}";
                _logger.LogInformation("File uploaded successfully to {FullPath}, relative path: {RelativePath}", fullPath, relativePath);
                return relativePath;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error uploading file to {DestinationPath}", destinationPath);
                throw;
            }
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

                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath.TrimStart('/'));
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
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                throw;
            }
        }
    }
}
