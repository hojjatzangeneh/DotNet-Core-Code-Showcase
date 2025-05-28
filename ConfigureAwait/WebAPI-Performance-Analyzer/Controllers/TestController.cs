using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;


namespace WebAPI_Performance_Analyzer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController(ILogger<TestController> logger) : ControllerBase
{
    static readonly HttpClient _httpClient = new HttpClient();
    readonly ILogger<TestController> _logger = logger;

    async Task<string> SimulateHttpCall()
    {
        // Simulate an HTTP call
        HttpResponseMessage response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
        return await response.Content.ReadAsStringAsync();
    }

    [HttpGet("await-default")]
    public async Task<IActionResult> AwaitDefault()
    {
        Stopwatch sw = Stopwatch.StartNew();
        _ = await SimulateHttpCall(); // context حفظ می‌شود
        sw.Stop();

        _logger.LogInformation("[Default Await] Time elapsed: {Time} ms", sw.ElapsedMilliseconds);
        return Ok(new { message = "Completed with default await", elapsed = sw.ElapsedMilliseconds });
    }

    [HttpGet("await-configure")]
    public async Task<IActionResult> AwaitWithConfigureAwait()
    {
        Stopwatch sw = Stopwatch.StartNew();
        _ = await SimulateHttpCall().ConfigureAwait(false); // context حفظ نمی‌شود
        sw.Stop();

        _logger.LogInformation("[ConfigureAwait(false)] Time elapsed: {Time} ms", sw.ElapsedMilliseconds);
        return Ok(new { message = "Completed with ConfigureAwait(false)", elapsed = sw.ElapsedMilliseconds });
    }
}
