using System;
using System.Collections.Generic;

namespace MVC.ViewModels.Clients
{
    public class ClientDetailsVM
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public int PhoneNumber { get; set; }

        public List<MovementSummary> RecentInbounds { get; set; } = new List<MovementSummary>();
        public List<MovementSummary> RecentOutbounds { get; set; } = new List<MovementSummary>();

        public int TotalCartons { get; set; }
        public int TotalPallets { get; set; }

        public List<ProductAggregate> Products { get; set; } = new List<ProductAggregate>();
        public List<SectionAggregate> Sections { get; set; } = new List<SectionAggregate>();
    }

    public class MovementSummary
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int DetailsCount { get; set; }
    }

    public class ProductAggregate
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Cartons { get; set; }
        public int Pallets { get; set; }
    }

    public class SectionAggregate
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int Cartons { get; set; }
        public int Pallets { get; set; }
    }
}
