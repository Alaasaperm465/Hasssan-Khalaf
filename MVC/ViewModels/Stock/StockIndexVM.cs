using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace MVC.ViewModels.Stock
{
    public class StockIndexVM
    {
        public List<SelectListItem> Clients { get; set; } = new List<SelectListItem>();
        public int? SelectedClientId { get; set; }
        public List<StockRowVM> Stocks { get; set; } = new List<StockRowVM>();
    }

    public class StockRowVM
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int Cartons { get; set; }
        public int Pallets { get; set; }
    }
}
