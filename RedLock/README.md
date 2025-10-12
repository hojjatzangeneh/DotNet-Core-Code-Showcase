# 🔐 RedLock & RedLock Distributed
### 🚀 .NET Core Distributed Locking Example — API + Console  
> A showcase of distributed locking using Redis & RedLockNet across multiple nodes.

---

## 🧩 Architecture Diagram

```
             ┌────────────────────────┐
             │      Console App       │
             │  ─ Executes business   │
             │    logic with lock     │
             └──────────┬─────────────┘
                        │
                        ▼
       ┌─────────────────────────────────────┐
       │     RedLock Distributed System       │
       │ ┌───────────┐ ┌───────────┐ ┌───────────┐ │
       │ │  Redis #1 │ │  Redis #2 │ │  Redis #3 │ │
       │ │ 16379     │ │ 16380     │ │ 16381     │ │
       │ └───────────┘ └───────────┘ └───────────┘ │
       └─────────────────────────────────────┘
                        │
                        ▼
             ┌────────────────────────┐
             │         API            │
             │  Exposes endpoints to  │
             │ acquire/release locks  │
             └────────────────────────┘
```

---

## ⚙️ Projects Overview

| Project | Description | Type |
|----------|--------------|------|
| `RedLock` | Demonstrates acquiring & releasing distributed locks using RedLockNet | 🖥 Console |
| `RedLock.API` | Provides REST API endpoints for managing distributed locks | 🌐 Web API |

---

## 🌍 Quick Start

### 🐳 With Docker Compose
```bash
git clone https://github.com/hojjatzangeneh/DotNet-Core-Code-Showcase.git
cd RedLock
docker compose up -d --build
```

Access your services:
- Redis Insight / Redis nodes: `localhost:16379`, `16380`, `16381`
- API: `http://localhost:80`
- Console output: via container logs or terminal

---

## 🧠 How It Works

RedLock follows the **distributed consensus** algorithm for ensuring only one process holds a lock at any time, even across multiple Redis instances.

### 🔄 Lock Flow:
1. Each node (`redis1`, `redis2`, `redis3`) participates in voting.
2. If a quorum (≥2 of 3) grants the lock → ✅ acquired.
3. If not → ❌ "Resource is locked" message returned.
4. Lock automatically expires after TTL.

---

## 🧰 Tech Stack

| Layer | Technology |
|-------|-------------|
| Framework | .NET 9.0 |
| Cache | Redis 7 |
| Distributed Lock | RedLockNet.SERedis |
| Containerization | Docker & Docker Compose |
| Monitoring | RedisInsight |
| Language | C# |

---

## 🔑 Environment Variables

| Variable | Description | Example |
|-----------|--------------|----------|
| `REDLOCK_ENDPOINTS` | Comma-separated Redis nodes | `redis1:6379,redis2:6379,redis3:6379` |
| `ASPNETCORE_ENVIRONMENT` | Hosting environment | `Development` |
| `ASPNETCORE_URLS` | Exposed URLs | `http://+:80` |

---

## 🇮🇷 راهنمای فارسی

### 🧱 معماری پروژه

پروژه شامل دو بخش اصلی است:

- **RedLock** ➤ وظیفه اجرای منطق قفل توزیع‌شده و تست RedLock  
- **RedLock.API** ➤ ارائه API برای گرفتن و آزاد کردن قفل‌ها در محیط شبکه

سیستم از سه سرور Redis مستقل استفاده می‌کند تا طبق الگوریتم **Quorum (اکثریت)** تصمیم بگیرد که آیا قفل گرفته شود یا خیر.

---

### 🧩 دیاگرام ساختار
```
  RedLock ConsoleApp ⇆ API ⇆ Redis1, Redis2, Redis3
             ↳ Distributed Consensus (RedLock)
```

---

### 🚀 اجرای سریع با Docker Compose

```bash
git clone https://github.com/hojjatzangeneh/DotNet-Core-Code-Showcase.git
cd RedLock
docker compose up -d --build
```

سرویس‌ها:
- API در پورت **80**
- Redisها در پورت‌های **16379, 16380, 16381**
- Console App در لاگ کانتینر قابل مشاهده است

---

### 🧠 منطق قفل توزیع‌شده (RedLock)

- قفل فقط زمانی گرفته می‌شود که حداقل دو Redis از سه تا موافق باشند.  
- اگر قفل قبلاً گرفته شده باشد، پیام `Resource is locked` نمایش داده می‌شود.  
- قفل به‌صورت خودکار بعد از زمان TTL آزاد می‌شود.

---

## 📊 Example Log Output

```
✅ Lock acquired for resource 'order:42'
...processing critical section...
🔓 Lock released successfully
```

or

```
⚠️ Resource 'order:42' is locked by another process.
```

---

## 🧑‍💻 Author
👤 **Hojjat Zangeneh**  
[🔗 GitHub Repository](https://github.com/hojjatzangeneh/DotNet-Core-Code-Showcase)

---

## ⭐ Support
اگر این پروژه برات مفید بود:
- ⭐ یه Star به ریپو بده
- یا Pull Request برای بهبودها بفرست 🚀
