using System.ComponentModel.DataAnnotations;

namespace Hassann_Khala.Application.DTOs.Client
{
    public class CreateClientRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int PhoneNumber { get; set; }
    }
}
