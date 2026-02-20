using Hassann_Khala.Domain;

namespace Hassann_Khala.Domain.Interfaces
{
    public interface IStockRepository
    {
        Task<Stock?> FindAsync(int clientId, int productId, int sectionId);

        Task<List<Stock>> GetByClientAsync(int clientId); // 

        Task AddAsync(Stock stock);
        void Update(Stock stock);
    }
}
