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
        private Timer timer;
        private readonly int timerIntervalInMinutes;

        public Worker(IConfiguration configuration, ILogger<Worker> logger)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            timerIntervalInMinutes = this.configuration.GetValue<int>("timerIntervalInMinutes");
            string eventSourceName = this.configuration.GetValue<string>("Logging:EventLog:SourceName");
            string logName = this.configuration.GetValue<string>("Logging:EventLog:LogName");
            CreateEventLogSource(eventSourceName, logName);
            var tppManifestFile = configuration.GetValue<string>("tppManifestFile");
            var tppPickupFolder = configuration.GetValue<string>("tppPickupFolder");
            var blobStorageSettings = configuration.GetSection("blobStorage").Get<BlobStorageSettings>();

            var tppConfiguration = new TppConfiguration
            {
                TppManifestFile = tppManifestFile,
                TppPickupFolder = tppPickupFolder,
                TimerIntervalInMinutes = timerIntervalInMinutes,
                BlobStorageSettings = blobStorageSettings,
                RetryConfig = new RetryConfig(maxRetryAttempts: 3, pauseBetweenFailuresInMilliseconds: 100)
            };

            tppClient = new TppClient(tppConfiguration);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(timerIntervalInMinutes));
            return base.StartAsync(cancellationToken);
        }

        private async void DoWork(object state)
        {
            try
            {
                await this.tppClient.Imports.ProcessFilesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during DoWork.");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);
            return base.StopAsync(cancellationToken);
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
