using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Domain.Entities;

namespace TinTot.Application.Interfaces.Banners
{
    public interface IBannerRepository
    {
        Task<Banner?> GetByIdAsync(int id);
        Task AddAsync(Banner banner);
        Task UpdateAsync(Banner banner);
        Task DeleteAsync(Banner banner);
        Task SaveChangesAsync();
    }
}
