# 🚀 MinimalFiltersDemo (.NET 9)

A simple and educational project demonstrating **all types of ASP.NET Core filters**:  
**Authorization**, **Resource**, **Action**, **Result**, **Exception**, and **Endpoint Filters**

پروژه‌ای ساده و آموزشی برای نمایش تمام انواع **فیلترهای ASP.NET Core**  
از جمله: **Authorization**, **Resource**, **Action**, **Result**, **Exception** و **Endpoint Filters**

---

## 🧠 Purpose | هدف پروژه

**EN:**  
This project shows the full ASP.NET Core filter pipeline — when each filter runs, what it’s used for,  
and how you can use them for caching, authorization, logging, validation, and error handling.

**FA:**  
این پروژه به صورت عملی نشان می‌دهد که هر فیلتر در چه مرحله‌ای اجرا می‌شود و برای چه کاربردهایی است،  
از جمله: **Caching، Authorization، Logging، Validation** و **Error Handling**.

---

## ⚙️ Technologies | تکنولوژی‌ها

| Feature | Description |
|----------|--------------|
| **.NET 9** | Target framework |
| **Minimal API + MVC** | Used together to demonstrate both types of filters |
| **Global Filters** | Registered at startup (Program.cs) |
| **Custom Filters** | Implemented manually for full control |
| **In-Memory Cache** | Used in Resource Filter |

| ویژگی | توضیح |
|--------|-------|
| **.NET 9** | فریم‌ورک مورد استفاده |
| **Minimal API + MVC** | برای نمایش هر دو نوع فیلتر استفاده شده |
| **Global Filters** | ثبت‌شده در سطح سراسری (Program.cs) |
| **Custom Filters** | پیاده‌سازی دستی برای کنترل بیشتر |
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

## Minimal API Endpoint Filter
```text
	Endpoint Filter → Handler → Response
```text
Endpoint Filter

## 📁 Project Structure | ساختار پروژه
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
