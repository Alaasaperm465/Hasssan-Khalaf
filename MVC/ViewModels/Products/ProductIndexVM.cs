using System.Collections.Generic;
using Hassann_Khala.Domain;

namespace MVC.ViewModels.Products
{
    public class ProductIndexVM
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public string? Search { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)System.Math.Ceiling((double)TotalCount / PageSize) : 1;
    }
}
