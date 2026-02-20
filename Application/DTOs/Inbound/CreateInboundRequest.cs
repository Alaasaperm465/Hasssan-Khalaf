using System.ComponentModel.DataAnnotations;

namespace Hassann_Khala.Application.DTOs.Inbound
{
    public class CreateInboundRequest
    {
        [Required]
        public string ClientName { get; set; } = null!;

        [Required]
        [MinLength(1)]
        public List<InboundLineRequest> Lines { get; set; } = new();
    }

    public class InboundLineRequest
    {
        [Required]
        public string ProductName { get; set; } = null!;

        [Required]
        public string SectionName { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int Cartons { get; set; }

        [Range(0, int.MaxValue)]
        public int Pallets { get; set; }
    }
}