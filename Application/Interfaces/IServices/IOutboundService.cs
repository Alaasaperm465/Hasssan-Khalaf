using Hassann_Khala.Application.DTOs.Outbound;

namespace Hassann_Khala.Application.Interfaces.IServices
{
    public interface IOutboundService
    {
        Task<int> CreateOutboundAsync(CreateOutboundRequest request);
    }
}