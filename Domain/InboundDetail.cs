using Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hassann_Khala.Domain
{
    public class InboundDetail: BaseEntity
    {
        public int InboundId { get; set; }
        public Inbound Inbound { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int SectionId { get; set; }
        public Section Section { get; set; } = null!;

        public int Cartons { get; set; }
        public int Pallets { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }
    }
}
