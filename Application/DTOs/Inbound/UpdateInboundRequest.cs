using System.ComponentModel.DataAnnotations;

namespace Hassann_Khala.Application.DTOs.Inbound
{
    public class UpdateInboundRequest
    {
        [Required]
        public string ClientName { get; set; } = null!;

        [Required]
        [MinLength(1)]
        public List<InboundLineRequest> Lines { get; set; } = new();
    }
}
