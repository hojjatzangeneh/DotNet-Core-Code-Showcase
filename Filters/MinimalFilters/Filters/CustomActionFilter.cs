using System.Diagnostics;

using Microsoft.AspNetCore.Mvc.Filters;

namespace MinimalFilters.Filters;

public class CustomActionFilter : IActionFilter
{
    private readonly Stopwatch _sw = new();

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // قبل از اکشن: لاگ / زمان سنجی / تغییر آرگومان ها
        _sw.Restart();
        var name = context.ActionDescriptor.DisplayName;
        Console.WriteLine($"[ActionFilter] OnActionExecuting -> {name}");
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // بعد از اکشن
        _sw.Stop();
        var elapsed = _sw.ElapsedMilliseconds;
        Console.WriteLine($"[ActionFilter] OnActionExecuted -> elapsed: {elapsed} ms");
    }
}
