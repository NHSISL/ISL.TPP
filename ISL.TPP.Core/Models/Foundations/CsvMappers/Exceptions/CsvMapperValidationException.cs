// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Foundations.CsvMappers.Exceptions
{
    public class CsvMapperValidationException : Xeption
    {
        public CsvMapperValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
