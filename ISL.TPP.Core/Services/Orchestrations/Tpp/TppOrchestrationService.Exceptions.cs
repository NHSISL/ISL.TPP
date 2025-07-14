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
        private delegate ValueTask ReturningNothingFunction();
        private delegate ValueTask<List<string>> ReturningStringListFunction();
        private delegate ValueTask<string> ReturningStringFunction();

        private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
        {
            try
            {
                await returningNothingFunction();
            }
            catch (InvalidArgumentTppOrchestrationException invalidArgumentTppOrchestrationException)
            {
                throw await CreateAndLogValidationException(invalidArgumentTppOrchestrationException);
            }
            catch (DocumentValidationException documentValidationException)
            {
                throw await CreateAndLogDependencyValidationException(documentValidationException);
            }
            catch (DocumentDependencyValidationException documentDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationException(documentDependencyValidationException);
            }
            catch (FileValidationException fileValidationException)
            {
                throw await CreateAndLogDependencyValidationException(fileValidationException);
            }
            catch (FileDependencyValidationException fileDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationException(fileDependencyValidationException);
            }
            catch (DocumentDependencyException documentDependencyException)
            {
                throw await CreateAndLogDependencyException(documentDependencyException);
            }
            catch (DocumentServiceException documentServiceException)
            {
                throw await CreateAndLogDependencyException(documentServiceException);
            }
            catch (FileDependencyException fileDependencyException)
            {
                throw await CreateAndLogDependencyException(fileDependencyException);
            }
            catch (FileServiceException fileServiceException)
            {
                throw await CreateAndLogDependencyException(fileServiceException);
            }
            catch (AggregateException aggregateException)
            {
                var failedTppServiceException =
                    new FailedTppOrchestrationServiceException(
                        message: "Failed TPP orchestration service occurred, please contact support",
                        innerException: aggregateException);

                throw await CreateAndLogServiceException(failedTppServiceException);
            }
            catch (Exception exception)
            {
                var failedTppServiceException =
                    new FailedTppOrchestrationServiceException(
                        message: "Failed TPP orchestration service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedTppServiceException);
            }
        }

        private async ValueTask<string> TryCatch(ReturningStringFunction returningStringFunction)
        {
            try
            {
                return await returningStringFunction();
            }
            catch (InvalidArgumentTppOrchestrationException invalidArgumentTppOrchestrationException)
            {
                throw await CreateAndLogValidationException(invalidArgumentTppOrchestrationException);
            }
            catch (DocumentValidationException documentValidationException)
            {
                throw await CreateAndLogDependencyValidationException(documentValidationException);
            }
            catch (DocumentDependencyValidationException documentDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationException(documentDependencyValidationException);
            }
            catch (FileValidationException fileValidationException)
            {
                throw await CreateAndLogDependencyValidationException(fileValidationException);
            }
            catch (FileDependencyValidationException fileDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationException(fileDependencyValidationException);
            }
            catch (DocumentDependencyException documentDependencyException)
            {
                throw await CreateAndLogDependencyException(documentDependencyException);
            }
            catch (DocumentServiceException documentServiceException)
            {
                throw await CreateAndLogDependencyException(documentServiceException);
            }
            catch (FileDependencyException fileDependencyException)
            {
                throw await CreateAndLogDependencyException(fileDependencyException);
            }
            catch (FileServiceException fileServiceException)
            {
                throw await CreateAndLogDependencyException(fileServiceException);
            }
            catch (Exception exception)
            {
                var failedTppServiceException =
                    new FailedTppOrchestrationServiceException(
                        message: "Failed TPP orchestration service occurred, please contact support",
                        innerException: exception);

                throw await CreateAndLogServiceException(failedTppServiceException);
            }
        }

        private async ValueTask<TppOrchestrationValidationException> CreateAndLogValidationException(Xeption exception)
        {
            var downloadOrchestrationValidationException =
                new TppOrchestrationValidationException(
                    message: "TPP orchestration validation errors occurred, please try again.",
                    exception);

            await this.loggingBroker.LogErrorAsync(downloadOrchestrationValidationException);

            return downloadOrchestrationValidationException;
        }

        private async ValueTask<TppOrchestrationDependencyValidationException>
            CreateAndLogDependencyValidationException(Xeption exception)
        {
            var tppOrchestrationDependencyValidationException =
                new TppOrchestrationDependencyValidationException(
                    message: "TPP orchestration dependency validation error occurred, " +
                        "fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(tppOrchestrationDependencyValidationException);

            return tppOrchestrationDependencyValidationException;
        }

        private async ValueTask<TppOrchestrationDependencyException>
            CreateAndLogDependencyException(Xeption exception)
        {
            var tppOrchestrationDependencyException =
                new TppOrchestrationDependencyException(
                    message: "TPP orchestration dependency error occurred, fix the errors and try again.",
                    innerException: exception.InnerException as Xeption);

            await this.loggingBroker.LogErrorAsync(tppOrchestrationDependencyException);

            return tppOrchestrationDependencyException;
        }

        private async ValueTask<TppOrchestrationServiceException> CreateAndLogServiceException(Xeption exception)
        {
            var tppServiceException =
                new TppOrchestrationServiceException(
                    message: "TPP orchestration service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(tppServiceException);

            return tppServiceException;
        }
    }
}
