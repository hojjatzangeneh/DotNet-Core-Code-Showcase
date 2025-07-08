
using Dapper;

using Dapper_Sample_Code.Data.Models;

using Dapper_Sample_Code.DTO;

using System.Data;
using System.Text;

namespace Dapper_Sample_Code.Data.Repositories;

public class UserRepository : IUserRepository
{
    readonly IDbConnection _db;

    public UserRepository(IDbConnection db)
    {
        _db = db;
    }

    public Task<int> BulkInsertAsync(IEnumerable<User> users)
    {
        string sql = "INSERT INTO Users (Name, Email, IsDeleted) VALUES (@Name, @Email, 0)";
        return _db.ExecuteAsync(sql, users);
    }

    public async Task<int> CountAsync(UserFilterDto filter)
    {
        StringBuilder sql = new StringBuilder("SELECT COUNT(*) FROM Users WHERE IsDeleted = 0");
        if(!string.IsNullOrEmpty(filter.NameContains))
        {
            sql.Append(" AND Name LIKE @Name");
        }

        if(!string.IsNullOrEmpty(filter.EmailContains))
        {
            sql.Append(" AND Email LIKE @Email");
        }

        return await _db.ExecuteScalarAsync<int>(
            sql.ToString(),
            new
            {
                Name = $"%{filter.NameContains}%",
                Email = $"%{filter.EmailContains}%"
            });
    }

    public async Task<IEnumerable<User>> GetAllWithProfileAsync()
    {
        string sql = "SELECT u.*, p.* FROM Users u JOIN Profiles p ON u.Id = p.UserId WHERE u.IsDeleted = 0";
        return await _db.QueryAsync<User, Profile, User>(
            sql,
            (u, p) =>
            {
                u.Profile = p;
                return u;
            },
            splitOn: "Id");
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _db.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id });
    }

    public async Task<(IEnumerable<User>, IEnumerable<Profile>)> GetMultipleAsync()
    {
        string sql = "SELECT * FROM Users WHERE IsDeleted = 0; SELECT * FROM Profiles";
        using SqlMapper.GridReader multi = await _db.QueryMultipleAsync(sql);
        IEnumerable<User> users = await multi.ReadAsync<User>();
        IEnumerable<Profile> profiles = await multi.ReadAsync<Profile>();
        return (users, profiles);
    }

    public async Task<IEnumerable<User>> GetPagedAsync(UserFilterDto filter)
    {
        StringBuilder sql = new StringBuilder("SELECT * FROM Users WHERE IsDeleted = 0");
        if(!string.IsNullOrEmpty(filter.NameContains))
        {
            sql.Append(" AND Name LIKE @Name");
        }

        if(!string.IsNullOrEmpty(filter.EmailContains))
        {
            sql.Append(" AND Email LIKE @Email");
        }

        sql.Append($" ORDER BY {filter.SortBy} {( filter.Desc ? "DESC" : "ASC" )}");
        sql.Append(" OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY");
        return await _db.QueryAsync<User>(
            sql.ToString(),
            new
            {
                Name = $"%{filter.NameContains}%",
                Email = $"%{filter.EmailContains}%",
                Skip = (filter.Page - 1) * filter.PageSize,
                Take = filter.PageSize
            });
    }

    public Task<int> InsertAsync(User user)
    {
        return _db.ExecuteAsync("INSERT INTO Users (Name, Email, IsDeleted) VALUES (@Name, @Email, 0)", user);
    }

    public Task<int> SoftDeleteAsync(int id)
    {
        return _db.ExecuteAsync("UPDATE Users SET IsDeleted = 1 WHERE Id = @Id", new { Id = id });
    }

    public Task<int> UpdateAsync(User user)
    {
        return _db.ExecuteAsync("UPDATE Users SET Name = @Name, Email = @Email WHERE Id = @Id", user);
    }
}