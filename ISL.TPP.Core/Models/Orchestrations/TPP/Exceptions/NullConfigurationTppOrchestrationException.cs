// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions
{
    public class NullConfigurationTppOrchestrationException : Xeption
    {
        public NullConfigurationTppOrchestrationException(string message)
            : base(message)
        { }
    }
}