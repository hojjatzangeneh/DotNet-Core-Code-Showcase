using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GRPC_Server.Controllers;

[Route("/")]
[ApiController]
public class HomeController : ControllerBase
{
    public IActionResult Get()
    {
        return Ok("Hi World");
    }
}
