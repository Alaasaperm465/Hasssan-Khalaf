using Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hassann_Khala.Domain
{
    public class SectionStock : BaseEntity
    {
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int SectionId { get; set; }
        public Section Section { get; set; } = null!;

        public int Cartons { get; set; }
        public int Pallets { get; set; }
    }
}