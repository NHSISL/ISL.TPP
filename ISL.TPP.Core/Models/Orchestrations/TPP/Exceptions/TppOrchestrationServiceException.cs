// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using Xeptions;

namespace ISL.TPP.Core.Models.Orchestrations.TPP.Exceptions
{
    internal class TppOrchestrationServiceException : Xeption
    {
        public TppOrchestrationServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}