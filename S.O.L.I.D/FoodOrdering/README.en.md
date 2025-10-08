# 🍔 FoodOrdering SOLID Example

> A sample project to demonstrate SOLID principles in C#

---

## ✨ About the Project
This is a simple **Food Ordering System** designed with **SOLID principles**.  
The goal is to show how clean architecture enables maintainable, testable, and extensible code.

---

## 🏗️ Project Architecture
```
FoodOrdering.SOLIDExample
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
git clone https://github.com/yourusername/FoodOrdering.SOLIDExample.git
cd FoodOrdering.SOLIDExample
dotnet run
```

### 📌 Sample Output
```
(Email) To: customer@example.com - Order 1234 received
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
👨‍💻 Hojjat Zangeneh  
📧 [customer@example.com](mailto:customer@example.com)

⭐ If you found this project useful, don’t forget to give it a star!
