// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Clients.Exceptions;
using ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions;
using ISL.TPP.Core.Services.Orchestrations.Tpp;
using Xeptions;

namespace ISL.TPP.Core.Clients.Imports
{
    internal class ImportClient : IImportClient
    {
        private readonly ITppOrchestrationService tppOrchestrationService;

        public ImportClient(ITppOrchestrationService tppOrchestrationService)
        {
            this.tppOrchestrationService = tppOrchestrationService;
        }

        public async ValueTask<List<string>> ProcessFilesAsync()
        {
            try
            {
                return await this.tppOrchestrationService.ProcessFilesAsync();
            }
            catch (TppOrchestrationValidationException tppOrchestrationValidationException)
            {
                throw new TppClientValidationException(
                    message: "Tpp client validation error occurred, fix errors and try again.",
                    innerException: tppOrchestrationValidationException.InnerException as Xeption,
                    data: tppOrchestrationValidationException.InnerException.Data);
            }
            catch (TppOrchestrationDependencyValidationException
                tppOrchestrationDependencyValidationException)
            {
                throw new TppClientValidationException(
                    message: "Tpp client validation error occurred, fix errors and try again.",
                    innerException: tppOrchestrationDependencyValidationException.InnerException as Xeption,
                    data: tppOrchestrationDependencyValidationException.InnerException.Data);
            }
            catch (TppOrchestrationDependencyException
                tppOrchestrationDependencyException)
            {
                throw new TppClientDependencyException(
                    message: "Tpp client dependency error occurred, contact support.",
                    innerException: tppOrchestrationDependencyException.InnerException as Xeption);
            }
            catch (TppOrchestrationServiceException
                tppOrchestrationServiceException)
            {
                throw new TppClientServiceException(
                    message: "Tpp client service error occurred, fix errors and try again.",
                    tppOrchestrationServiceException.InnerException as Xeption);
            }
        }
    }
}
