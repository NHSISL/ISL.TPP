// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Diagnostics;

namespace ISL.TPP.WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<Worker> logger;
        private Timer timer;
        private readonly int timerIntervalInMinutes;

        public Worker(IConfiguration configuration, ILogger<Worker> logger)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            timerIntervalInMinutes = this.configuration.GetValue<int>("TimerIntervalInMinutes");
            string eventSourceName = this.configuration.GetValue<string>("Logging:EventLog:SourceName");
            string logName = this.configuration.GetValue<string>("Logging:EventLog:LogName");
            CreateEventLogSource(eventSourceName, logName);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(timerIntervalInMinutes));
            return base.StartAsync(cancellationToken);
        }

        private void DoWork(object state)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            logger.LogDebug("Debug: {time}", DateTimeOffset.Now);
            logger.LogWarning("Warning: {time}", DateTimeOffset.Now);
            logger.LogError("Error: {time}", DateTimeOffset.Now);
            // Add your background task logic here
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
            catch (Exception)
            {
            }
        }
    }
}
