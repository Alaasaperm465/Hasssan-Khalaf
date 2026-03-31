using Hassann_Khala.Application.DTOs.Reports;

namespace Hassann_Khala.Application.Interfaces.IServices
{
    public interface IPdfService
    {
        byte[] GenerateMovementDetailsPdf(MovementDetailsDto report);
    }
}
