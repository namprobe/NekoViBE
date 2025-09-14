using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Interfaces
{
    public interface IFileServiceFactory
    {
        IFileService CreateFileService(string storageType);
    }
}
