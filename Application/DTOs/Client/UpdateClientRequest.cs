using System.ComponentModel.DataAnnotations;

namespace Hassann_Khala.Application.DTOs.Client
{
    public class UpdateClientRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;
    }
}
