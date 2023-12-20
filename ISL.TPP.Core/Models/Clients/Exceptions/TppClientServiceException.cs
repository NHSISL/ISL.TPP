// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Clients.Exceptions
{
    public class TppClientServiceException : Xeption
    {
        public TppClientServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
