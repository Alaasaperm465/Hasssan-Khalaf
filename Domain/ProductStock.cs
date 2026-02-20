using Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hassann_Khala.Domain
{
    public class ProductStock : BaseEntity
    {
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Cartons { get; set; }
        public int Pallets { get; set; }
    }
}