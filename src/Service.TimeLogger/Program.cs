using System;
using System.Net;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MySettingsReader;
using Service.TimeLogger.Settings;

namespace Service.TimeLogger
{
    public class Program
    {
        public const string SettingsFileName = ".myjeteducation";

        public static SettingsModel Settings { get; private set; }

        public static ILoggerFactory LogFactory { get; private set; }

        public static Func<T> ReloadedSettings<T>(Func<SettingsModel, T> getter) => () =>
        {
            var settings = SettingsReader.GetSettings<SettingsModel>(SettingsFileName);
            return getter.Invoke(settings);
        };

        public static void Main(string[] args)
        {
            Console.Title = "MyJetEducation Service.TimeLogger";

            Settings = SettingsReader.GetSettings<SettingsModel>(SettingsFileName);

            using ILoggerFactory loggerFactory = LogConfigurator.ConfigureElk("MyJetEducation", Settings.SeqServiceUrl, Settings.ElkLogs);
            ILogger<Program> logger = loggerFactory.CreateLogger<Program>();
            LogFactory = loggerFactory;

            try
            {
                logger.LogInformation("Application is being started");

                CreateHostBuilder(loggerFactory, args).Build().Run();

                logger.LogInformation("Application has been stopped");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Application has been terminated unexpectedly");
            }
        }

        public static IHostBuilder CreateHostBuilder(ILoggerFactory loggerFactory, string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    string httpPort = Environment.GetEnvironmentVariable("HTTP_PORT") ?? "8080";
                    string grpcPort = Environment.GetEnvironmentVariable("GRPC_PORT") ?? "80";

                    Console.WriteLine($"HTTP PORT: {httpPort}");
                    Console.WriteLine($"GRPC PORT: {grpcPort}");

                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, int.Parse(httpPort), o => o.Protocols = HttpProtocols.Http1);
                        options.Listen(IPAddress.Any, int.Parse(grpcPort), o => o.Protocols = HttpProtocols.Http2);
                    });

                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(loggerFactory);
                    services.AddSingleton(typeof (ILogger<>), typeof (Logger<>));
                });
    }
}
