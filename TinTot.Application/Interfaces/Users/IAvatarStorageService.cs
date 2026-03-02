using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.Interfaces.Users
{
    public interface IAvatarStorageService
    {
        Task<string> UploadImageAsync(Stream stream, string publicId);
        Task DeleteImageAsync(string publicId);
    }
}
