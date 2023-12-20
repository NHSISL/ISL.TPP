// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions
{
    public class TppOrchestrationValidationException : Xeption
    {
        public TppOrchestrationValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
