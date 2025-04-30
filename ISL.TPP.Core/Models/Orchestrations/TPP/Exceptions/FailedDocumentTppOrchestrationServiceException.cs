// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions
{
    public class FailedDocumentTppOrchestrationServiceException : Xeption
    {
        public FailedDocumentTppOrchestrationServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}