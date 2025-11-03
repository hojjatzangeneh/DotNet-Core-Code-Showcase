using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MinimalFilters.Filters;

public class CustomAuthorizationFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var headers = context.HttpContext.Request.Headers;
        if ( !headers.TryGetValue("X-User" , out var user) || string.IsNullOrWhiteSpace(user) )
        {
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}
