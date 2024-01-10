// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Foundations.Documents.Exceptions
{
    internal class DocumentDependencyValidationException : Xeption
    {
        public DocumentDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
