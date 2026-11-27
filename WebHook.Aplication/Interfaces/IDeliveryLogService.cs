using System.Collections.Generic;
using System.Threading.Tasks;
using WebHook.Application.DTOs;

namespace WebHook.Application.Interfaces
{
    public interface IDeliveryLogService
    {
        Task<List<DeliveryLogDto>> GetRecentLogsAsync(int count = 50);
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}