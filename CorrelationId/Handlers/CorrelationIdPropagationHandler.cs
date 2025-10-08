using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace CorrelationIdSample.Handlers
{
    public class CorrelationIdPropagationHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public const string HeaderName = "X-Correlation-ID";

        public CorrelationIdPropagationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? correlationId = null;

            var ctx = _httpContextAccessor.HttpContext;
            if (ctx != null && ctx.Request.Headers.TryGetValue(HeaderName, out var v) && !string.IsNullOrWhiteSpace(v))
            {
                correlationId = v.ToString();
            }
            else
            {
                correlationId = System.Diagnostics.Activity.Current?.Tags?.FirstOrDefault(t => t.Key == "correlation_id").Value;
            }

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            if (request.Headers.Contains(HeaderName))
                request.Headers.Remove(HeaderName);

            request.Headers.Add(HeaderName, correlationId);

            return base.SendAsync(request, cancellationToken);
        }
    }
}