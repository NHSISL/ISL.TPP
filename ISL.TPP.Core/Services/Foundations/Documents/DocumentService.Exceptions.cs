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
                throw CreateAndLogValidationException(nullDocumentException);
            }
            catch (InvalidDocumentException invalidDocumentException)
            {
                throw CreateAndLogValidationException(invalidDocumentException);
            }
            catch (RequestFailedException requestFailedException)
            {
                var failedRequestException = new FailedDocumentRequestException(
                    message: "Failed document request occurred, please contact support",
                    innerException: requestFailedException);

                throw CreateAndLogDependencyException(failedRequestException);
            }
            catch (Exception exception)
            {
                var failedDocumentServiceException =
                    new FailedDocumentServiceException(
                        message: "Failed document service error occurred, contact support.",
                        innerException: exception);

                throw CreateAndLogServiceException(failedDocumentServiceException);
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
                throw CreateAndLogValidationException(invalidDocumentException);
            }
            catch (NotFoundDocumentException notFoundDocumentException)
            {
                throw CreateAndLogValidationException(notFoundDocumentException);
            }
            catch (RequestFailedException requestFailedException)
            {
                var failedRequestException = new FailedDocumentRequestException(
                    message: "Failed document request occurred, please contact support",
                    innerException: requestFailedException);

                throw CreateAndLogDependencyException(failedRequestException);
            }
            catch (Exception exception)
            {
                var failedDocumentBlobServiceException =
                    new FailedDocumentServiceException(
                        message: "Failed document service error occurred, contact support.",
                        innerException: exception);

                throw CreateAndLogServiceException(failedDocumentBlobServiceException);
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
                throw CreateAndLogValidationException(invalidDocumentException);
            }
            catch (RequestFailedException requestFailedException)
            {
                var failedRequestException = new FailedDocumentRequestException(
                    message: "Failed document request occurred, please contact support",
                    innerException: requestFailedException);

                throw CreateAndLogDependencyException(failedRequestException);
            }
            catch (Exception exception)
            {
                var failedDocumentBlobServiceException =
                    new FailedDocumentServiceException(
                        message: "Failed document service error occurred, contact support.",
                        innerException: exception);

                throw CreateAndLogServiceException(failedDocumentBlobServiceException);
            }
        }

        private DocumentValidationException CreateAndLogValidationException(Xeption exception)
        {
            var documentValidationExceptionn =
                new DocumentValidationException(
                    message: "Document validation errors occured, please try again",
                    innerException: exception);

            this.loggingBroker.LogError(documentValidationExceptionn);

            return documentValidationExceptionn;
        }

        private DocumentDependencyException CreateAndLogDependencyException(Xeption exception)
        {
            var documentDependencyException = new DocumentDependencyException(
                message: "Document dependency error occurred, contact support.",
                innerException: exception);

            this.loggingBroker.LogError(documentDependencyException);

            return documentDependencyException;
        }

        private DocumentDependencyValidationException CreateAndLogDependencyValidationException(Xeption exception)
        {
            var documentDependencyValidationException =
                new DocumentDependencyValidationException(
                    message: "Document dependency validation occurred, please try again.",
                    innerException: exception);

            this.loggingBroker.LogError(documentDependencyValidationException);

            return documentDependencyValidationException;
        }

        private DocumentServiceException CreateAndLogServiceException(Xeption exception)
        {
            var documentServiceException = new DocumentServiceException(
                message: "Document service error occurred, contact support.",
                innerException: exception);

            this.loggingBroker.LogError(documentServiceException);

            return documentServiceException;
        }
    }
}
