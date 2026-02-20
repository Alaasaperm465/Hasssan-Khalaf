
using Hassann_Khala.Domain.Interfaces;

namespace Hassann_Khala.Domain.Interfaces
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> FindByNameAsync(string name);
        Task<IEnumerable<Client>> GetAllAsync();
    }
}
