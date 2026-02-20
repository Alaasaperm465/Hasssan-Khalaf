using System;

namespace Hassann_Khala.Application.Interfaces.IServices
{
    public interface ITokenService
    {
        string CreateToken(int userId, string userName, string role);
    }
}