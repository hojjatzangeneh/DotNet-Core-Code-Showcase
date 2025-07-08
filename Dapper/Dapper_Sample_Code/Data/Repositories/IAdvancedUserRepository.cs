using Dapper_Sample_Code.Data.Models;

namespace Dapper_Sample_Code.Data.Repositories;

public interface IAdvancedUserRepository
{
    Task<int> BulkInsertUsersAsync(IEnumerable<User> users);

    Task<(IEnumerable<User>, int)> GetPagedUsersAsync(int page, int pageSize);

    Task<IEnumerable<User>> GetUsersWithProfileAsync();
}