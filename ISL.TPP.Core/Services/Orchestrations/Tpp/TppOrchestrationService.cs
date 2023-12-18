// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISL.TPP.Core.Brokers.Loggings;
using ISL.TPP.Core.Models.Orchestrations.TPP;
using ISL.TPP.Core.Services.Foundations.Documents;
using ISL.TPP.Core.Services.Foundations.Files;

namespace ISL.TPP.Core.Services.Orchestrations.Tpp
{
    internal class TppOrchestrationService : ITppOrchestrationService
    {
        private readonly IFileService fileService;
        private readonly IDocumentService documentService;
        private readonly TppConfiguration tppConfiguration;
        private readonly ILoggingBroker loggingBroker;

        public TppOrchestrationService(
            IFileService fileService,
            IDocumentService documentService,
            TppConfiguration tppConfiguration,
            ILoggingBroker loggingBroker)
        {
            this.fileService = fileService;
            this.documentService = documentService;
            this.tppConfiguration = tppConfiguration;
            this.loggingBroker = loggingBroker;
        }

        public async ValueTask<List<string>> ProcessFilesAsync()
        {
            List<string> files = new List<string>();

            List<string> filePaths = await this.fileService
                .RetrieveListOfFilesAsync(this.tppConfiguration.TppPickupFolder);

            string manifestFile = this.tppConfiguration.TppManifestFile;

            if (filePaths.Any(filePath => System.IO.Path.GetFileName(filePath) == manifestFile))
            {
                throw new NotImplementedException();
            }

            return files;
        }
    }
}
