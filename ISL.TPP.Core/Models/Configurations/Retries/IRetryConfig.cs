// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;

namespace ISL.TPP.Core.Models.Configurations.Retries
{
    internal interface IRetryConfig
    {
        int MaxRetryAttempts { get; }
        TimeSpan PauseBetweenFailures { get; }
    }
}
