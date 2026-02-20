using Hassann_Khala.Domain;

namespace Hassann_Khala.Application.Interfaces.IRepositories
{
    public interface IProductRepository
    {
        Task AddAsync(Product product);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<bool> AnyStockAsync(int productId);
        Task SaveChangesAsync();
        Task<bool> ExistsAsync(int id);
    }
}
