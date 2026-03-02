using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Domain.Entities;

namespace TinTot.Application.Interfaces.Categories
{
    public interface ICategoryRepository
    {
        Task<bool> ExistsAsync(int id);
        Task<bool> HasChildrenAsync(int id);
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
        Task SaveChangesAsync();
    }
}
