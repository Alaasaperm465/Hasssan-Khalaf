namespace Hassann_Khala.Application.DTOs.Stock
{
    public class StockResponse
    {
        public int ClientId { get; set; }
        public int ProductId { get; set; }
        public int SectionId { get; set; }
        public int Cartons { get; set; }
        public int Pallets { get; set; }
    }
}