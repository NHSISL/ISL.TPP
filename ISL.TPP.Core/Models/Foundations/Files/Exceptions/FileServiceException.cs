// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Foundations.Files.Exceptions
{
    internal class FileServiceException : Xeption
    {
        public FileServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
