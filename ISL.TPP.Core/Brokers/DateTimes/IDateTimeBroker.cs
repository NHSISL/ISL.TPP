// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Brokers.DateTimes
{
    internal interface IDateTimeBroker
    {
        ValueTask<DateTimeOffset> GetCurrentDateTimeOffsetAsync();
    }
}
