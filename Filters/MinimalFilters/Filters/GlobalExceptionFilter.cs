using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MinimalFilters.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        // لاگ کردن و تبدیل به پاسخ JSON امن
        Console.WriteLine($"[ExceptionFilter] {context.Exception.GetType().Name}: {context.Exception.Message}");

        var json = new { error = "An error occurred." , detail = context.Exception.Message };
        context.Result = new ObjectResult(json) { StatusCode = 500 };
        context.ExceptionHandled = true;
    }
}
