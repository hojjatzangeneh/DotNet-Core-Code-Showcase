
namespace Dapper_Sample_Code.Data.Repositories;

public interface IUnitOfWork : IDisposable
{
    Task BeginTransactionAsync();

    Task CommitAsync();

    Task RollbackAsync();

    IUserRepository Users { get; }
}