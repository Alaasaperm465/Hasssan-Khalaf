using System.ComponentModel.DataAnnotations;

namespace Hassann_Khala.Application.DTOs.Product
{
    public class UpdateProductDto
    {
        [Required]
        [MaxLength(250)]
        public string Name { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }
}
