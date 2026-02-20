using System.Threading.Tasks;
using Hassann_Khala.Domain;

namespace Hassann_Khala.Domain.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByNameAsync(string name);
    }
}
