// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISL.TPP.Core.Models.Foundations.SubscriberAgreements.Exceptions;
using Xeptions;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal partial class SubscriberAgreementService
    {
        private delegate ValueTask<List<string>> ReturningStringListFunction();

        private async ValueTask<List<string>> TryCatch(ReturningStringListFunction returningStringListFunction)
        {
            try
            {
                return await returningStringListFunction();
            }
            catch (Exception exception)
            {
                var failedSubscriberAgreementServiceException =
                    new FailedSubscriberAgreementServiceException(
                        message: "Failed subscriber agreement service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedSubscriberAgreementServiceException);
            }
        }

        private async ValueTask<SubscriberAgreementServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var subscriberAgreementServiceException = new SubscriberAgreementServiceException(
                message: "Subscriber agreement service error occurred, contact support.",
                innerException: exception);

            await this.loggingBroker.LogErrorAsync(subscriberAgreementServiceException);

            return subscriberAgreementServiceException;
        }
    }
}
