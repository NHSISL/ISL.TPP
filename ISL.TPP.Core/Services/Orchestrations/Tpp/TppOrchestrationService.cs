// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Force.DeepCloner;
using ISL.TPP.Core.Brokers.DateTimes;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Models;
using ISL.TPP.Core.Models.Brokers.Storages.Blobs;
using ISL.TPP.Core.Models.Configurations;
using ISL.TPP.Core.Models.Foundations.Documents;
using ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions;
using ISL.TPP.Core.Services.Foundations.CsvMappers;
using ISL.TPP.Core.Services.Foundations.Documents;
using ISL.TPP.Core.Services.Foundations.Files;
using Xeptions;

namespace ISL.TPP.Core.Services.Orchestrations.Tpp
{
    internal partial class TppOrchestrationService : ITppOrchestrationService
    {
        private readonly IFileService fileService;
        private readonly IDocumentService documentService;
        private readonly ICsvMapperService csvMapperService;
        private readonly TppConfiguration tppConfiguration;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public TppOrchestrationService(
            IFileService fileService,
            IDocumentService documentService,
            ICsvMapperService csvMapperService,
            TppConfiguration tppConfiguration,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.fileService = fileService;
            this.documentService = documentService;
            this.csvMapperService = csvMapperService;
            this.tppConfiguration = tppConfiguration;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask ProcessFilesAsync() =>
            TryCatch(async () =>
            {
                ValidateConfigurationSettings();
                var exceptions = new List<Exception>();

                foreach (string reportingGroup in this.tppConfiguration.ReportingGroups)
                {
                    try
                    {
                        var reportingGroupFolder = Path.Combine(
                            this.tppConfiguration.TppPickupFolder,
                            reportingGroup);

                        await ProcessReportingGroupFilesAsync(reportingGroupFolder, reportingGroup);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }

                    try
                    {
                        var reportingGroupFolder = Path.Combine(
                            this.tppConfiguration.TppPickupFolder,
                            reportingGroup,
                            tppConfiguration.TppWorkingFolders.ReProcess);

                        await ProcessReportingGroupReprocessFolderFilesAsync(reportingGroupFolder, reportingGroup);
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

            });

        virtual internal async ValueTask ProcessReportingGroupReprocessFolderFilesAsync(
            string reprocessingFolder,
            string reportingGroup)
        {
            List<string> foldersToProcess = await this.fileService.RetrieveListOfSubFoldersAsync(reprocessingFolder);
            var exceptions = new List<Exception>();

            foreach (string folder in foldersToProcess)
            {
                try
                {
                    await ProcessReportingGroupFilesAsync(folder, reportingGroup);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                        $"Error processing folder '{folder}'");

                    this.loggingBroker.LogError(ex);
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException($"Unable to land document(s)", exceptions);
            }
        }

        virtual internal async ValueTask ProcessReportingGroupFilesAsync(
            string reportingGroupFolder,
            string reportingGroup)
        {
            var exceptions = new List<Exception>();

            if (!await this.fileService.CheckIfDirectoryExistsAsync(reportingGroupFolder))
            {
                await this.fileService.CreateDirectoryAsync(reportingGroupFolder);
            }

            List<string> filePaths = await this.fileService
                .RetrieveListOfFilesAsync(reportingGroupFolder);

            string manifestFile = this.tppConfiguration.TppManifestFile;

            if (filePaths.Any(filePath => filePath.EndsWith(manifestFile, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"Manifest file found in {reportingGroupFolder}");

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"Processing {filePaths.Count} file(s)...");

                List<string> manifestFileLastList = filePaths.DeepClone();

                string manifestFilePath = manifestFileLastList
                    .FindLast(file => file.EndsWith(manifestFile, StringComparison.OrdinalIgnoreCase));

                if (manifestFilePath != null)
                {
                    manifestFileLastList.Remove(manifestFilePath);
                    manifestFileLastList.Add(manifestFilePath);
                }

                byte[] manifestData = await this.fileService.ReadFromFileAsync(manifestFilePath);
                string manifestDataString = Encoding.UTF8.GetString(manifestData);

                List<Manifest> manifest = await this.csvMapperService
                    .MapCsvToObjectAsync<Manifest>(
                        data: manifestDataString,
                        hasHeaderRecord: true);

                var manifestDateTime = manifest.First().DateExtractTo;
                bool allSuccessFull = true;

                foreach (string filePath in manifestFileLastList)
                {
                    try
                    {
                        var blobDestinationPath =
                            $"{reportingGroup}" +
                            $"\\{manifestDateTime}" +
                            $"\\{filePath.Replace(reportingGroupFolder, "")}";

                        blobDestinationPath = blobDestinationPath.Replace("\\\\", "\\");

                        bool isSuccess = await WriteFileToDestinationAsync(
                            sourceFilePath: filePath,
                            blobDestinationPath);

                        if (isSuccess == false)
                        {
                            allSuccessFull = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                            $"Error processing file '{filePath}'");

                        this.loggingBroker.LogError(ex);
                        exceptions.Add(ex);
                    }
                }

                foreach (string filePath in manifestFileLastList)
                {
                    try
                    {
                        var cleanupDestinationFolder = $"{tppConfiguration.TppPickupFolder}\\{reportingGroup}" +
                            $"\\{(allSuccessFull
                                ? this.tppConfiguration.TppWorkingFolders.Processed
                                : this.tppConfiguration.TppWorkingFolders.Errored)}" +
                            $"\\{manifestDateTime}" +
                            $"\\{filePath.Replace(reportingGroupFolder, "")}";

                        cleanupDestinationFolder = cleanupDestinationFolder.Replace("\\\\", "\\");
                        await this.fileService.MoveFileAsync(filePath, cleanupDestinationFolder);

                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                            $"Moved file to '{cleanupDestinationFolder}'");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                            $"Error processing file '{filePath}'");

                        this.loggingBroker.LogError(ex);
                        exceptions.Add(ex);
                    }
                }

                string sourceFolder = await this.fileService.GetDirectoryAsync(manifestFileLastList.Last());

                List<string> files = await this.fileService
                    .RetrieveListOfFilesAsync(path: sourceFolder, searchOption: SearchOption.AllDirectories);

                if (!files.Any())
                {
                    await this.fileService.DeleteDirectoryAsync(sourceFolder, recursive: true);
                }

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"Finished processing {manifestFileLastList.Count} file(s).");
            }

