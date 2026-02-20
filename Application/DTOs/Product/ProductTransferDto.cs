namespace Hassann_Khala.Application.DTOs.Product
{
    public class ProductTransferDto
    {
        public int ClientId { get; set; }
        public int ProductId { get; set; }
        public int FromSectionId { get; set; }
        public int ToSectionId { get; set; }
        public int Cartons { get; set; }
        public int Pallets { get; set; }
    }
}
