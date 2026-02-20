using System.ComponentModel.DataAnnotations;

namespace Hassann_Khala.Application.DTOs.Product
{
    public class CreateProductDto
    {
        [Required]
        [MaxLength(250)]
        public string Name { get; set; } = null!;
    }
}
