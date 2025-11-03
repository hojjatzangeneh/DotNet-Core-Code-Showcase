# LINQ Professional Examples — .NET Class Library

این پروژه یک کتابخانهٔ آموزشی و حرفه‌ای برای یادگیری و استفاده از **LINQ** در #C است. هدف آن ارائهٔ مثال‌های جامع از قابلیت‌های مختلف LINQ در دنیای واقعی است.

---

## 📁 ساختار پروژه

- **LinqExamples.cs** — شامل مجموعه‌ای از متدهای استاتیک برای نمایش کاربردهای مختلف LINQ
- **Models.cs** — کلاس‌های ساده مانند `Person`, `Order`, `Product` برای تست و تمرین
- **README.md** — توضیحات پروژه (همین فایل)

---

## 💡 ویژگی‌ها و پوشش مباحث

| دسته | مثال‌ها |
|------|----------|
| **پایه‌ای (Basic)** | `Where`, `Select`, `OrderBy`, `ThenBy`, `Distinct`, `Take`, `Skip` |
| **پیشرفته (Advanced)** | `GroupBy`, `Join`, `GroupJoin`, `SelectMany`, `Aggregate`, `Zip`, `Except`, `Union` |
| **مقایسه و مجموعه‌ها** | Equality comparerها، عملیات `Intersect`, `Except`, `DistinctBy` |
| **اجرای Deferred و Immediate** | تفاوت بین `IEnumerable` و `List` / استفاده از `ToList()`, `Count()`, `Any()` |
| **Partitioning** | `Take`, `Skip`, `TakeWhile`, `SkipWhile`, صفحه‌بندی (Paging) |
| **Lookup & Dictionary** | مثال‌هایی با `ToLookup()` و `ToDictionary()` |
| **Expression Trees** | ساخت کوئری‌های پویا و تبدیل Expression به LINQ |
| **Async / EF Core Integration** | توضیح چگونگی استفاده از LINQ در EF Core و Queryable Extensions |

---

## 🧩 مثال استفاده (Usage)

```csharp
using LinqExamplesLibrary;

var basic = new LinqExamples();

// فیلتر کردن داده‌ها
var adults = basic.FilterAdults();

// گروهبندی داده‌ها
var grouped = basic.GroupOrdersByPerson();

// جوین بین دو مجموعه
var joined = basic.JoinPeopleWithOrders();

// اجرای Aggregate برای محاسبه مجموع
var total = basic.CalculateTotalSales();

// اجرای Query Expression به جای Method Syntax
var qSyntax = basic.QuerySyntaxExample();
```

---

## 🧠 نکات مهم

- تمام مثال‌ها **با داده‌های فرضی (mock)** پیاده‌سازی شده‌اند.
- هر متد می‌تواند به عنوان تمرین یا تست واحد (Unit Test) استفاده شود.
- برای EF Core می‌توان همان متدها را با `IQueryable` جایگزین کرد.

---

## 🧰 ابزار و نسخه‌ها

- **.NET 9 / C# 12**
- IDE پیشنهادی: Visual Studio 2022 یا Rider
- سازگار با: **Console App**, **xUnit Tests**, یا هر پروژهٔ آموزشی LINQ

---

## 🌍 English Summary

This library provides a **comprehensive collection of LINQ examples** for .NET developers. It covers from basic filtering and projection to advanced grouping, joining, and expression tree generation.

### Highlights:
- Real-world ready examples
- Organized by topic
- Works with `IEnumerable` and `IQueryable`
- Suitable for learning, teaching, or unit testing LINQ skills

### How to use:
Simply reference the library in any C# project:
```bash
dotnet add reference LinqExamplesLibrary.csproj
```
Then call any static or instance method from the `LinqExamples` class.

---

📘 **Author:** Hojjat Zangeneh  
🧾 **License:** MIT  
🕓 **Last Updated:** November 2025
