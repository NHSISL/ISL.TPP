// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions
{
    internal class TppOrchestrationDependencyException : Xeption
    {
        public TppOrchestrationDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
