namespace MVC.ViewModels.Inbound
{
    public class InboundDetailsVM
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<InboundDetailVM> Details { get; set; } = new List<InboundDetailVM>();
        public int TotalCartons => Details?.Sum(d => d.Cartons) ?? 0;
        public int TotalPallets => Details?.Sum(d => d.Pallets) ?? 0;
    }
}