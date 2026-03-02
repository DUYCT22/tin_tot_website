using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinTot.Application.DTOs.Users
{
    public class AvatarUploadDto
    {
        public string FileName { get; set; } = default!;
        public Stream Content { get; set; } = default!;
    }
}
