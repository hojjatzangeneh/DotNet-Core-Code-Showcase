using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CorrelationIdSample.Background;
using Microsoft.AspNetCore.Http;

namespace CorrelationIdSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {
        private readonly ILogger<SampleController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SimpleBackgroundQueue _queue;
        public SampleController(ILogger<SampleController> logger, IHttpClientFactory httpClientFactory, SimpleBackgroundQueue queue)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _queue = queue;
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            _logger.LogInformation("Ping endpoint called");

            // Call external (demo) using propagated CorrelationId
            var client = _httpClientFactory.CreateClient("demo");
            try
            {
                var res = await client.GetAsync("https://httpbin.org/get");
                _logger.LogInformation("External call done with status {StatusCode}", res.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "External call failed (network may be blocked in sandbox)");
            }

            // Enqueue a background job and pass correlation id explicitly
            var cid = Request.Headers["X-Correlation-ID"].ToString();
            if (string.IsNullOrWhiteSpace(cid))
                cid = Guid.NewGuid().ToString("N");

            _queue.Enqueue(new BackgroundJob
            {
                CorrelationId = cid,
                Work = async ct =>
                {
                    // simulate work
                    await Task.Delay(1000, ct);
                    _logger.LogInformation("Background work executed inside job");
                }
            });

            return Ok(new { message = "pong", correlation = cid });
        }
    }
}