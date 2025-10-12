using StackExchange.Redis;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

Console.WriteLine("🚀 Starting RedLock test...");

var redisPassword = "4992040";

//var redisEndpoints = new[]
//{
//    new ConfigurationOptions
//    {
//        EndPoints = { "redis1:6379" },
//        Password = redisPassword,
//        AbortOnConnectFail = false
//    },
//    new ConfigurationOptions
//    {
//        EndPoints = { "redis2:6379" },
//        Password = redisPassword,
//        AbortOnConnectFail = false
//    },
//    new ConfigurationOptions
//    {
//        EndPoints = { "redis3:6379" },
//        Password = redisPassword,
//        AbortOnConnectFail = false
//    }
//};
var redisEndpoints = new[]
{
    new ConfigurationOptions
    {
        EndPoints = { "192.168.0.246:16379" },
        Password = redisPassword,
        AbortOnConnectFail = false
    },
    new ConfigurationOptions
    {
        EndPoints = { "192.168.0.246:16380" },
        Password = redisPassword,
        AbortOnConnectFail = false
    },
    new ConfigurationOptions
    {
        EndPoints = { "192.168.0.246:16381" },
        Password = redisPassword,
        AbortOnConnectFail = false
    }
};
// اتصال به Redis‌ها
var multiplexers = new List<RedLockMultiplexer>();
foreach ( var cfg in redisEndpoints )
{
    var mux = await ConnectionMultiplexer.ConnectAsync(cfg);
    multiplexers.Add(new RedLockMultiplexer(mux));
}

using var redlockFactory = RedLockFactory.Create(multiplexers);

// حالا تست گرفتن لاک
var resource = "locks:order:123";
var expiry = TimeSpan.FromSeconds(30);
var wait = TimeSpan.FromSeconds(10);
var retry = TimeSpan.FromMilliseconds(200);

Console.WriteLine("🔒 Trying to acquire distributed lock...");

using var redLock = await redlockFactory.CreateLockAsync(resource , expiry , wait , retry);

if ( redLock.IsAcquired )
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("✅ Lock acquired! Safe to proceed.");
    Console.ResetColor();

    // عملیات حساس
    await Task.Delay(3000);

    Console.WriteLine("🟢 Done. Lock will be released automatically.");
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("⚠️ Could not acquire lock. Try again later.");
    Console.ResetColor();
}

Console.WriteLine("🏁 Finished.");
