using System;
using System.Collections.Generic;
using System.Linq;

namespace MVC.ViewModels.Outbound
{
    public class OutboundDetailsVM
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<MVC.ViewModels.Inbound.InboundDetailVM> Details { get; set; } = new List<MVC.ViewModels.Inbound.InboundDetailVM>();
        public int TotalCartons => Details?.Sum(d => d.Cartons) ?? 0;
        public int TotalPallets => Details?.Sum(d => d.Pallets) ?? 0;
    }
}
