using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hassann_Khala.Domain;
using Hassann_Khala.Domain.Interfaces;
using InfraStructure.Context;
using Microsoft.EntityFrameworkCore;

namespace InfraStructure.Repos
{
    public class OutboundRepository : IRepository<Outbound>
    {
        private readonly DBContext _db;
        public OutboundRepository(DBContext db) { _db = db; }

        public async Task AddAsync(Outbound entity)
        {
            await _db.Outbounds.AddAsync(entity);
        }

        public async Task<IEnumerable<Outbound>> GetAllAsync()
        {
            return await _db.Outbounds
                .Include(o => o.Client)
                .Include(o => o.Details).ThenInclude(d => d.Product)
                .Include(o => o.Details).ThenInclude(d => d.Section)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Outbound?> GetByIdAsync(int id)
        {
            return await _db.Outbounds
                .Include(o => o.Client)
                .Include(o => o.Details).ThenInclude(d => d.Product)
                .Include(o => o.Details).ThenInclude(d => d.Section)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public void Remove(Outbound entity)
        {
            _db.Outbounds.Remove(entity);
        }

        public void Update(Outbound entity)
        {
            _db.Outbounds.Update(entity);
        }

        public async Task<IEnumerable<Outbound>> GetDailyReportAsync()
        {
            var start = DateTime.UtcNow.Date;
            var end = start.AddDays(1);
            return await _db.Outbounds
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
                .Include(o => o.Client)
                .Include(o => o.Details).ThenInclude(d => d.Product)
                .Include(o => o.Details).ThenInclude(d => d.Section)
                .ToListAsync();
        }

        public async Task<IEnumerable<Outbound>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _db.Outbounds
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
                .Include(o => o.Client)
                .Include(o => o.Details).ThenInclude(d => d.Product)
                .Include(o => o.Details).ThenInclude(d => d.Section)
                .ToListAsync();
        }
    }
}
