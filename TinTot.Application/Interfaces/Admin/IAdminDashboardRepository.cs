using TinTot.Application.DTOs.Admin;

namespace TinTot.Application.Interfaces.Admin;

public interface IAdminDashboardRepository
{
    Task<AdminDashboardDto> GetDashboardDataAsync();
}
