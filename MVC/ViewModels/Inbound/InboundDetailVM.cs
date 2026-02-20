using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels.Inbound
{
    public class InboundDetailVM
    {
        public int Id { get; set; }
        public int InboundId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        [Required]
        [Display(Name = "Section")]
        public int SectionId { get; set; }

        public string? SectionName { get; set; }

        [Range(0, int.MaxValue)]
        public int Cartons { get; set; }

        [Range(0, int.MaxValue)]
        public int Pallets { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }
    }
}
