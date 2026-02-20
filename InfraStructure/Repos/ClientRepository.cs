using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;
using InfraStructure.Context;
using Microsoft.EntityFrameworkCore;

namespace InfraStructure.Repos
{
    public class ClientRepository : IClientRepository
    {
        private readonly DBContext _db;
        public ClientRepository(DBContext db) { _db = db; }

        public async Task AddAsync(Client entity)
        {
            await _db.Clients.AddAsync(entity);
        }

        public async Task<IEnumerable<Client>> GetAllAsync()
        {
            return await _db.Clients.AsNoTracking().ToListAsync();
        }

        public async Task<Client?> GetByIdAsync(int id)
        {
            return await _db.Clients.FindAsync(id);
        }

        public void Remove(Client entity)
        {
            _db.Clients.Remove(entity);
        }

        public void Update(Client entity)
        {
            _db.Clients.Update(entity);
        }

        public async Task<Client?> FindByNameAsync(string name)
        {
            return await _db.Clients.FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<IEnumerable<Client>> GetDailyReportAsync()
        {
            // Generic placeholder, return all
            return await GetAllAsync();
        }

        public async Task<IEnumerable<Client>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            // Clients don't have date ranges; return all
            return await GetAllAsync();
        }
    }
}
