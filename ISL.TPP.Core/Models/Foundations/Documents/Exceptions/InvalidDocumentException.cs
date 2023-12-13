// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Foundations.Documents.Exceptions
{
    public class InvalidDocumentException : Xeption
    {
        public InvalidDocumentException(string message)
            : base(message)
        { }
    }
}
