# 🚀 MinimalFilters (.NET 9)

A simple and educational project demonstrating **all types of ASP.NET Core filters**:  
**Authorization**, **Resource**, **Action**, **Result**, **Exception**, and **Endpoint Filters**

پروژه ای ساده و آموزشی برای نمایش تمام انواع **فیلترهای ASP.NET Core**  
از جمله: **Authorization**, **Resource**, **Action**, **Result**, **Exception** و **Endpoint Filters**

---

## 🧠 Purpose | هدف پروژه

**EN:**  
This project shows the full ASP.NET Core filter pipeline — when each filter runs, what it’s used for,  
and how you can use them for caching, authorization, logging, validation, and error handling.

**FA:**  
این پروژه به صورت عملی نشان می دهد که هر فیلتر در چه مرحله ای اجرا می شود و برای چه کاربردهایی است،  
از جمله: **Caching، Authorization، Logging، Validation** و **Error Handling**.

---

## ⚙️ Technologies | تکنولوژی ها

| Feature | Description |
|----------|--------------|
| **.NET 9** | Target framework |
| **Minimal API + MVC** | Used together to demonstrate both types of filters |
| **Global Filters** | Registered at startup (Program.cs) |
| **Custom Filters** | Implemented manually for full control |
| **In-Memory Cache** | Used in Resource Filter |

| ویژگی | توضیح |
|--------|-------|
| **.NET 9** | فریم ورک مورد استفاده |
| **Minimal API + MVC** | برای نمایش هر دو نوع فیلتر استفاده شده |
| **Global Filters** | ثبت شده در سطح سراسری (Program.cs) |
| **Custom Filters** | پیاده سازی دستی برای کنترل بیشتر |
| **In-Memory Cache** | در Resource Filter استفاده شده |

---

## 🧩 Filter Execution Order | ترتیب اجرای فیلترها

```text
Authorization Filter
↓
Resource Filter
↓
Action Filter
↓
(Controller Action)
↓
Result Filter
↓
(Response)
↑
(Exception Filter - on errors)
```
---
## Minimal API Endpoint Filter
```text
Endpoint Filter → Handler → Response
```

## 🏗️ Project Structure | ساختار پروژه

```text
MinimalFilters/
│
├── Controllers/
│   └── SampleController.cs
│
├── Filters/
│   ├── CustomAuthorizationFilter.cs
│   ├── CustomResourceFilter.cs
│   ├── CustomActionFilter.cs
│   ├── CustomResultFilter.cs
│   ├── GlobalExceptionFilter.cs
│
├── Models/
│   └── Product.cs
│
├── Program.cs
├── MinimalFilters.csproj
└── README.md
```

🚀 Run the Project | نحوه اجرا
---
```bash
dotnet restore
dotnet run
```
🧪 Test Examples | مثال های تست
---
🛡 Authorization Filter

EN: Requires header X-User: demo.

Without it → 401 Unauthorized.

FA: نیاز به هدر X-User دارد. در غیر این صورت پاسخ 401 دریافت می کنید.

⚙️ Resource Filter
---
EN: Caches the response if query fromCache=1 is present.

FA: اگر fromCache=1 در Query باشد، پاسخ cache می شود.

⚡ Action Filter
---
EN: Logs before and after executing the Action.

FA: قبل و بعد از اجرای اکشن لاگ می گیرد و زمان اجرا را نمایش 
می دهد.

🎯 Result Filter
---
EN: Adds a custom response header X-Result-Filter: Applied.

FA: قبل از ارسال پاسخ به کاربر، یک هدر جدید اضافه می کند.

🚨 Exception Filter
---
EN: Adds a custom response header X-Result-Filter: Applied.

FA: قبل از ارسال پاسخ به کاربر، یک هدر جدید اضافه می‌کند.

```text
{
  "error": "An error occurred.",
  "detail": "No product with id 13 allowed!"
}
```
🧱 Endpoint Filter
---
EN: Used in Minimal API. Requires header X-Api-Key: secret.

FA: در مسیر Minimal API استفاده شده و نیاز به هدر X-Api-Key: secret دارد.

```
curl -H "X-Api-Key: secret" https://localhost:5001/minimal/products/1
```
🧭 Filter Summary | خلاصه فیلترها
---
| Filter Type   | Interface              | Purpose                | Scope       |
| ------------- | ---------------------- | ---------------------- | ----------- |
| Authorization | `IAuthorizationFilter` | Access control         | MVC         |
| Resource      | `IResourceFilter`      | Caching, performance   | MVC         |
| Action        | `IActionFilter`        | Pre/Post Action logic  | MVC         |
| Result        | `IResultFilter`        | Modify response result | MVC         |
| Exception     | `IExceptionFilter`     | Handle errors          | MVC         |
| Endpoint      | `IEndpointFilter`      | Minimal API filters    | Minimal API |

| نوع فیلتر     | Interface              | هدف                  | محل استفاده |
| ------------- | ---------------------- | -------------------- | ----------- |
| Authorization | `IAuthorizationFilter` | کنترل دسترسی         | MVC         |
| Resource      | `IResourceFilter`      | کش و منابع           | MVC         |
| Action        | `IActionFilter`        | منطق قبل/بعد از اکشن | MVC         |
| Result        | `IResultFilter`        | تغییر نتیجه خروجی    | MVC         |
| Exception     | `IExceptionFilter`     | مدیریت خطا           | MVC         |
| Endpoint      | `IEndpointFilter`      | فیلترهای Minimal API | Minimal API |

❤️ Support | پشتیبانی
---
EN:
If you find this useful, give it a ⭐ on GitHub or fork it and extend the examples.

FA:
اگر پروژه برات مفید بود، لطفاً در گیت‌هاب ⭐ بده یا Fork کن و مثال‌های خودت رو اضافه کن.

📄 License | مجوز
---
MIT License © 2025
Made with ❤️ for learning ASP.NET Core Filters

ساخته شده با ❤️ برای آموزش فیلترهای ASP.NET Core