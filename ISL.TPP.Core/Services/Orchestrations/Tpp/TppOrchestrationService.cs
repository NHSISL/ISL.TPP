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
            bool folderExists = await this.fileService.CheckIfDirectoryExistsAsync(reprocessingFolder);

            if (!folderExists)
            {
                await this.fileService.CreateDirectoryAsync(reprocessingFolder);
            }

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

                    await this.loggingBroker.LogErrorAsync(ex);
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
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"Folder not found.  Creating folder '{reportingGroupFolder}'");

                await this.fileService.CreateDirectoryAsync(reportingGroupFolder);
            }

            List<string> filePaths = await this.fileService
                .RetrieveListOfFilesAsync(reportingGroupFolder);

            List<string> distinctFilePaths = filePaths
                .Select(filePath => filePath.Trim())
                .Distinct()
                .ToList();

            if (distinctFilePaths.Count != filePaths.Count)
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"-------- LIST NOT DISTINCT ----------");
            }

            string manifestFile = this.tppConfiguration.TppManifestFile;

            if (filePaths.Any(filePath => filePath.EndsWith(manifestFile, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"Manifest file found in '{reportingGroupFolder}'");

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

                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                            $"Attempting to upload file to blobstore: file: '{filePath}', " +
                                $"destination: {blobDestinationPath}");

                        bool isSuccess = await WriteFileToDestinationAsync(
                            sourceFilePath: filePath,
                            blobDestinationPath);

                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                            $"File '{filePath}' upload to blobstore {(isSuccess ? "SUCCEEDED" : "FAILED")}");

                        if (isSuccess == false)
                        {
                            allSuccessFull = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                            $"Error processing file '{filePath}'" + Environment.NewLine +
                            $"Error: {ex.Message} {ex?.InnerException?.Message} ");

                        await this.loggingBroker.LogErrorAsync(ex);
                        exceptions.Add(ex);
                    }
                }

                try
                {
                    await CleanupFilesAsync(
                        fileList: manifestFileLastList,
                        reportingGroup,
                        reportingGroupFolder,
                        manifestDateTime,
                        allSuccessFull);
                }
                catch (Exception exception)
                {
                    string message =
                        $"{DateTime.Now.ToString("HH:mm:ss")} - " +
                        $"Unable to move files." + Environment.NewLine +
                        $"Error: {exception.Message} {exception?.InnerException?.Message} " +
                        $"{exception?.InnerException?.InnerException?.Message}";

                    Console.WriteLine(message);

                    if (exception is AggregateException)
                    {
                        foreach (var innerException in ((AggregateException)exception).InnerExceptions)
                        {
                            await this.loggingBroker.LogCriticalAsync(innerException);
                            exceptions.Add(innerException);
                        }
                    }
                    else
                    {
                        await this.loggingBroker.LogCriticalAsync(exception);
                        exceptions.Add(exception);
                    }
                }

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"Finished processing {manifestFileLastList.Count} file(s).");
            }
            else
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"No manifest file found in '{reportingGroupFolder}'.  Sleeping till next cycle...");
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
            string tempFilePath = await fileService.GetTempFileNameAsync();

            try
            {
                bool fileExists =
                    await this.fileService.CheckIfFileExistsAsync(sourceFilePath);

                if (!fileExists)
                {
                    throw new FileNotFoundException($"Unable to read file: {sourceFilePath}");
                }

                bool allSuccessfull = true;

                await using (FileStream tempWriteStream = new FileStream(
                    tempFilePath,
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true))
                {
                    await this.fileService.ReadFromFileAsync(tempWriteStream, sourceFilePath);
                }

                List<BlobStorageSettings> activeDestination =
                    this.tppConfiguration.BlobStoragesSettings.Where(config => config.Enabled).ToList();

                foreach (BlobStorageSettings blobStorageSettings in activeDestination)
                {
                    try
                    {
                        await using (FileStream tempReadStream = new FileStream(
                            tempFilePath,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read,
                            bufferSize: 81920,
                            useAsync: true))
                        {
                            await this.documentService.AddDocumentAsync(
                                tempReadStream,
                                destinationFilePath,
                                blobStorageSettings.AzureBlobContainer);
                        }

                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                            $"Port file to blobstore.  Copy from '{sourceFilePath}' " +
                            $"to destination '{destinationFilePath}' on " +
                            $"{blobStorageSettings.AzureBlobServiceUri}{blobStorageSettings.AzureBlobContainer}");
                    }
                    catch (Exception exception)
                    {
                        string message =
                            $"Unable to write file '{sourceFilePath}' to destination '{destinationFilePath}' on " +
                            $"{blobStorageSettings.Name}";

                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - {message}");

                        FailedDocumentTppOrchestrationServiceException failedDocumentTppOrchestrationServiceException =
                            new FailedDocumentTppOrchestrationServiceException(
                                message,
                                innerException: exception as Xeption);

                        await this.loggingBroker.LogErrorAsync(failedDocumentTppOrchestrationServiceException);
                        allSuccessfull = false;
                    }
                }

                return allSuccessfull;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"WriteFileToDestination error - file '{sourceFilePath}' to destination '{destinationFilePath}'" +
                    Environment.NewLine +
                    $"Error: {exception.Message} {exception?.InnerException?.Message} " +
                    $"{exception.InnerException?.InnerException?.Message}");

                return false;
            }
            finally
            {
                if (await fileService.CheckIfFileExistsAsync(tempFilePath) == true)
                {
                    await this.fileService.DeleteFileAsync(tempFilePath);
                }
            }
        }

        virtual internal async ValueTask CleanupFilesAsync(
            List<string> fileList,
            string reportingGroup,
            string reportingGroupFolder,
            string manifestDateTime,
            bool allSuccessFull)
        {
            ValidateArgumentsOnCleanupFiles(
                fileList: fileList,
                reportingGroup: reportingGroup,
                reportingGroupFolder: reportingGroupFolder,
                manifestDateTime: manifestDateTime);

            bool folderExists =
                await this.fileService.CheckIfDirectoryExistsAsync(reportingGroupFolder);

            ValidateFolderExistOnCleanupFiles(folderExists, reportingGroupFolder);

            List<Exception> exceptions = new List<Exception>();

            foreach (string filePath in fileList)
            {
                try
                {
                    bool fileExists =
                        await this.fileService.CheckIfFileExistsAsync(filePath);

                    ValidateFileExistOnCleanupFiles(fileExists, filePath);

                    var cleanupDestinationFolder =
                        Path.Combine(
                            tppConfiguration.TppPickupFolder,
                            reportingGroup,
                            allSuccessFull
                                ? this.tppConfiguration.TppWorkingFolders.Processed
                                : this.tppConfiguration.TppWorkingFolders.Errored,
                            manifestDateTime,
                            filePath.Replace(reportingGroupFolder, "").TrimStart('\\'));

                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                        $"Copy file from '{filePath}' to '{cleanupDestinationFolder}'");

                    await this.fileService.CopyFileAsync(
                        sourcePath: filePath,
                        destinationPath: cleanupDestinationFolder);

                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                        $"Deleting file '{filePath}'");

                    await this.fileService.DeleteFileAsync(filePath);

                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                        $"Completed file move from '{filePath}' to '{cleanupDestinationFolder}'");
                }
                catch (Exception ex)
                {
                    string message =
                        $"{DateTime.Now.ToString("HH:mm:ss")} - " +
                        $"Unable to move file '{filePath}' to error folder." + Environment.NewLine +
                        $"Error: {ex.Message} {ex?.InnerException?.Message} " +
                        $"{ex?.InnerException?.InnerException?.Message}";

                    Console.WriteLine(message);

                    await this.loggingBroker.LogCriticalAsync(ex);
                    exceptions.Add(ex);
                }
            }

            List<string> files = await this.fileService.RetrieveListOfFilesAsync(
                path: reportingGroupFolder,
                searchPattern: "*",
                searchOption: SearchOption.AllDirectories);

            if (files is null || !files.Any())
            {
                bool exists = await this.fileService.CheckIfDirectoryExistsAsync(reportingGroupFolder);

                if (exists)
                {
                    await this.fileService.DeleteDirectoryAsync(reportingGroupFolder, recursive: true);

                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                        $"Removed folder '{reportingGroupFolder}'");
                }
            }
            else
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - " +
                    $"Found {files.Count} file(s) in '{reportingGroupFolder}'.  Cleanup not completed");
            }

            if (exceptions.Any())
            {
                throw new AggregateException($"Unable to cleanup {exceptions.Count} file(s)", exceptions);
            }
        }
    }
}
