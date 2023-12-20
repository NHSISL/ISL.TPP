// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using Xeptions;

namespace ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions
{
    public class FailedTppOrchestrationServiceException : Xeption
    {
        public FailedTppOrchestrationServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}