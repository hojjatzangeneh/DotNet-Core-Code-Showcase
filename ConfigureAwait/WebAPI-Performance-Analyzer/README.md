# Web API Performance Analyzer: Async Await vs ConfigureAwait

📌 **Goal:** Analyze and compare the performance of `async/await` methods in ASP.NET Core with and without `ConfigureAwait(false)` to optimize resource usage and improve overall performance.

---

## ✨ Why This Project Matters

In asynchronous C# programming, proper use of `ConfigureAwait(false)` can:

- Prevent unnecessary use of the **SynchronizationContext**,
- Reduce overall **latency**,
- Avoid potential **deadlocks** in environments like WPF and WinForms,
- And improve performance in web applications, reusable libraries, and job processors.

---

## 🔧 Technologies Used

- [.NET 8](https://dotnet.microsoft.com/)
- ASP.NET Core Web API
- Serilog for logging
- Swagger UI for easy endpoint testing
- `Stopwatch` for execution time measurement
- Docker-ready (for simple deployment if needed)

---

## 🧪 Test and Comparison of Two Scenarios

This project includes two endpoints to compare `await` behavior in different configurations:

| Endpoint                    | Description |
|----------------------------|-------------|
| `/api/test/await-default`   | Uses `await` with the default behavior (captures context) |
| `/api/test/await-configure` | Uses `await ConfigureAwait(false)` (no context capture) |

Each endpoint logs the execution time of an async operation to highlight the differences.

---

## 📈 Sample Log Output

```bash
[Info] Default Await: Execution time = 120ms  
[Info] ConfigureAwait(false): Execution time = 98ms
