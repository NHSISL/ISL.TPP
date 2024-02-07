// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Foundations.Documents;
using ISL.TPP.Core.Services.Foundations.Documents;
using ISL.TPP.Core.Services.Foundations.Files;

namespace ISL.TPP.Core.Services.Orchestrations.Tpp
{
    internal partial class TppOrchestrationService : ITppOrchestrationService
    {
        private readonly IFileService fileService;
        private readonly IDocumentService documentService;
        private readonly TppConfiguration tppConfiguration;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public TppOrchestrationService(
            IFileService fileService,
            IDocumentService documentService,
            TppConfiguration tppConfiguration,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.fileService = fileService;
            this.documentService = documentService;
            this.tppConfiguration = tppConfiguration;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<List<string>> ProcessFilesAsync() =>
            TryCatch(async () =>
            {
                ValidateConfigurationSettings();
                List<string> files = new List<string>();
                var exceptions = new List<Exception>();

                foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
                {
                    try
                    {
                        List<string> reportingGroupFiles = await ProcessReportingGroupFiles(reportingGroup);
                        files.AddRange(reportingGroupFiles);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }

                if (exceptions.Any())
                {
                    throw new AggregateException($"Unable to land {exceptions.Count} document(s)", exceptions);
                }

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"Waiting for {this.tppConfiguration.TimerIntervalInMinutes} minute(s)...");

                return files;
            });

        private ValueTask<List<string>> ProcessReportingGroupFiles(string reportingGroup) =>
            TryCatch(async () =>
            {
                List<string> files = new List<string>();
                var exceptions = new List<Exception>();
                var reportingGroupFolder = Path.Combine(this.tppConfiguration.TppPickupFolder, reportingGroup);

                List<string> filePaths = await this.fileService
                    .RetrieveListOfFilesAsync(reportingGroupFolder);

                string manifestFile = this.tppConfiguration.TppManifestFile;

                if (filePaths.Any(filePath => filePath.EndsWith(manifestFile, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                        $"Manifest file found in {reportingGroupFolder}");

                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                        $"Processing {filePaths.Count} file(s)...");

                    var currentDateTime = this.dateTimeBroker.GetCurrentDateTimeOffset().ToString("yyyyMMddHHmmss");

                    foreach (string filePath in filePaths)
                    {
                        try
                        {
                            string file = await TryCatch(async () =>
                            {
                                var file = await this.fileService.ReadFromFileAsync(filePath);

                                ValidateFile(file);

                                var newFileName =
                                    $"{reportingGroup}\\{currentDateTime}\\{filePath.Replace(reportingGroupFolder, "")}";

                                newFileName = newFileName.Replace("\\\\", "\\");

                                var document = new Document
                                {
                                    FileName = newFileName,
                                    DocumentData = file
                                };

                                await this.documentService.AddDocumentAsync(
                                    document,
                                    container: this.tppConfiguration.BlobStorageSettings.AzureBlobContainer);

                                await this.fileService.DeleteFileAsync(filePath);

                                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                                    $"File '{document.FileName}' successfully uploaded.");

                                return document.FileName;
                            });

                            files.Add(file);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                                $"Error processing file '{filePath}'");

                            this.loggingBroker.LogError(ex);
                            exceptions.Add(ex);
                        }
                    }

                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                        $"Finished processing {files.Count} file(s).");
                }

                if (exceptions.Any())
                {
                    throw new AggregateException($"Unable to land {exceptions.Count} document(s)", exceptions);
                }

                return files;
            });
    }
}
