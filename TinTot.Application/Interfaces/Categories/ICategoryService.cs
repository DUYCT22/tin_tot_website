using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs;
using TinTot.Application.DTOs.Users;

namespace TinTot.Application.Interfaces.Categories
{
    public interface ICategoryService
    {
        Task<CategoryDto> CreateAsync(CategoryUpsertDto dto, AvatarUploadDto? imageUpload);
        Task<CategoryDto> UpdateAsync(int id, CategoryUpsertDto dto, AvatarUploadDto? imageUpload);
        Task DeleteAsync(int id);
    }
}
