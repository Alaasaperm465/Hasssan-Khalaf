using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace MVC.ViewModels.Outbound
{
    public class OutboundCreateVM
    {
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; } = new List<SelectListItem>();

        // Delegate support (similar to Inbound)
        public int? DelegateId { get; set; }
        public IEnumerable<SelectListItem> Delegates { get; set; } = new List<SelectListItem>();

        // Top-level section selection (applies to lines filtering)
        public int? SectionId { get; set; }

        public string? CreatedByName { get; set; }
        public string? CreatedById { get; set; }
        public int? AdditionalEntry { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        public IEnumerable<SelectListItem> Products { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Sections { get; set; } = new List<SelectListItem>();
        public List<OutboundDetailVM> Details { get; set; } = new List<OutboundDetailVM>();

        public int TotalCartons => Details?.Sum(d => d.Cartons) ?? 0;
        public int TotalPallets => Details?.Sum(d => d.Pallets) ?? 0;
    }

    public class OutboundDetailVM
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int SectionId { get; set; }

        [Range(0, int.MaxValue)]
        public int Cartons { get; set; }

        [Range(0, int.MaxValue)]
        public int Pallets { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }
    }
}
