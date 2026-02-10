using System;
using System.Net;

using FluentScheduler;

Console.WriteLine("=== FluentScheduler v6 Clean Demo Started ===");

// ============================
// 1️⃣ Every 2 seconds
// ============================
new Schedule(
    () => Console.WriteLine($"[1] Every 2 seconds: {DateTime.Now}") ,
    run => run.Every(2).Seconds()
);

// ============================
// 2️⃣ Once, after 5 seconds
// ============================
new Schedule(
    () => Console.WriteLine($"[2] Run once after 5 seconds: {DateTime.Now}") ,
    run => run.OnceIn(5).Seconds()
);

// ============================
// 3️⃣ Now and every 5 minutes
// ============================
new Schedule(
    () => Console.WriteLine($"[3] Now and every 5 minutes: {DateTime.Now}") ,
    run => run.Now().AndEvery(5).Minutes()
);

// ============================
// 4️⃣ Every weekday
// ============================
new Schedule(
    () => Console.WriteLine($"[4] Weekday job: {DateTime.Now}") ,
    run => run.EveryWeekday()
);

// ============================
// 5️⃣ Every weekend
// ============================
new Schedule(
    () => Console.WriteLine($"[5] Weekend job: {DateTime.Now}") ,
    run => run.EveryWeekend()
);

// ============================
// 6️⃣ Everyday at 21:15
// ============================
new Schedule(
    () => Console.WriteLine($"[6] Everyday at 21:15: {DateTime.Now}") ,
    run => run.Every(1).Days().At(21 , 15)
);

// ============================
// 7️⃣ Every 20th day of the month
// ============================
new Schedule(
    () => Console.WriteLine($"[7] Every 20th of month: {DateTime.Now}") ,
    run => run.Every(1).Months().On(20)
);

// ============================
// 8️⃣ Everyday except Mondays
// ============================
new Schedule(
    () => Console.WriteLine($"[8] Everyday except Monday: {DateTime.Now}") ,
    run => run.Every(1).Days().Except(DayOfWeek.Monday)
);

// ============================
// 9️⃣ Everyday between 01:00 and 04:00
// ============================
new Schedule(
    () => Console.WriteLine($"[9] Between 01:00 and 04:00: {DateTime.Now}") ,
    run => run.Now().AndEvery(1).Days().Between(1 , 0 , 4 , 0)
);

// ============================
// 🔟 Cron: Every 5 minutes
// ============================
new Schedule(
    () => Console.WriteLine($"[10] Cron job every 5 minutes: {DateTime.Now}") ,
    "*/5 * * * *"
);

// ============================
// 11️⃣ Async job example
// ============================
new Schedule(
    async () =>
    {
        try
        {
            using var client = new WebClient();
            var content = await client.DownloadStringTaskAsync("http://example.com");
            Console.WriteLine($"[11] Async job completed, length: {content.Length}");
        }
        catch ( Exception ex )
        {
            Console.WriteLine($"[11] Async job error: {ex.Message}");
        }
    } ,
    run => run.Now()
);

// ============================
// 12️⃣ Multiple schedules together
// ============================
var s1 = new Schedule(
    () => Console.WriteLine($"[12a] A minute just passed: {DateTime.Now}") ,
    run => run.Every(1).Minutes()
);

var s2 = new Schedule(
    () => Console.WriteLine($"[12b] 5 minutes just passed: {DateTime.Now}") ,
    run => run.Every(5).Minutes()
);

var s3 = new Schedule(
    () => Console.WriteLine($"[12c] 10 minutes just passed: {DateTime.Now}") ,
    run => run.Every(10).Minutes()
);

var schedules = new[] { s1 , s2 , s3 };
schedules.Start(); // Start all schedules

// ============================
// 13️⃣ Long-running schedule with delay
// ============================
var longJob = new Schedule(
    () =>
    {
        Console.WriteLine("[13] Long job started, waiting 5 seconds...");
        Thread.Sleep(5000);
        Console.WriteLine("[13] Long job finished");
    } ,
    run => run.Every(1).Seconds()
);

longJob.Start();

// ============================
// 14️⃣ Stop demo
// ============================
Console.WriteLine("Press ENTER to stop longJob...");
Console.ReadLine();
longJob.Stop(); // Stop just the longJob

Console.WriteLine("Press ENTER to stop all schedules and block...");
Console.ReadLine();
schedules.StopAndBlock(); // Stop all schedules

Console.WriteLine("=== Demo finished ===");
Console.WriteLine("Press ENTER to exit...");
Console.ReadLine();
