using Microsoft.AspNetCore.Mvc.Filters;

namespace MinimalFilters.Filters;

public class CustomResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // قبل از اینکه نتیجه به کاربر ارسال شود
        context.HttpContext.Response.Headers.Add("X-Result-Filter" , "Applied");
        Console.WriteLine("[ResultFilter] OnResultExecuting");
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // بعد از ارسال نتیجه
        Console.WriteLine("[ResultFilter] OnResultExecuted");
    }
}