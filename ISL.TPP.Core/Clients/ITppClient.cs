// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using ISL.TPP.Core.Clients.Imports;

namespace ISL.TPP.Core.Clients
{
    public interface ITppClient
    {
        IImportClient Imports { get; }
    }
}
