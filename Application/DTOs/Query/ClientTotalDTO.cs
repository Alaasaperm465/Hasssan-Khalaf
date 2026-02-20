namespace MVC.Controllers
{
    public class ClientTotalDTO
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int TotalCartons { get; set; }
        public int TotalPallets { get; set; }
    }
}
