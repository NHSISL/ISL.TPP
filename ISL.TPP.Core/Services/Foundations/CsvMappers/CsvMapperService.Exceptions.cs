// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Foundations.CsvMappers.Exceptions;
using Xeptions;

namespace ISL.TPP.Core.Services.Foundations.CsvMappers
{
    internal partial class CsvMapperService
    {
        private delegate ValueTask<string> ReturningStringFunction();
        private delegate ValueTask<List<T>> ReturningObjectFunction<T>();

        private async ValueTask<List<T>> TryCatch<T>(ReturningObjectFunction<T> returningObjectFunction)
        {
            try
            {
                return await returningObjectFunction();
            }
            catch (InvalidCsvMapperArgumentsException invalidCsvMapperArgumentsException)
            {
                throw CreateAndLogValidationException(invalidCsvMapperArgumentsException);
            }
            catch (Exception exception)
            {
                var failedCsvMapperServiceException =
                    new FailedCsvMapperServiceException(
                        message: "Failed CSV mapper service error occurred, contact support.",
                        innerException: exception);

                throw CreateAndLogServiceException(failedCsvMapperServiceException);
            }
        }

        private async ValueTask<string> TryCatch(ReturningStringFunction returningStringFunction)
        {
            try
            {
                return await returningStringFunction();
            }
            catch (InvalidCsvMapperArgumentsException invalidCsvMapperArgumentsException)
            {
                throw CreateAndLogValidationException(invalidCsvMapperArgumentsException);
            }
            catch (Exception exception)
            {
                var failedCsvMapperServiceException =
                    new FailedCsvMapperServiceException(
                        message: "Failed CSV mapper service error occurred, contact support.",
                        innerException: exception);

                throw CreateAndLogServiceException(failedCsvMapperServiceException);
            }
        }

        private CsvMapperValidationException CreateAndLogValidationException(Xeption exception)
        {
            var csvMapperValidationException = new CsvMapperValidationException(
                message: "CSV mapper validation errors occurred, fix the errors and try again.",
                innerException: exception);

            this.loggingBroker.LogError(csvMapperValidationException);

            return csvMapperValidationException;
        }

        private CsvMapperServiceException CreateAndLogServiceException(Xeption exception)
        {
            var csvMapperServiceException = new CsvMapperServiceException(
                message: "CSV mapper service error occurred, contact support.",
                innerException: exception);

            this.loggingBroker.LogError(csvMapperServiceException);

            return csvMapperServiceException;
        }
    }
}
