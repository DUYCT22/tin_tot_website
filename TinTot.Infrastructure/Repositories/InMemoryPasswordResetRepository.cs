using System.Collections.Concurrent;
using TinTot.Application.Interfaces.Users;
using TinTot.Domain.Entities;

namespace TinTot.Infrastructure.Repositories
{
    public class InMemoryPasswordResetRepository : IPasswordResetRepository
    {
        private static readonly ConcurrentDictionary<string, PasswordResetCode> ResetCodes = new(StringComparer.OrdinalIgnoreCase);

        public Task<PasswordResetCode?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            ResetCodes.TryGetValue(normalizedEmail, out var passwordResetCode);
            return Task.FromResult(passwordResetCode);
        }

        public Task UpsertAsync(PasswordResetCode passwordResetCode)
        {
            var normalizedEmail = passwordResetCode.Email.Trim().ToLowerInvariant();
            ResetCodes[normalizedEmail] = passwordResetCode;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            ResetCodes.TryRemove(normalizedEmail, out _);
            return Task.CompletedTask;
        }
    }
}
