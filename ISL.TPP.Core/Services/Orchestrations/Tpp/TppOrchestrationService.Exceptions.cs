// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
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
    }
}
