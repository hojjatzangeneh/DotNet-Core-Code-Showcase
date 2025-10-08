using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Context;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace CorrelationIdSample.Background
{
    public class BackgroundJob
    {
        public Func<CancellationToken, Task> Work { get; set; } = _ => Task.CompletedTask;
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
    }

    public class SimpleBackgroundQueue
    {
        private readonly ConcurrentQueue<BackgroundJob> _q = new();

        public void Enqueue(BackgroundJob job)
        {
            _q.Enqueue(job);
        }

        public bool TryDequeue(out BackgroundJob? job) => _q.TryDequeue(out job);
    }

    // Hosted service runner


    public class BackgroundQueueRunner : BackgroundService
    {
        private readonly SimpleBackgroundQueue _queue;
        private readonly ILogger<BackgroundQueueRunner> _logger;

        public BackgroundQueueRunner(SimpleBackgroundQueue queue, ILogger<BackgroundQueueRunner> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BackgroundQueueRunner started");
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var job))
                {
                    // Run in a Task and propagate the CorrelationId to LogContext explicitly
                    _ = Task.Run(async () =>
                    {
                        using (LogContext.PushProperty("CorrelationId", job.CorrelationId))
                        {
                            _logger.LogInformation("Background job started (from queue)");
                            try
                            {
                                await job.Work(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Background job failed");
                            }
                            _logger.LogInformation("Background job finished");
                        }
                    }, stoppingToken);
                }
                await Task.Delay(500, stoppingToken);
            }
        }
    }
}