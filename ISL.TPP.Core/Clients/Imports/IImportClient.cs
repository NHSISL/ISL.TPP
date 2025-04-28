// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Threading.Tasks;

namespace ISL.TPP.Core.Clients.Imports
{
    public interface IImportClient
    {
        ValueTask ProcessFilesAsync();
    }
}
