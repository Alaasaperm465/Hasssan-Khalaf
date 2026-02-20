using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace MVC.ViewModels.Outbound
{
    public class OutboundCreateVM
    {
        [Required]
        public int ClientId { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Products { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Sections { get; set; } = new List<SelectListItem>();
        public List<OutboundDetailVM> Details { get; set; } = new List<OutboundDetailVM>();
    }

    public class OutboundDetailVM
    {
        public int ProductId { get; set; }
        public int SectionId { get; set; }
        public int Cartons { get; set; }
        public int Pallets { get; set; }
        public decimal Quantity { get; set; }
    }
}
