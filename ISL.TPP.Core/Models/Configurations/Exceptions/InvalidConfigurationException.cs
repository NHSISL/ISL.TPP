// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Collections;
using Xeptions;

namespace ISL.TPP.Core.Models.Configurations.Exceptions
{
    public class InvalidConfigurationException : Xeption
    {
        public InvalidConfigurationException(
            string message = "",
            Exception innerException = null,
            IDictionary data = null)
            : base(
                  message: $"Invalid configuration. Please correct the errors and try again.  {message}",
                  innerException,
                  data)
        { }
    }
}