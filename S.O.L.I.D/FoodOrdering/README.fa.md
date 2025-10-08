# 🍔 FoodOrdering SOLID Example

> یک پروژه نمونه برای نشان دادن اصول SOLID در C#

---

## ✨ درباره پروژه
این پروژه یک سیستم ساده‌ی سفارش غذاست که با استفاده از اصول **SOLID** طراحی شده.  
هدف اینه که نشون بدیم چطور میشه با رعایت معماری تمیز، کدی قابل نگه‌داری، تست‌پذیر و توسعه‌پذیر نوشت.

---

## 🏗️ معماری پروژه
```
FoodOrdering.SOLIDExample
│
├── Domain              → موجودیت‌ها (Entities, Value Objects)
├── Application         → منطق یوزکیس‌ها (Use Cases, Services, Interfaces)
├── Infrastructure      → پیاده‌سازی فنی (Repositories, Payment, Notification)
└── Program.cs          → Composition Root (DI, startup)
```

---

## 🧩 اصول SOLID در این پروژه
- **S (Single Responsibility):** هر کلاس تنها یک مسئولیت دارد.
- **O (Open/Closed):** افزودن قابلیت جدید بدون تغییر کد موجود.
- **L (Liskov Substitution):** زیرکلاس‌ها قابل جایگزینی هستند.
- **I (Interface Segregation):** اینترفیس‌ها کوچک و تخصصی هستند.
- **D (Dependency Inversion):** وابستگی‌ها به abstraction است نه implementation.

---

## 🚀 نحوه اجرا
```bash
git clone https://github.com/yourusername/FoodOrdering.SOLIDExample.git
cd FoodOrdering.SOLIDExample
dotnet run
```

### 📌 خروجی نمونه
```
(Email) To: customer@example.com - Order 1234 received
Order placed: 1234
```

---

## 🎯 ویژگی‌ها
- پیاده‌سازی کامل اصول **SOLID**
- استفاده از **Dependency Injection**
- Repository و Notification قابل تعویض
- آماده برای توسعه با EF Core و سرویس‌های واقعی

---

## 🛠️ آینده
- اتصال به EF Core
- اضافه کردن Unit Test ها
- اضافه کردن Logging
- ساخت API با ASP.NET Core

---

## ❤️ نویسنده
👨‍💻 حجت زنگنه  
📧 [customer@example.com](mailto:customer@example.com)

⭐ اگر این پروژه براتون مفید بود، ستاره یادتون نره!
