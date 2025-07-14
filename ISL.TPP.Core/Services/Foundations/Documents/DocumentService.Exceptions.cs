// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Azure;
using ISL.TPP.Core.Models.Foundations.Documents;
using ISL.TPP.Core.Models.Foundations.Documents.Exceptions;
using Xeptions;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal partial class DocumentService
    {
        private delegate ValueTask ReturningNothingFunction();
        private delegate ValueTask<Document> ReturningDocumentFunction();
        private delegate ValueTask<string> ReturningStringFunction();

        private async ValueTask TryCatch(ReturningNothingFunction returningNothingFunction)
        {
            try
            {
                await returningNothingFunction();
            }
            catch (NullDocumentException nullDocumentException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullDocumentException);
            }
            catch (NotFoundDocumentException notFoundDocumentException)
            {
                throw await CreateAndLogValidationExceptionAsync(notFoundDocumentException);
            }
            catch (InvalidDocumentException invalidDocumentException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidDocumentException);
            }
            catch (RequestFailedException requestFailedException)
            {
                if (requestFailedException.Status != 404)
                {
                    var failedRequestException = new FailedDocumentRequestException(
                        message: "Failed document request occurred, please contact support.",
                        innerException: requestFailedException);

                    throw await CreateAndLogDependencyExceptionAsync(failedRequestException);
                }
            }
            catch (Exception exception)
            {
                var failedDocumentServiceException =
                    new FailedDocumentServiceException(
                        message: "Failed document service error occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedDocumentServiceException);
            }
        }

        private async ValueTask<Document> TryCatch(ReturningDocumentFunction returningDocumentFunction)
        {
            try
            {
                return await returningDocumentFunction();
            }
            catch (InvalidDocumentException invalidDocumentException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidDocumentException);
            }
            catch (NotFoundDocumentException notFoundDocumentException)
            {
                throw await CreateAndLogValidationExceptionAsync(notFoundDocumentException);
            }
            catch (RequestFailedException requestFailedException)
            {
                var failedRequestException = new FailedDocumentRequestException(
                    message: "Failed document request occurred, please contact support.",
                    innerException: requestFailedException);

                throw await CreateAndLogDependencyExceptionAsync(failedRequestException);
            }
            catch (Exception exception)
            {
                var failedDocumentBlobServiceException =
                    new FailedDocumentServiceException(
                        message: "Failed document service error occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedDocumentBlobServiceException);
            }
        }

        private async ValueTask<string> TryCatch(ReturningStringFunction returningStringFunction)
        {
            try
            {
                return await returningStringFunction();
            }
            catch (InvalidDocumentException invalidDocumentException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidDocumentException);
            }
            catch (RequestFailedException requestFailedException)
            {
                var failedRequestException = new FailedDocumentRequestException(
                    message: "Failed document request occurred, please contact support.",
                    innerException: requestFailedException);

                throw await CreateAndLogDependencyExceptionAsync(failedRequestException);
            }
            catch (Exception exception)
            {
                var failedDocumentBlobServiceException =
                    new FailedDocumentServiceException(
                        message: "Failed document service error occurred, please contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedDocumentBlobServiceException);
            }
        }

        private async ValueTask<DocumentValidationException> CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var documentValidationException =
                new DocumentValidationException(
                    message: "Document validation errors occured, please try again",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentValidationException);

            return documentValidationException;
        }

        private async ValueTask<DocumentDependencyException> CreateAndLogDependencyExceptionAsync(Xeption exception)
        {
            var documentDependencyException = new DocumentDependencyException(
                message: "Document dependency error occurred, please contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentDependencyException);

            return documentDependencyException;
        }

        private async ValueTask<DocumentDependencyValidationException> CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var documentDependencyValidationException =
                new DocumentDependencyValidationException(
                    message: "Document dependency validation occurred, please try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentDependencyValidationException);

            return documentDependencyValidationException;
        }

        private async ValueTask<DocumentServiceException> CreateAndLogServiceExceptionAsync(Xeption exception)
        {
            var documentServiceException = new DocumentServiceException(
                message: "Document service error occurred, please contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentServiceException);

            return documentServiceException;
        }
    }
}