            if (exceptions.Any())
            {
                throw new AggregateException($"Unable to land {exceptions.Count} document(s)", exceptions);
            }
        }

        virtual internal async ValueTask<bool> WriteFileToDestinationAsync(
            string sourceFilePath,
            string destinationFilePath)
        {
            try
            {
                var file = await this.fileService.ReadFromFileAsync(sourceFilePath);
                ValidateFile(file);

                var document = new Document
                {
                    FileName = destinationFilePath,
                    DocumentData = file
                };

                List<BlobStorageSettings> activeDestination =
                    this.tppConfiguration.BlobStoragesSettings.Where(config => config.Enabled).ToList();

                bool allSuccessfull = true;

                foreach (BlobStorageSettings blobStorageSettings in activeDestination)
                {
                    try
                    {
                        await this.documentService.AddDocumentAsync(
                            document,
                            blobStorageSettings);

                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                            $"File '{sourceFilePath}' to destination '{destinationFilePath}' on " +
                            $"{blobStorageSettings.AzureBlobServiceUri}{blobStorageSettings.AzureBlobContainer}");
                    }
                    catch (Exception exception)
                    {
                        string message =
                            $"Unable to write file '{sourceFilePath}' to destination '{destinationFilePath}' on " +
                            $"{blobStorageSettings.Name}";

                        FailedDocumentTppOrchestrationServiceException failedDocumentTppOrchestrationServiceException =
                            new FailedDocumentTppOrchestrationServiceException(
                                message,
                                innerException: exception as Xeption);

                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - {message}");
                        this.loggingBroker.LogError(failedDocumentTppOrchestrationServiceException);
                        allSuccessfull = false;
                    }
                }

                return allSuccessfull;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
