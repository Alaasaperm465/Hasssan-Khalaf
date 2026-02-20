namespace Hassann_Khala.Application.DTOs.Auth
{
    public class LoginRequest
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}