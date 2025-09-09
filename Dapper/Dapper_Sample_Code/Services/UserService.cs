using Dapper_Sample_Code.Data.Models;
using Dapper_Sample_Code.Data.Repositories;

namespace Dapper_Sample_Code.Services;

public class UserService(IUserRepository repo) : IUserService
{
    readonly IUserRepository userRepository = repo;

    public async Task<int> CreateUserAsync(User user)
    {
        return await userRepository.InsertAsync(user);
    }

    public async Task<int> DeleteUserAsync(int id)
    {
        return await userRepository.SoftDeleteAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await userRepository.GetAllWithProfileAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await userRepository.GetByIdAsync(id);
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        return await userRepository.UpdateAsync(user);
    }
}