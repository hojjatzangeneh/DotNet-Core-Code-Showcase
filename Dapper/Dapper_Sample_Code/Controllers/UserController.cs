using Dapper_Sample_Code.Data.Models;
using Dapper_Sample_Code.Data.Repositories;
using Dapper_Sample_Code.DTO;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Dapper_Sample_Code.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    readonly IMemoryCache _cache;
    readonly IUnitOfWork _uow;

    public UserController(IUnitOfWork uow, IMemoryCache cache)
    {
        _uow = uow;
        _cache = cache;
    }

    [HttpPost("bulk")]
    public Task<int> BulkInsert(List<User> users)
    {
        return _uow.Users.BulkInsertAsync(users);
    }

    [HttpDelete("{id}")]
    public Task<int> Delete(int id)
    {
        return _uow.Users.SoftDeleteAsync(id);
    }

    [HttpGet("{id}")]
    public Task<User?> Get(int id)
    {
        return _uow.Users.GetByIdAsync(id);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] UserFilterDto filter)
    {
        string cacheKey = $"users_{filter.Page}_{filter.PageSize}_{filter.NameContains}_{filter.EmailContains}";
        if(!_cache.TryGetValue(cacheKey, out IEnumerable<User> users))
        {
            users = await _uow.Users.GetPagedAsync(filter);
            _cache.Set(cacheKey, users, TimeSpan.FromMinutes(5));
        }
        int total = await _uow.Users.CountAsync(filter);
        return Ok(new { total, users });
    }

    [HttpGet("multi")]
    public async Task<IActionResult> Multi()
    {
        return Ok(await _uow.Users.GetMultipleAsync());
    }

    [HttpPost]
    public Task<int> Post(User user)
    {
        return _uow.Users.InsertAsync(user);
    }

    [HttpPut]
    public Task<int> Put(User user)
    {
        return _uow.Users.UpdateAsync(user);
    }
}