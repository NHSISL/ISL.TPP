// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using Xeptions;

namespace ISL.TPP.Core.Models.Foundations.Documents.Exceptions
{
    public class FailedDocumentStorageException : Xeption
    {
        public FailedDocumentStorageException(string message, Exception innerException)
           : base(message, innerException)
        { }
    }
}
