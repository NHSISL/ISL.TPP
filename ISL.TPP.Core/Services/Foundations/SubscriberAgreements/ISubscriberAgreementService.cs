// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Services.Foundations.Documents
{
    internal interface ISubscriberAgreementService
    {
        ValueTask<List<string>> GetActiveSubscriberAgreementsAsync();
    }
}
