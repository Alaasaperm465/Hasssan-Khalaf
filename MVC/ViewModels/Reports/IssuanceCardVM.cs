using System;
using System.Collections.Generic;

namespace MVC.ViewModels.Reports
{
    public class IssuanceLineVM
    {
        public string ProductName { get; set; } = string.Empty;
        public int Cartons { get; set; }
        public int Pallets { get; set; }
    }

    public class IssuanceCardVM
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Inbound / Outbound
        public DateTime CreatedAt { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int TotalCartons { get; set; }
        public int TotalPallets { get; set; }
        public List<IssuanceLineVM> Lines { get; set; } = new List<IssuanceLineVM>();
    }
}
