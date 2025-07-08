using Dapper_Sample_Code.Data.Models;
using Dapper_Sample_Code.Data.Repositories;

namespace Dapper_Sample_Code.Services;

public class UserService : IUserService
{
    readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    public Task<int> CreateUserAsync(User user)
    {
        return _repo.CreateUserAsync(user);
    }

    public Task<int> DeleteUserAsync(int id)
    {
        return _repo.DeleteUserAsync(id);
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return _repo.GetAllUsersAsync();
    }

    public Task<User> GetUserByIdAsync(int id)
    {
        return _repo.GetUserByIdAsync(id);
    }

    public Task<int> UpdateUserAsync(User user)
    {
        return _repo.UpdateUserAsync(user);
    }
}