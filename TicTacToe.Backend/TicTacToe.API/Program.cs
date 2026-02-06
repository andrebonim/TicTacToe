using Microsoft.AspNetCore.Hosting;
using Serilog;
using TicTacToe.API;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             // === CONFIGURAÇÃO DO SERILOG (ANTES do ConfigureWebHostDefaults) ===
             .UseSerilog((context, services, configuration) => configuration
                 .MinimumLevel.Warning()
                 .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                 .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                 .Enrich.FromLogContext()
                 .WriteTo.File(
                     path: "logs/log-.txt",
                     rollingInterval: RollingInterval.Day,
                     retainedFileCountLimit: 7,
                     outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                 )             
             )
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder
                     .ConfigureAppConfiguration((hostingContext, config) =>
                     {
                         var env = hostingContext.HostingEnvironment;

                         config
                             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                             .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                             .AddEnvironmentVariables();

#if DEBUG
                         if (System.IO.File.Exists("local.settings.json"))
                             config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
#endif
                     })
                     .ConfigureLogging(logging =>
                     {
                         logging.ClearProviders();
                         logging.AddConsole();
                         logging.SetMinimumLevel(LogLevel.Warning);
                     })
                     .UseStartup<Startup>();
             });
}