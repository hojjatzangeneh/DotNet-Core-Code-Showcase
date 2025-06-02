# 🛠️ BackgroundJobProcessor

A .NET 9 console app demonstrating background job processing with and without `ConfigureAwait(false)`.

---

## Features

- Enqueues and processes jobs using a hosted background service.
- Logs execution time for each job.
- Compares default `await` vs `await ... ConfigureAwait(false)` in a non-UI, non-web context.

---

## Why It Matters

- Shows how `ConfigureAwait(false)` can improve throughput in background services by avoiding unnecessary context capture.
- Useful for scalable, high-performance job processing scenarios.

---

## Running
