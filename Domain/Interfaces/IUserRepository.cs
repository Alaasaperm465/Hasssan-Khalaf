using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;
using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;

namespace Hassann_Khala.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> FindByUserNameAsync(string userName);
    }
}