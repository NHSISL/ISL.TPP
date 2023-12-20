// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Services.Orchestrations.Tpp
{
    internal interface ITppOrchestrationService
    {
        ValueTask<List<string>> ProcessFilesAsync();
    }
}
