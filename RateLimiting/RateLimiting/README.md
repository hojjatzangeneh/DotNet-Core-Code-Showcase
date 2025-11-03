# Rate Limiting — Minimal API (.NET 9)

### 🇬🇧 English + 🇮🇷 فارسی — In One File

This project demonstrates **ASP.NET Core Rate Limiting** in a clean and minimal way using **.NET 9 Minimal APIs**.  
این پروژه نمونه‌ای کامل و تمیز از **Rate Limiting در ASP.NET Core** با استفاده از **Minimal API در .NET 9** است.

---

## 📘 Overview | نمای کلی

**Rate Limiting** controls how many requests a client can send in a given time period — to prevent abuse, overload, or DoS.  
**Rate Limiting** یعنی کنترل تعداد درخواست‌هایی که یک کاربر در بازهٔ زمانی مشخص می‌تواند بفرستد — برای جلوگیری از فشار زیاد، حملات، یا استفادهٔ نادرست.

این پروژه چند نوع limiter مختلف را نمایش می‌دهد:
- **Token Bucket** (سراسری برای همه endpointها)
- **Fixed Window** (برای endpointهای حساس مثل لاگین)
- **Partitioned Per-IP** (برای هر IP یک limiter جدا)
- **Sliding Window Per-API-Key** (محدودسازی دقیق به ازای هر API key)

همه اینها با یک **هندلر سفارشی برای HTTP 429** همراه هستند تا پاسخ واضح و JSON برگردد.

---

## 🧠 Concepts | مفاهیم اصلی

| مفهوم | توضیح کوتاه فارسی | English Summary |
|--------|------------------|----------------|
| **Token Bucket** | هر درخواست یک توکن مصرف می‌کند. توکن‌ها در بازه‌های زمانی بازسازی می‌شوند. | Requests consume tokens; tokens refill over time. |
| **Fixed Window** | تعداد مجاز در هر بازهٔ زمانی ثابت (مثلاً ۵ درخواست در ۶۰ ثانیه). | Fixed time window with limited requests per period. |
| **Sliding Window** | مثل Fixed Window ولی با بازهٔ متحرک دقیق‌تر. | More accurate rolling time window limit. |
| **Partitioned Limiter** | محدودسازی جدا برای هر کاربر یا IP. | Separate counters per IP or user key. |

---

## ⚙️ How It Works | نحوهٔ عملکرد

در `Program.cs` چند limiter تعریف شده است:

```csharp
builder.Services.AddRateLimiter(options =>
{
    // 1️⃣ Global Token Bucket Limiter
    options.AddTokenBucketLimiter("global-token", opt =>
    {
        opt.TokenLimit = 20; // ظرفیت سطل
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10; // چند درخواست در صف می‌ماند
        opt.ReplenishmentPeriod = TimeSpan.FromMilliseconds(50);
        opt.TokensPerPeriod = 1; // چند توکن در هر بازه اضافه شود
        opt.AutoReplenishment = true; // بازسازی خودکار
    });

    // 2️⃣ Fixed Window Limiter
    options.AddFixedWindowLimiter("fixed-login", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(60);
        opt.PermitLimit = 5; // حداکثر ۵ درخواست در هر ۶۰ ثانیه
    });

    // 3️⃣ Per-IP Token Bucket Limiter
    options.AddPolicy<string>("per-ip", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetTokenBucketLimiter(ip, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,
            QueueLimit = 5,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            TokensPerPeriod = 2,
            AutoReplenishment = true
        });
    });

    // 4️⃣ Per-API-Key Sliding Window Limiter
    options.AddPolicy("per-api-key", context =>
    {
        var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString() ?? "anon";
        return RateLimitPartition.GetSlidingWindowLimiter(apiKey, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    // ✋ Custom Rejection Handler (HTTP 429)
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.Headers["Retry-After"] = "10";
        await context.HttpContext.Response.WriteAsJsonAsync(new { error = "Too many requests" }, cancellationToken: ct);
    };
});
```

---

## 🚀 Run the Project | اجرای پروژه

```bash
dotnet build
dotnet run
```

سرور در حالت پیش‌فرض روی `https://localhost:5001` اجرا می‌شود.

---

## 🧪 Test with Postman | تست با Postman

1. **Global Token Limiter**
   - Endpoint: `GET https://localhost:5001/`
   - بفرستید بیش از ۲۰ درخواست پشت‌سر‌هم → بعد از حد مجاز پاسخ `429 Too Many Requests` می‌گیرید.

2. **Fixed Window Limiter (Login)**
   - Endpoint: `POST https://localhost:5001/login`
   - Body JSON:
     ```json
     {
       "username": "admin",
       "password": "123"
     }
     ```
   - بعد از ۵ درخواست در ۶۰ ثانیه، پاسخ 429 می‌دهد.

3. **Per-IP Limiter**
   - Endpoint: `GET https://localhost:5001/ip-limited`
   - هر IP جداگانه شمارش می‌شود.

4. **Per-API-Key Sliding Window**
   - Endpoint: `GET https://localhost:5001/apikey-data`
   - Add header:
     ```
     X-Api-Key: test123
     ```
   - درخواست‌های هر کلید جدا شمارش می‌شود. اگر چند کلید مختلف بفرستید، هرکدام سهم جدا دارند.

---

## 💡 Tips | نکات

- اگر چند instance از برنامه دارید، محدودسازی محلی فقط برای همان instance است. برای اشتراک داده‌ها از Redis استفاده کنید.
- برای تشخیص IP واقعی در پشت پروکسی، از `ForwardedHeadersMiddleware` استفاده کنید.
- می‌توانید با ابزارهایی مثل **k6**, **JMeter**, یا **Postman Runner** تست بار انجام دهید.

---

## 🧱 Project Files | فایل‌های پروژه

```
RateLimitingDemo/
├── Program.cs
├── RateLimitingDemo.csproj
└── README.md
```

**RateLimitingDemo.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

---

## 🏁 Summary | خلاصه

✅ نمونهٔ کامل از Rate Limiting در .NET 9 با چهار مدل اصلی  
✅ همراه با هندلر سفارشی 429  
✅ تست‌پذیر با Postman یا Curl  
✅ مناسب برای یادگیری یا گیت‌هاب شخصی

> ✨ Created by Hojjat Zangeneh — educational demo for ASP.NET Core 9 rate limiting.
