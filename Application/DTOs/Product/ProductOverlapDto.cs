namespace Hassann_Khala.Application.DTOs.Product
{
    public class ProductOverlapDto
    {
        // Prefer client name selection as requested; keep ClientId optional for compatibility
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }

        public int SectionId { get; set; }
        public int SourceProductId { get; set; }
        public int TargetProductId { get; set; }
        public int Cartons { get; set; }
        public int Pallets { get; set; }
    }
}
