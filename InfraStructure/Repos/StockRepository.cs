using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;
using InfraStructure.Context;
using Microsoft.EntityFrameworkCore;

namespace InfraStructure.Repos
{
    public class StockRepository : IStockRepository
    {
        private readonly DBContext _db;
        public StockRepository(DBContext db) { _db = db; }

        public async Task<Stock?> FindAsync(int clientId, int productId, int sectionId)
        {
            return await _db.Stocks.FirstOrDefaultAsync(s => s.ClientId == clientId && s.ProductId == productId && s.SectionId == sectionId);
        }

        public async Task AddAsync(Stock stock)
        {
            await _db.Stocks.AddAsync(stock);
        }

        public void Update(Stock stock)
        {
            _db.Stocks.Update(stock);
        }
        public async Task<List<Stock>> GetByClientAsync(int clientId)
        {
            return await _db.Stocks
                .Include(s => s.Product)
                .Include(s => s.Section)
                .Where(s => s.ClientId == clientId && (s.Cartons > 0 || s.Pallets > 0))
                .ToListAsync();
        }

    }
}
