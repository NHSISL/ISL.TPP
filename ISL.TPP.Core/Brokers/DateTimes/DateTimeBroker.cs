// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;

namespace ISL.TPP.Core.Brokers.DateTimes
{
    internal class DateTimeBroker : IDateTimeBroker
    {
        public DateTimeOffset GetCurrentDateTimeOffset() =>
            DateTimeOffset.UtcNow;
    }
}
