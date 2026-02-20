using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;
using InfraStructure.Context;
using Microsoft.EntityFrameworkCore;

namespace InfraStructure.Repos
{
    public class ProductStockRepository : IProductStockRepository
    {
        private readonly DBContext _db;
        public ProductStockRepository(DBContext db) { _db = db; }

        public async Task<ProductStock?> FindAsync(int clientId, int productId)
        {
            return await _db.ProductStocks.FirstOrDefaultAsync(ps => ps.ClientId == clientId && ps.ProductId == productId);
        }

        public async Task AddAsync(ProductStock stock)
        {
            await _db.ProductStocks.AddAsync(stock);
        }

        public void Update(ProductStock stock)
        {
            _db.ProductStocks.Update(stock);
        }
    }
}
