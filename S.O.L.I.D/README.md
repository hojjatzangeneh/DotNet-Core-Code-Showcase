# 🍔 FoodOrdering SOLID Example

> A sample project to demonstrate SOLID principles in C#

---

## ✨ About the Project
This is a simple **Food Ordering System** designed with **SOLID principles**.  
The goal is to show how clean architecture enables maintainable, testable, and extensible code.

---

## 🏗️ Project Architecture
```
FoodOrdering
│
├── Domain              → Entities, Value Objects
├── Application         → Use Cases, Services, Interfaces
├── Infrastructure      → Technical Implementations (Repositories, Payment, Notification)
└── Program.cs          → Composition Root (DI, startup)
```

---

## 🧩 SOLID in Action
- **S (Single Responsibility):** Each class has a single responsibility.
- **O (Open/Closed):** New features can be added without modifying existing code.
- **L (Liskov Substitution):** Subclasses are interchangeable.
- **I (Interface Segregation):** Interfaces are small and specific.
- **D (Dependency Inversion):** Depends on abstractions, not implementations.

---

## 🚀 How to Run
```bash
git clone https://github.com/hojjatzangeneh/DotNet-Core-Code-Showcase.git
cd S.O.L.I.D/FoodOrdering
dotnet run
```

### 📌 Sample Output
```
(Email) To: hojjat.zangeneh@gmail.com - Order 1234 received
Order placed: 1234
```

---

## 🎯 Features
- Full implementation of **SOLID** principles
- Dependency Injection
- Swappable Repository & Notification implementations
- Ready to extend with EF Core and real-world services

---

## 🛠️ Future Improvements
- Connect to EF Core
- Add Unit Tests
- Add Logging
- Build a real ASP.NET Core API

---

## ❤️ Author
👨 💻 Hojjat Zangeneh  
📧 [hojjat.zangeneh@gmail.com](mailto:hojjat.zangeneh@gmail.com)

⭐ If you found this project useful, don’t forget to give it a star!


# 🍔 FoodOrdering SOLID Example

> یک پروژه نمونه برای نشان دادن اصول SOLID در C#

---

## ✨ درباره پروژه
این پروژه یک سیستم ساده ی سفارش غذاست که با استفاده از اصول **SOLID** طراحی شده.  
هدف اینه که نشون بدیم چطور میشه با رعایت معماری تمیز، کدی قابل نگه داری، تست پذیر و توسعه پذیر نوشت.

---

## 🏗️ معماری پروژه
```
FoodOrdering
│
├── Domain              → موجودیت ها (Entities, Value Objects)
├── Application         → منطق یوزکیس ها (Use Cases, Services, Interfaces)
├── Infrastructure      → پیاده سازی فنی (Repositories, Payment, Notification)
└── Program.cs          → Composition Root (DI, startup)
```

---

## 🧩 اصول SOLID در این پروژه
- **S (Single Responsibility):** هر کلاس تنها یک مسئولیت دارد.
- **O (Open/Closed):** افزودن قابلیت جدید بدون تغییر کد موجود.
- **L (Liskov Substitution):** زیرکلاس ها قابل جایگزینی هستند.
- **I (Interface Segregation):** اینترفیس ها کوچک و تخصصی هستند.
- **D (Dependency Inversion):** وابستگی ها به abstraction است نه implementation.

---

## 🚀 نحوه اجرا
```bash
git clone https://github.com/hojjatzangeneh/DotNet-Core-Code-Showcase.git
cd S.O.L.I.D/FoodOrdering
dotnet run
```

### 📌 خروجی نمونه
```
(Email) To: hojjat.zangeneh@gmail.com - Order 1234 received
Order placed: 1234
```

---

## 🎯 ویژگی ها
- پیاده سازی کامل اصول **SOLID**
- استفاده از **Dependency Injection**
- Repository و Notification قابل تعویض
- آماده برای توسعه با EF Core و سرویس های واقعی

---

## 🛠️ آینده
- اتصال به EF Core
- اضافه کردن Unit Test ها
- اضافه کردن Logging
- ساخت API با ASP.NET Core

---

## ❤️ نویسنده
👨 💻 حجت زنگنه  
📧 [hojjat.zangeneh@gmail.com](mailto:hojjat.zangeneh@gmail.com)

⭐ اگر این پروژه براتون مفید بود، ستاره یادتون نره!
