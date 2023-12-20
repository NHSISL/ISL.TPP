// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections;
using Xeptions;

namespace ISL.TPP.Core.Models.Clients.Exceptions
{
    public class TppClientValidationException : Xeption
    {
        public TppClientValidationException(string message)
            : base(message)
        { }

        public TppClientValidationException(string message, Xeption innerException, IDictionary data)
            : base(message, innerException, data)
        { }
    }
}
