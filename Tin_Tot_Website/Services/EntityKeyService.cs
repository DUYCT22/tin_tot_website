using Microsoft.AspNetCore.DataProtection;

namespace Tin_Tot_Website.Services
{
    public class EntityKeyService : IEntityKeyService
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public EntityKeyService(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtectionProvider = dataProtectionProvider;
        }

        public string ProtectId(string scope, int id)
        {
            var protector = _dataProtectionProvider.CreateProtector($"TinTot:{scope}");
            return protector.Protect(id.ToString());
        }

        public int? UnprotectId(string scope, string protectedId)
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector($"TinTot:{scope}");
                var raw = protector.Unprotect(protectedId);
                return int.TryParse(raw, out var id) ? id : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
