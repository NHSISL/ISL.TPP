// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Diagnostics;
using ISL.TPP.Core.Clients;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Configurations.Retries;

namespace ISL.TPP.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly TppClient tppClient;
        private readonly ILogger<Worker> logger;
        private readonly int timerIntervalInMinutes = 1;

        public Worker(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (loggerFactory is null)
            {
                loggerFactory = LoggerFactory.Create(builder => { });
            }

            this.logger = loggerFactory.CreateLogger<Worker>();
            timerIntervalInMinutes = this.configuration.GetValue<int>("timerIntervalInMinutes");

            string eventSourceName = this.configuration.GetValue<string>("Logging:EventLog:SourceName");
            string logName = this.configuration.GetValue<string>("Logging:EventLog:LogName");
            CreateEventLogSource(eventSourceName, logName);

            var tppManifestFile = configuration.GetValue<string>("tppManifestFile");
            var tppPickupFolder = configuration.GetValue<string>("tppPickupFolder");
            var tppSubmissionFolder = configuration.GetValue<string>("tppSubmissionFolder");

            var blobStoragesSettings = configuration.GetSection("blobStorages").Get<List<BlobStorageSettings>>()
                ?? new List<BlobStorageSettings>();

            var tppWorkingFolders = configuration.GetSection("tppWorkingFolders").Get<TppWorkingFolders>()
                ?? new TppWorkingFolders();

            var reportingGroups = configuration.GetSection("reportingGroups").Get<List<string>>()
                ?? new List<string>();

            var tppConfiguration = new TppConfiguration
            {
                TppManifestFile = tppManifestFile,
                TppPickupFolder = tppPickupFolder,
                TppSubmissionFolder = tppSubmissionFolder,
                TimerIntervalInMinutes = timerIntervalInMinutes,
                BlobStoragesSettings = blobStoragesSettings,
                TppWorkingFolders = tppWorkingFolders,
                ReportingGroups = reportingGroups,
                RetryConfig = new RetryConfig(maxRetryAttempts: 3, pauseBetweenFailuresInMilliseconds: 100)
            };

            tppClient = new TppClient(tppConfiguration, loggerFactory);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var startTime = DateTime.UtcNow;

                try
                {
                    await this.tppClient.Imports.ProcessFilesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during ProcessFilesAsync.");
                    LogExceptionToFile(ex);
                }

                var duration = DateTime.UtcNow - startTime;
                var delay = TimeSpan.FromMinutes(timerIntervalInMinutes) - duration;

                if (delay > TimeSpan.Zero)
                {
                    try
                    {
                        await Task.Delay(delay, stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Worker is stopping...");
            await base.StopAsync(cancellationToken);
            logger.LogInformation("Worker stopped cleanly.");
        }

        private void LogExceptionToFile(Exception exception)
        {
            string logFileName = "UnhandledExceptions.log";
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"Timestamp: {DateTime.Now}");
                writer.WriteLine($"Exception Message: {exception.Message}");
                writer.WriteLine($"Stack Trace: {exception.StackTrace}");
                writer.WriteLine(new string('-', 50));
            }
        }

        private void CreateEventLogSource(string eventSourceName, string logName)
        {
            try
            {
                if (!EventLog.SourceExists(eventSourceName))
                {
                    EventLog.CreateEventSource(eventSourceName, logName);
                    logger.LogInformation($"Event Log source '{eventSourceName}' created successfully.");
                }
                else
                {
                    logger.LogInformation($"Event Log source '{eventSourceName}' already exists.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred setting up the event logger.");
                throw;
            }
        }
    }
}
