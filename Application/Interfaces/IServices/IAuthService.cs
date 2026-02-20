using Hassann_Khala.Application.DTOs.Auth;

namespace Hassann_Khala.Application.Interfaces.IServices
{
    public interface IAuthService
    {
        Task<string> AuthenticateAsync(LoginRequest request);
    }
}