// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
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

        public async ValueTask<List<string>> ProcessFilesAsync() =>
            throw new NotImplementedException();
    }
}
