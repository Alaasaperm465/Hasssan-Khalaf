using Hassann_Khala.Application.DTOs.Inbound;
using Hassann_Khala.Domain;

namespace Hassann_Khala.Application.Interfaces.IServices
{
    public interface IInboundService
    {
        Task<int> CreateInboundAsync(CreateInboundRequest request);
        Task<IEnumerable<Inbound>> GetAllInboundsAsync();
        Task<IEnumerable<Inbound>> GetDailyInboundReportAsync();
        Task<IEnumerable<Inbound>> GetInboundReportFromToAsync(DateTime startDate, DateTime endDate);
        Task UpdateInboundAsync(int id, UpdateInboundRequest request);
    }
}