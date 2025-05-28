using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Threading.Channels;

namespace BackgroundJobProcessor;

public class Program
{
    public static async Task Main(string[] args)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddSingleton<IJobQueue, InMemoryJobQueue>();
                });

        await builder.RunConsoleAsync();
    }
}

public interface IJobQueue
{
    ValueTask<Func<Task>> DequeueAsync(CancellationToken cancellationToken);

    void Enqueue(Func<Task> job);
}

public class InMemoryJobQueue : IJobQueue
{
    readonly Channel<Func<Task>> _queue = Channel.CreateUnbounded<Func<Task>>();

    public ValueTask<Func<Task>> DequeueAsync(CancellationToken cancellationToken)
    { return _queue.Reader.ReadAsync(cancellationToken); }

    public void Enqueue(Func<Task> job) { _queue.Writer.TryWrite(job); }
}

public class Worker : BackgroundService
{
    readonly IJobQueue _jobQueue;
    readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger, IJobQueue jobQueue)
    {
        _logger = logger;
        _jobQueue = jobQueue;

        _jobQueue.Enqueue(() => SimulateWorkAsync("Job 1"));
        _jobQueue.Enqueue(() => SimulateWorkAsync("Job 2"));
        _jobQueue.Enqueue(() => SimulateWorkAsync("Job 3"));
        _jobQueue.Enqueue(() => SimulateWorkAsync("Job 4"));
        _jobQueue.Enqueue(() => SimulateWorkAsync("Job 5"));
        _jobQueue.Enqueue(() => SimulateWorkWithConfigureAwait("Job 6"));
        _jobQueue.Enqueue(() => SimulateWorkWithConfigureAwait("Job 7"));
        _jobQueue.Enqueue(() => SimulateWorkWithConfigureAwait("Job 8"));
        _jobQueue.Enqueue(() => SimulateWorkWithConfigureAwait("Job 9"));
        _jobQueue.Enqueue(() => SimulateWorkWithConfigureAwait("Job 10"));
    }

    async Task SimulateWorkAsync(string name)
    {
        Stopwatch sw = Stopwatch.StartNew();
        await Task.Delay(1000);
        sw.Stop();
        _logger.LogInformation("[{Name}] Finished in {Time} ms (default await)", name, sw.ElapsedMilliseconds);
    }

    async Task SimulateWorkWithConfigureAwait(string name)
    {
        Stopwatch sw = Stopwatch.StartNew();
        await Task.Delay(1000).ConfigureAwait(false);
        sw.Stop();
        _logger.LogInformation("[{Name}] Finished in {Time} ms (ConfigureAwait(false))", name, sw.ElapsedMilliseconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Job Processor started.");

        while(!stoppingToken.IsCancellationRequested)
        {
            Func<Task> job = await _jobQueue.DequeueAsync(stoppingToken);
            try
            {
                await job();
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Job execution failed.");
            }
        }
    }
}
