namespace Hassann_Khala.Application.DTOs.Outbound
{
    public class CreateOutboundRequest
    {
        public int ClientId { get; set; }
        public List<OutboundLine> Lines { get; set; } = new();
    }

    public class OutboundLine
    {
        public int ProductId { get; set; }
        public int SectionId { get; set; }
        public int Cartons { get; set; }
        public int Pallets { get; set; }
        public decimal Quantity { get; set; }
    }
}