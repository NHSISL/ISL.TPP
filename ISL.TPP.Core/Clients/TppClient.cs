// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Files;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Brokers.Storages.Blobs;
using ISL.TPP.Core.Clients.Imports;
using ISL.TPP.Core.Models.Brokers.CsvMappers;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Configurations.Retries;
using ISL.TPP.Core.Services.Foundations.CsvMappers;
using ISL.TPP.Core.Services.Foundations.Documents;
using ISL.TPP.Core.Services.Foundations.Files;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ISL.TPP.Core.Clients
{
    public partial class TppClient : ITppClient
    {
        public TppClient(
            TppConfiguration tppConfiguration,
            ILoggerFactory? loggerFactory = null)
        {
            IServiceProvider serviceProvider = RegisterServices(tppConfiguration, loggerFactory);
            InitialiseClients(serviceProvider);
        }

        internal TppClient(
         TppConfiguration tppConfiguration,
         IServiceCollection serviceCollection,
         ILoggerFactory? loggerFactory = null)
        {
            IServiceProvider serviceProvider =
                RegisterServices(tppConfiguration, loggerFactory, serviceCollection, acceptanceTests: true);

            InitialiseClients(serviceProvider);
        }

        public IImportClient Imports { get; private set; }

        private void InitialiseClients(IServiceProvider serviceProvider)
        {
            Imports = serviceProvider.GetRequiredService<IImportClient>();
        }

        private static IServiceProvider RegisterServices(
            TppConfiguration tppConfiguration,
            ILoggerFactory? loggerFactory,
            IServiceCollection? serviceCollection = null,
            bool acceptanceTests = false)
        {

            if (loggerFactory is null)
            {
                loggerFactory = LoggerFactory.Create(builder => { });
            }

            ILogger<LoggingBroker> loggingBroker = loggerFactory.CreateLogger<LoggingBroker>();

            ValidateTppConfiguration(tppConfiguration);

            if (serviceCollection is null)
            {
                serviceCollection = new ServiceCollection();
            }

            if (!acceptanceTests)
            {
                IEnumerable<BlobStorageSettings> blobStoragesSettings = tppConfiguration.BlobStoragesSettings;

                serviceCollection
                    .AddSingleton(_ => blobStoragesSettings)
                    .AddTransient<IBlobStorageBroker, BlobStorageBroker>()
                    .AddTransient<IFileBroker, FileBroker>()
                    .AddTransient<IDateTimeBroker, DateTimeBroker>()
                    .AddTransient<ISubscriberAgreementService, SubscriberAgreementService>();
            }

            serviceCollection
                .AddSingleton(tppConfiguration)

                .AddSingleton<IRetryConfig>(_ =>
                {
                    var retryConfig = tppConfiguration.RetryConfig;

                    return new RetryConfig(
                        maxRetryAttempts: retryConfig.MaxRetryAttempts,
                        pauseBetweenFailures: retryConfig.PauseBetweenFailures
                    );
                })

                .AddTransient(_ => loggingBroker)
                .AddTransient<ILoggingBroker, LoggingBroker>()
                .AddTransient<ICsvMapperBroker, CsvMapperBroker>()
                .AddTransient<IFileService, FileService>()
                .AddTransient<IDocumentService, DocumentService>()
                .AddTransient<IImportClient, ImportClient>()
                .AddTransient<ICsvMapperService, CsvMapperService>()
                .AddTransient<ITppOrchestrationService, TppOrchestrationService>();

            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider;
        }
    }
}
