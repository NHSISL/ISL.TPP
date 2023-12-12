// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Foundations.Files.Exceptions
{
    internal class FileValidationException : Xeption
    {
        public FileValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
