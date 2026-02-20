namespace MVC.ViewModels.Inbound
{
    public class InboundListVM
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int LinesCount { get; set; }
    }
}