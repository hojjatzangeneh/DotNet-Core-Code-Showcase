# Web API Performance Analyzer: Async Await vs ConfigureAwait

📌 **هدف:** بررسی و مقایسه‌ی عملکرد متدهای `async/await` در محیط ASP.NET Core با و بدون `ConfigureAwait(false)` برای بهینه‌سازی مصرف منابع و بهبود performance.

---

## ✨ چرا این پروژه مهمه؟

در برنامه‌نویسی async با C#، استفاده‌ی درست از `ConfigureAwait(false)` می‌تونه:
- از استفاده‌ی غیرضروری از **SynchronizationContext** جلوگیری کنه،
- باعث کاهش **latency** بشه،
- از **deadlock** در بعضی از محیط‌ها (مثل WPF/WinForms) جلوگیری کنه،
- و در مجموع باعث بهبود عملکرد در اپ‌های تحت وب، کتابخانه‌ها و job processorها بشه.

---

## 🔧 تکنولوژی‌های استفاده شده

- [.NET 8](https://dotnet.microsoft.com/)
- ASP.NET Core Web API
- Serilog برای لاگ‌گیری
- Swagger UI برای تست راحت endpointها
- Stopwatch برای سنجش زمان اجرا
- Docker-ready (در صورت نیاز به دیپلوی ساده)

---

## 🧪 تست و مقایسه‌ی دو حالت

این پروژه دو endpoint دارد:

| Endpoint                  | توضیح |
|--------------------------|-------|
| `/api/test/await-default` | استفاده از `await` به صورت پیش‌فرض (با context) |
| `/api/test/await-configure` | استفاده از `await ConfigureAwait(false)` (بدون context) |

هر کدام زمان اجرای async operation را لاگ می‌کنند تا تفاوت‌ها قابل مشاهده باشد.

---

## 📈 خروجی نمونه لاگ

```bash
[Info] Default Await: Execution time = 120ms
[Info] ConfigureAwait(false): Execution time = 98ms
