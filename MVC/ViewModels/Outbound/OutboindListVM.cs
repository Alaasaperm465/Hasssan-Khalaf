namespace MVC.ViewModels.Outbound
{
    public class OutboindListVM
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int LinesCount { get; set; }
    }
}
