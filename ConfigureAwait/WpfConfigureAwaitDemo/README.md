# 🖥️ WpfConfigureAwaitDemo

A .NET 9 WPF application demonstrating the effects of using `ConfigureAwait(false)` in UI applications.

---

## ✨ What It Demonstrates

- **Default `await`**: Safely updates the UI after an asynchronous operation.
- **`ConfigureAwait(false)`**: Attempts to update the UI from a background thread, resulting in a cross-thread exception.

---

## 🚦 How to Use

1. Run the application.
2. Click **"With Context"** to see a safe UI update after a delay.
3. Click **"Without Context"** to trigger an exception by updating the UI from a non-UI thread.

---

## 📝 Key Takeaway

- Use `ConfigureAwait(false)` in library or backend code, but **not** when you need to update the UI after an async operation in WPF or WinForms.

---

