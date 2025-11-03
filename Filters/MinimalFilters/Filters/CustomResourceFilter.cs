using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MinimalFilters.Filters;

public class HInMemoryCache
{
    private readonly Dictionary<string , object> _cache = [];
    public bool TryGet(string key , out object? value) => _cache.TryGetValue(key , out value);
    public void Set(string key , object value) => _cache[key] = value;
}

public class CustomResourceFilter(HInMemoryCache cache) : IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        // مثال: اگر آدرس با query ?fromCache=1 اومده، سعی میکنیم از cache برگردونیم
        var req = context.HttpContext.Request;
        if ( req.Query.ContainsKey("fromCache") )
        {
            var key = req.Path.ToString() + req.QueryString;
            if ( cache.TryGet(key , out var cached) )
            {
                context.Result = new JsonResult(cached);
            }
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        // بعد از اجرای action، اگر نتیجه Json باشه میتونیم cache کنیم
        if ( context.Result is JsonResult jr )
        {
            var key = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            cache.Set(key , jr.Value ?? new { });
        }
    }
}
