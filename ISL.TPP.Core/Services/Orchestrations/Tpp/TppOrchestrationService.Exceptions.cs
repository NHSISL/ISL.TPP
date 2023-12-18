// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using ISL.TPP.Core.Models.Foundations.Files.Exceptions;
using ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions;
using Xeptions;

namespace ISL.TPP.Core.Services.Orchestrations.Tpp
{
    internal partial class TppOrchestrationService : ITppOrchestrationService
    {
        private delegate ValueTask<List<string>> ReturningStringListFunction();

        private async ValueTask<List<string>> TryCatch(ReturningStringListFunction returningStringListFunction)
        {
            try
            {
                return await returningStringListFunction();
            }
            catch (InvalidArgumentTppOrchestrationException invalidArgumentTppOrchestrationException)
            {
                throw CreateAndLogValidationException(invalidArgumentTppOrchestrationException);
            }
            catch (DocumentValidationException documentValidationException)
            {
                throw CreateAndLogDependencyValidationException(documentValidationException);
            }
            catch (DocumentDependencyValidationException documentDependencyValidationException)
            {
                throw CreateAndLogDependencyValidationException(documentDependencyValidationException);
            }
            catch (FileValidationException fileValidationException)
            {
                throw CreateAndLogDependencyValidationException(fileValidationException);
            }
            catch (FileDependencyValidationException fileDependencyValidationException)
            {
                throw CreateAndLogDependencyValidationException(fileDependencyValidationException);
            }
            catch (DocumentDependencyException documentDependencyException)
            {
                throw CreateAndLogDependencyException(documentDependencyException);
            }
            catch (DocumentServiceException documentServiceException)
            {
                throw CreateAndLogDependencyException(documentServiceException);
            }
            catch (FileDependencyException fileDependencyException)
            {
                throw CreateAndLogDependencyException(fileDependencyException);
            }
            catch (FileServiceException fileServiceException)
            {
                throw CreateAndLogDependencyException(fileServiceException);
            }
            catch (Exception exception)
            {
                var failedTppServiceException =
                    new FailedTppOrchestrationServiceException(
                        message: "Failed TPP orchestration service occurred, please contact support",
                        innerException: exception);

                throw CreateAndLogServiceException(failedTppServiceException);
            }
        }

        private TppOrchestrationValidationException CreateAndLogValidationException(Xeption exception)
        {
            var downloadOrchestrationValidationException =
                new TppOrchestrationValidationException(
                    message: "TPP orchestration validation errors occurred, please try again.",
                    exception);

            this.loggingBroker.LogError(downloadOrchestrationValidationException);

            return downloadOrchestrationValidationException;
        }

        private TppOrchestrationDependencyValidationException
            CreateAndLogDependencyValidationException(Xeption exception)
        {
            var tppOrchestrationDependencyValidationException =
                new TppOrchestrationDependencyValidationException(
                    message: "TPP orchestration dependency validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            this.loggingBroker.LogError(tppOrchestrationDependencyValidationException);

            return tppOrchestrationDependencyValidationException;
        }

        private TppOrchestrationDependencyException
            CreateAndLogDependencyException(Xeption exception)
        {
            var tppOrchestrationDependencyException =
                new TppOrchestrationDependencyException(
                    message: "TPP orchestration dependency error occurred, fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            this.loggingBroker.LogError(tppOrchestrationDependencyException);

            throw tppOrchestrationDependencyException;
        }

        private TppOrchestrationServiceException CreateAndLogServiceException(Xeption exception)
        {
            var tppServiceException =
                new TppOrchestrationServiceException(
                    message: "TPP orchestration service error occurred, contact support.",
                    innerException: exception);

            this.loggingBroker.LogError(tppServiceException);

            return tppServiceException;
        }
    }
}
