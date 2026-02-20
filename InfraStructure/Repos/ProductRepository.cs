using System.Collections.Generic;
using System.Threading.Tasks;
using Hassann_Khala.Domain;
using Hassann_Khala.Application.Interfaces.IRepositories;
using InfraStructure.Context;
using Microsoft.EntityFrameworkCore;
using Hassann_Khala.Domain.Interfaces;
using System.Linq;
using System;

namespace InfraStructure.Repos
{
    public class ProductRepository : Hassann_Khala.Application.Interfaces.IRepositories.IProductRepository, Hassann_Khala.Domain.Interfaces.IProductRepository
    {
        private readonly DBContext _db;
        public ProductRepository(DBContext db) { _db = db; }

        public async Task AddAsync(Product product)
        {
            await _db.Products.AddAsync(product);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _db.Products.AsNoTracking().ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _db.Products.FindAsync(id);
        }

        public async Task<bool> AnyStockAsync(int productId)
        {
            return await _db.Stocks.AnyAsync(s => s.ProductId == productId);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Products.AnyAsync(p => p.Id == id && p.IsActive);
        }

        // helper to allow getting by name
        public async Task<Product?> GetByNameInternalAsync(string name)
        {
            return await _db.Products.FirstOrDefaultAsync(p => p.Name == name && p.IsActive);
        }

        // Implementation for Domain.Interfaces.IProductRepository
        public async Task<Product?> GetByNameAsync(string name)
        {
            return await GetByNameInternalAsync(name);
        }

        public void Update(Product entity)
        {
            _db.Products.Update(entity);
        }

        public void Remove(Product entity)
        {
            _db.Products.Remove(entity);
        }

        public async Task<IEnumerable<Product>> GetDailyReportAsync()
        {
            var start = DateTime.UtcNow.Date;
            var end = start.AddDays(1);
            return await _db.Products.Where(p => EF.Property<DateTime>(p, "CreatedAt") >= start && EF.Property<DateTime>(p, "CreatedAt") < end).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByDateRangeAsync(DateTime start, DateTime end)
        {
            var s = start.ToUniversalTime();
            var e = end.ToUniversalTime();
            return await _db.Products.Where(p => EF.Property<DateTime>(p, "CreatedAt") >= s && EF.Property<DateTime>(p, "CreatedAt") < e).ToListAsync();
        }
    }
}
