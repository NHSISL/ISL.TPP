// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using Xeptions;

namespace ISL.TPP.Core.Models.Foundations.Files.Exceptions
{
    public class FailedFileDependencyException : Xeption
    {
        public FailedFileDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
