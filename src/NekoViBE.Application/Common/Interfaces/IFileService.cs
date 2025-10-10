using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string destinationPath, CancellationToken cancellationToken);
        Task DeleteFileAsync(string filePath, CancellationToken cancellationToken);
        string GetFileUrl(string relativeFilePath);
    }
}
