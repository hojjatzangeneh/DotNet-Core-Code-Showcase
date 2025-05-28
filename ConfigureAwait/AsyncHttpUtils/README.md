# 🚀 AsyncHttpUtils

A lightweight, extendable utility library for making robust asynchronous HTTP calls in .NET 9.

Includes built-in support for:
- ✅ Generic GET, POST, PUT, DELETE operations
- 🔁 Retry policies with [Polly](https://github.com/App-vNext/Polly)
- ⏱ Timeout handling at both client and request level
- 🧪 Simple error wrapping with `ApiResponse<T>`
- 🧩 Integration-ready for DI / HttpClientFactory

---

## 📦 Installation

```bash
dotnet add package Microsoft.Extensions.Http
dotnet add package Microsoft.Extensions.Logging.Abstractions
dotnet add package Microsoft.Extensions.Http.Polly
```

---

## 🔧 How to Use

```csharp
builder.Services.AddHttpClient<IHttpService, HttpService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(Policies.GetRetryPolicy());
```

---

## 📄 Use in Your Code

```csharp
public class MyComponent
{
    private readonly IHttpService _httpService;

    public MyComponent(IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task LoadDataAsync()
    {
        var result = await _httpService.GetAsync<MyDto>("https://api.example.com/data");

        if (result.IsSuccess)
            Console.WriteLine(result.Data);
        else
            Console.WriteLine($"Error: {result.ErrorMessage}");
    }
}
```

---

## ✅ Features

- ✔️ Minimal and extendable code
- 💡 Clean handling of async context with `ConfigureAwait(false)`
- 🛡 Built-in error resilience using `Polly`
- 📦 No third-party JSON library — uses `System.Net.Http.Json`

---

## 🤝 Contributing

Feel free to fork, improve, or suggest enhancements. PRs are welcome!

---

## 📄 License

MIT

---

## 👨‍💻 Author

Developed with ❤️ by [Hojjat Zangeneh](https://www.linkedin.com/in/hojjatzangeneh)
