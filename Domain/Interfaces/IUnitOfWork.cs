namespace Hassann_Khala.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the provided async action inside a database transaction. The implementation
        /// should begin a transaction, execute the action, commit on success and rollback on failure.
        /// </summary>
        /// <param name="action">Async action to execute</param>
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}