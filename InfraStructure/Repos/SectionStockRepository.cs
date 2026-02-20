using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;
using InfraStructure.Context;
using Microsoft.EntityFrameworkCore;

namespace InfraStructure.Repos
{
    public class SectionStockRepository : ISectionStockRepository
    {
        private readonly DBContext _db;
        public SectionStockRepository(DBContext db) { _db = db; }

        public async Task<SectionStock?> FindAsync(int clientId, int sectionId)
        {
            return await _db.SectionStocks.FirstOrDefaultAsync(ss => ss.ClientId == clientId && ss.SectionId == sectionId);
        }

        public async Task AddAsync(SectionStock stock)
        {
            await _db.SectionStocks.AddAsync(stock);
        }

        public void Update(SectionStock stock)
        {
            _db.SectionStocks.Update(stock);
        }
    }
}
