namespace Hassann_Khala.Domain
{
    using global::Domain;
    using System.ComponentModel.DataAnnotations;

    public class Client: BaseEntity

    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int PhoneNumber { get; set; }
    }
}
