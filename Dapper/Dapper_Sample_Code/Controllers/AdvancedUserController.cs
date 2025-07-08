using Dapper_Sample_Code.Data.Models;
using Dapper_Sample_Code.Data.Repositories;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AdvancedUserController : ControllerBase
{
    readonly IAdvancedUserRepository _repo;

    public AdvancedUserController(IAdvancedUserRepository repo)
    {
        _repo = repo;
    }

    [HttpPost("bulk")]
    public Task<int> BulkInsert(List<User> users)
    {
        return _repo.BulkInsertUsersAsync(users);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(int page = 1, int pageSize = 10)
    {
        var (users, total) = await _repo.GetPagedUsersAsync(page, pageSize);
        return new JsonResult(new { total, users });
    }

    [HttpGet("with-profile")]
    public Task<IEnumerable<User>> GetWithProfile()
    {
        return _repo.GetUsersWithProfileAsync();
    }
}