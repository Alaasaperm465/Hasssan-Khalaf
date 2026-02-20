using Hassann_Khala.Domain.Interfaces;
using InfraStructure.Context;
using Microsoft.EntityFrameworkCore;

namespace InfraStructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DBContext _db;
        public UnitOfWork(DBContext db) { _db = db; }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => await _db.SaveChangesAsync(cancellationToken);

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            // Use EF Core transaction
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    await action();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
