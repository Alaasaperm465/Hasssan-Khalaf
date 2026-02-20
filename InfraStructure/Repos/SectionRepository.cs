using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;
using InfraStructure.Context;
using Microsoft.EntityFrameworkCore;

namespace InfraStructure.Repos
{
    public class SectionRepository : ISectionRepository
    {
        private readonly DBContext _db;
        public SectionRepository(DBContext db) { _db = db; }

        public async Task AddAsync(Section entity)
        {
            await _db.Sections.AddAsync(entity);
        }

        public async Task<IEnumerable<Section>> GetAllAsync()
        {
            return await _db.Sections.AsNoTracking().ToListAsync();
        }

        public async Task<Section?> GetByIdAsync(int id)
        {
            return await _db.Sections.FindAsync(id);
        }

        public void Remove(Section entity)
        {
            _db.Sections.Remove(entity);
        }

        public void Update(Section entity)
        {
            _db.Sections.Update(entity);
        }

        public async Task<Section?> GetByNameAsync(string name)
        {
            return await _db.Sections.FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task<IEnumerable<Section>> GetDailyReportAsync()
        {
            return await GetAllAsync();
        }

        public async Task<IEnumerable<Section>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await GetAllAsync();
        }
    }
}
