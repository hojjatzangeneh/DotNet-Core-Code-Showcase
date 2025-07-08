
using System.Data;

namespace Dapper_Sample_Code.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    readonly IDbConnection _db;
    IDbTransaction? _transaction;

    public UnitOfWork(IDbConnection db)
    {
        _db = db;
        Users = new UserRepository(db);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = _db.BeginTransaction();
    }

    public async Task CommitAsync()
    {
        _transaction?.Commit();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
    }

    public async Task RollbackAsync()
    {
        _transaction?.Rollback();
    }

    public IUserRepository Users { get; }
}