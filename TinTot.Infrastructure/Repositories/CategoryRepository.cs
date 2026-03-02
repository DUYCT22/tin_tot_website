using Microsoft.EntityFrameworkCore;
using TinTot.Application.Interfaces.Categories;
using TinTot.Domain.Entities;
using TinTot.Infrastructure.Data;

namespace TinTot.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> ExistsAsync(int id) => _context.Categories.AnyAsync(x => x.Id == id);
        public Task<bool> HasChildrenAsync(int id) => _context.Categories.AnyAsync(x => x.ParentId == id);
        public Task<Category?> GetByIdAsync(int id) => _context.Categories.FirstOrDefaultAsync(x => x.Id == id);

        public async Task AddAsync(Category category) => await _context.Categories.AddAsync(category);
        public Task UpdateAsync(Category category) { _context.Categories.Update(category); return Task.CompletedTask; }
        public Task DeleteAsync(Category category) { _context.Categories.Remove(category); return Task.CompletedTask; }
        public Task SaveChangesAsync() => _context.SaveChangesAsync();

    }
}
