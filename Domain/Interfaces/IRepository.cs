using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hassann_Khala.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetDailyReportAsync();
        Task<IEnumerable<T>> GetByDateRangeAsync(DateTime start, DateTime end);
    }
}