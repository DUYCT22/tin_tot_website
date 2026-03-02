using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs;
using TinTot.Application.DTOs.Users;

namespace TinTot.Application.Interfaces.Banners
{
    public interface IBannerService
    {
        Task<BannerDto> CreateAsync(BannerUpsertDto dto, AvatarUploadDto imageUpload);
        Task<BannerDto> UpdateAsync(int id, BannerUpsertDto dto, AvatarUploadDto? imageUpload);
        Task DeleteAsync(int id);
    }
}
