// ----------------------
// Program.cs
// ----------------------

using System;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.InMemory; // اگر SQL Server نداری
using Microsoft.Extensions.DependencyInjection;

namespace HangfireApp;
class Program
{
    static void Main(string[] args)
    {
        // ----------------------
        // 4️⃣  سرویس‌های DI
        // ----------------------
        var services = new ServiceCollection();
        services.AddTransient<IEmailService , EmailService>();
        services.AddTransient<EmailJob>();
        services.AddTransient<SimpleJob>();

        // ----------------------
        // 5️⃣  تنظیم Hangfire
        // ----------------------
        // اگر SQL Server داری، از UseSqlServerStorage استفاده کن
        // GlobalConfiguration.Configuration.UseSqlServerStorage("Server=.;Database=HangfireDB;Trusted_Connection=True;");

        // برای نمونه ساده بدون SQL Server:
        GlobalConfiguration.Configuration.UseInMemoryStorage();

        // اضافه کردن Retry پیشرفته
        GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
        {
            Attempts = 5 ,                   // تعداد Retry
            DelaysInSeconds = new int[] { 10 , 30 , 60 } // فواصل بین Retry
        });

        // ساخت سرویس provider
        var serviceProvider = services.BuildServiceProvider();

        // ----------------------
        // 6️⃣  Start Background Job Server
        // ----------------------
        using ( var server = new BackgroundJobServer() )
        {
            Console.WriteLine("[Hangfire] سرور شروع شد. دسترسی به داشبورد /hangfire در وب نیست چون Console است.");

            // ----------------------
            // 7️⃣ Fire-and-Forget Job
            // ----------------------
            BackgroundJob.Enqueue<SimpleJob>(job => job.Execute());

            // ----------------------
            // 8️⃣ Delayed Job (اجرای بعد از 10 ثانیه)
            // ----------------------
            BackgroundJob.Schedule<SimpleJob>(job => job.Execute() , TimeSpan.FromSeconds(10));

            // ----------------------
            // 9️⃣ Recurring Job (اجرای هر 30 ثانیه)
            // ----------------------
            RecurringJob.AddOrUpdate<SimpleJob>(
                "recurring-job" ,
                job => job.Execute() ,
                Cron.Minutely() // هر دقیقه
            );

            // ----------------------
            // 🔟 Job با پارامتر و DI (ارسال ایمیل)
            // ----------------------
            RecurringJob.AddOrUpdate<EmailJob>(
                "daily-email-job" ,
                job => job.Execute("user@example.com") ,
                Cron.Minutely() // برای تست هر دقیقه، در پروژه واقعی Cron.Daily(9,0)
            );

            // ----------------------
            // 1️⃣1️⃣ Job Chaining (Job دوم بعد از Job اول)
            // ----------------------
            var firstJobId = BackgroundJob.Enqueue<SimpleJob>(job => job.Execute());
            BackgroundJob.ContinueJobWith(firstJobId , () => Console.WriteLine("[ChainedJob] Job دوم بعد از Job اول اجرا شد"));

            // ----------------------
            // 1️⃣2️⃣ نگه داشتن Console برای اجرای Jobها
            // ----------------------
            Console.WriteLine("برای خروج Ctrl+C یا Enter را فشار دهید...");
            Console.ReadLine();
        }
    }
}

// ----------------------
// 1️⃣ سرویس ایمیل (مثال Dependency Injection)
// ----------------------
public interface IEmailService
{
    void Send(string to , string subject , string body);
}

public class EmailService : IEmailService
{
    public void Send(string to , string subject , string body)
    {
        Console.WriteLine($"[EmailService] ایمیل ارسال شد به {to}, موضوع: {subject}");
        // شبیه‌سازی خطای تصادفی برای تست Retry
        if ( new Random().Next(0 , 3) == 0 )
            throw new Exception("خطای شبیه‌سازی ارسال ایمیل");
    }
}

// ----------------------
// 2️⃣ Job ایمیل
// ----------------------
public class EmailJob
{
    private readonly IEmailService _emailService;
    public EmailJob(IEmailService emailService) => _emailService = emailService;

    // Job با پارامتر ایمیل
    public void Execute(string email)
    {
        Console.WriteLine($"[EmailJob] شروع اجرای Job به {email} در {DateTime.Now}");
        _emailService.Send(email , "گزارش روزانه" , "محتوای گزارش...");
        Console.WriteLine($"[EmailJob] اتمام Job در {DateTime.Now}");
    }
}

// ----------------------
// 3️⃣ Job ساده برای Fire-and-Forget و Chaining
// ----------------------
public class SimpleJob
{
    public void Execute() => Console.WriteLine($"[SimpleJob] اجرا شد: {DateTime.Now}");
}
