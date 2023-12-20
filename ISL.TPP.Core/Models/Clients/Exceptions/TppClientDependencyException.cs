// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using Xeptions;

namespace ISL.TPP.Core.Models.Clients.Exceptions
{
    public class TppClientDependencyException : Xeption
    {
        public TppClientDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
