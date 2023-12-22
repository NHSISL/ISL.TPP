// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
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

                List<string> filePaths = await this.fileService
                    .RetrieveListOfFilesAsync(this.tppConfiguration.TppPickupFolder);

                string manifestFile = this.tppConfiguration.TppManifestFile;

                if (filePaths.Any(filePath => filePath.EndsWith(manifestFile, StringComparison.OrdinalIgnoreCase)))
                {
                    var currentDateTime = this.dateTimeBroker.GetCurrentDateTimeOffset().ToString("yyyyMMddHHmmss");

                    foreach (string filePath in filePaths)
                    {
                        var file = await this.fileService.ReadFromFileAsync(filePath);

                        var newFileName =
                            $"{currentDateTime}\\{filePath.Replace(this.tppConfiguration.TppPickupFolder, "")}";

                        newFileName = newFileName.Replace("\\\\", "\\");

                        var document = new Document
                        {
                            FileName = newFileName,
                            DocumentData = file
                        };

                        await this.documentService
                            .AddDocumentAsync(document, this.tppConfiguration.BlobStorageSettings.AzureBlobContainer);

                        await this.fileService.DeleteFileAsync(filePath);
                        files.Add(document.FileName);
                    }
                }

                return files;
            });
    }
}
