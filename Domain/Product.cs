namespace Hassann_Khala.Domain
{
    using global::Domain;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    public class Product: BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        [Range(0, 99999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0.0m;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        // Navigation: stocks referencing this product
        [JsonIgnore]
        public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
    }
}
