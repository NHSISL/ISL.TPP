// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Foundations.CsvMappers.Exceptions
{
    public class InvalidCsvMapperArgumentsException : Xeption
    {
        public InvalidCsvMapperArgumentsException(string message)
            : base(message)
        { }
    }
}
