using TinTot.Application.DTOs.Admin;

namespace TinTot.Application.Interfaces.Admin;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardDataAsync();
}
