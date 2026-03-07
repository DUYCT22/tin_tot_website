namespace Tin_Tot_Website.Services
{
    public interface IEntityKeyService
    {
        string ProtectId(string scope, int id);
        int? UnprotectId(string scope, string protectedId);
    }
}
