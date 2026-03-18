using TinTot.Application.DTOs.Admin;
using TinTot.Application.Interfaces.Admin;

namespace TinTot.Application.Services.Admin;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDashboardRepository _adminDashboardRepository;

    public AdminDashboardService(IAdminDashboardRepository adminDashboardRepository)
    {
        _adminDashboardRepository = adminDashboardRepository;
    }

    public Task<AdminDashboardDto> GetDashboardDataAsync() => _adminDashboardRepository.GetDashboardDataAsync();
}
