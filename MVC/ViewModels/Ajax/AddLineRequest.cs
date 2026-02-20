namespace MVC.ViewModels.Ajax
{
    public class AddLineRequest
    {
        public int? InboundId { get; set; }
        public int? OutboundId { get; set; }
        public int ClientId { get; set; }
        public int ProductId { get; set; }
        public int SectionId { get; set; }
        public int Cartons { get; set; }
        public int Pallets { get; set; }
    }
}
