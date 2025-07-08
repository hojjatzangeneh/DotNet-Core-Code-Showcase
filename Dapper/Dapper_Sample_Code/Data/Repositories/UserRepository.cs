using Dapper;

using Dapper_Sample_Code.Data.Models;

using System.Data;

namespace Dapper_Sample_Code.Data.Repositories;

public class UserRepository : IUserRepository
{
    readonly IDbConnection _db;

    public UserRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<int> CreateUserAsync(User user)
    {
        return await _db.ExecuteAsync("INSERT INTO Users (Name, Email) VALUES (@Name, @Email)", user);
    }

    public async Task<int> DeleteUserAsync(int id)
    {
        return await _db.ExecuteAsync("DELETE FROM Users WHERE Id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _db.QueryAsync<User>("SELECT * FROM Users");
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _db.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        return await _db.ExecuteAsync("UPDATE Users SET Name = @Name, Email = @Email WHERE Id = @Id", user);
    }
}