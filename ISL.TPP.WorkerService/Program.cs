// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

namespace ISL.TPP.WorkerService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .UseWindowsService();

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("local.appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables("ISL_TPP_");
            });

            builder.ConfigureServices((hostContext, services) =>
            {
                services
                    .AddHostedService<Worker>()
                    .AddLogging(loggingBuilder =>
                    {
                        var eventLogSourceName =
                            hostContext.Configuration.GetValue<string>("Logging:EventLog:SourceName");

                        var logName =
                                hostContext.Configuration.GetValue<string>("Logging:EventLog:LogName");

                        loggingBuilder.AddEventLog(options =>
                        {
                            options.SourceName = eventLogSourceName;
                            options.LogName = logName;
                        });
                    });
            });

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}