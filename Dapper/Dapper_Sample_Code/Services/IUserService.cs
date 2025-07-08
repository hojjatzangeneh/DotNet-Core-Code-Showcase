using Dapper_Sample_Code.Data.Models;

namespace Dapper_Sample_Code.Services;

public interface IUserService
{
    Task<int> CreateUserAsync(User user);

    Task<int> DeleteUserAsync(int id);

    Task<IEnumerable<User>> GetAllUsersAsync();

    Task<User> GetUserByIdAsync(int id);

    Task<int> UpdateUserAsync(User user);
}