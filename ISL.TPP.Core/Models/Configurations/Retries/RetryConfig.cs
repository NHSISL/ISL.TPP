// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;

namespace ISL.TPP.Core.Models.Configurations.Retries
{
    public class RetryConfig : IRetryConfig
    {
        public RetryConfig()
        { }

        public RetryConfig(int maxRetryAttempts, int pauseBetweenFailuresInMilliseconds)
        {
            MaxRetryAttempts = maxRetryAttempts;
            PauseBetweenFailures = TimeSpan.FromMicroseconds(pauseBetweenFailuresInMilliseconds);
        }

        public RetryConfig(int maxRetryAttempts, TimeSpan pauseBetweenFailures)
        {
            MaxRetryAttempts = maxRetryAttempts;
            PauseBetweenFailures = pauseBetweenFailures;
        }

        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan PauseBetweenFailures { get; set; } = TimeSpan.FromSeconds(1);
    }
}
