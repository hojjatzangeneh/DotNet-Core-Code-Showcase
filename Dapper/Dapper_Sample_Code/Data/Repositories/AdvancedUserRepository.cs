using Dapper;

using Dapper_Sample_Code.Data.Models;

using System.Data;

namespace Dapper_Sample_Code.Data.Repositories;

public class AdvancedUserRepository : UserRepository, IAdvancedUserRepository
{
    readonly IDbConnection _db;

    public AdvancedUserRepository(IDbConnection db) : base(db)
    {
        _db = db;
    }

    public async Task<int> BulkInsertUsersAsync(IEnumerable<User> users)
    {
        string sql = "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)";
        return await _db.ExecuteAsync(sql, users);
    }

    public async Task<(IEnumerable<User>, int)> GetPagedUsersAsync(int page, int pageSize)
    {
        string sql = @"SELECT * FROM Users ORDER BY Id OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
                    SELECT COUNT(*) FROM Users";

        using SqlMapper.GridReader multi = await _db.QueryMultipleAsync(
            sql,
            new { Skip = (page - 1) * pageSize, Take = pageSize });
        IEnumerable<User>? users = await multi.ReadAsync<User>();
        int total = await multi.ReadFirstAsync<int>();
        return (users, total);
    }

    public async Task<IEnumerable<User>> GetUsersWithProfileAsync()
    {
        string sql = @"SELECT u.*, p.* FROM Users u JOIN Profiles p ON u.Id = p.UserId";
        return await _db.QueryAsync<User, Profile, User>(
            sql,
            (u, p) =>
            {
                u.Profile = p;
                return u;
            },
            splitOn: "Id");
    }
}