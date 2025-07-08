using Dapper_Sample_Code.Data.Models;
using Dapper_Sample_Code.DTO;

namespace Dapper_Sample_Code.Data.Repositories;

public interface IUserRepository
{
    Task<int> BulkInsertAsync(IEnumerable<User> users);

    Task<int> CountAsync(UserFilterDto filter);

    Task<IEnumerable<User>> GetAllWithProfileAsync();

    Task<User?> GetByIdAsync(int id);

    Task<(IEnumerable<User>, IEnumerable<Profile>)> GetMultipleAsync();

    Task<IEnumerable<User>> GetPagedAsync(UserFilterDto filter);

    Task<int> InsertAsync(User user);

    Task<int> SoftDeleteAsync(int id);

    Task<int> UpdateAsync(User user);
}