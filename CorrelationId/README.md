# CorrelationIdSample

Minimal ASP.NET Core sample demonstrating:
- Correlation ID middleware (`X-Correlation-ID`) that accepts incoming header or creates one.
- Serilog enrichment via `LogContext.PushProperty("CorrelationId", ...)`.
- DelegatingHandler to propagate the Correlation ID to outgoing `HttpClient` requests.
- Simple background queue example that propagates Correlation ID to background work.

## Run
- Requires .NET 9 SDK.
- `dotnet restore`
- `dotnet run`

## Try
- `curl -v http://localhost:5000/sample/ping`
- Observe console logs showing `(cid:<value>)` in the log lines.


# نمونه CorrelationId

پروژهٔ کوچک ASP.NET Core که نشان می دهد:
- Middleware برای پذیرش یا ساخت Correlation ID در هدر `X-Correlation-ID`.
- افزودن CorrelationId به لاگ ها با `LogContext.PushProperty("CorrelationId", ...)` در Serilog.
- DelegatingHandler برای ارسال CorrelationId در درخواست های خروجی `HttpClient`.
- مثال سادهٔ صف پس زمینه که CorrelationId را به کارهای پس زمینه منتقل می کند.

## اجرا
- نیاز به .NET 7 SDK دارد.
- `dotnet restore`
- `dotnet run`

## تست
- `curl -v http://localhost:5000/sample/ping`
- در لاگ کنسول مقدار `(cid:<value>)` را مشاهده کن.