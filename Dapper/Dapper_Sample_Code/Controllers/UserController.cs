using Dapper_Sample_Code.Data.Models;
using Dapper_Sample_Code.Services;

using Microsoft.AspNetCore.Mvc;

namespace Dapper_Sample_Code.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpDelete("{id}")]
    public Task<int> Delete(int id)
    {
        return _service.DeleteUserAsync(id);
    }

    [HttpGet]
    public Task<IEnumerable<User>> Get()
    {
        return _service.GetAllUsersAsync();
    }

    [HttpGet("{id}")]
    public Task<User> Get(int id)
    {
        return _service.GetUserByIdAsync(id);
    }

    [HttpPost]
    public Task<int> Post(User user)
    {
        return _service.CreateUserAsync(user);
    }

    [HttpPut]
    public Task<int> Put(User user)
    {
        return _service.UpdateUserAsync(user);
    }
}