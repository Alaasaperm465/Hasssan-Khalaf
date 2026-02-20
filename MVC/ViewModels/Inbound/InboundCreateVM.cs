using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVC.ViewModels.Inbound
{
    public class InboundCreateVM
    {
        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        public IEnumerable<SelectListItem> Clients { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Products { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Sections { get; set; } = new List<SelectListItem>();

        public List<InboundDetailVM> Details { get; set; } = new List<InboundDetailVM>();

        public int TotalCartons => Details?.Sum(d => d.Cartons) ?? 0;
        public int TotalPallets => Details?.Sum(d => d.Pallets) ?? 0;
    }
}
