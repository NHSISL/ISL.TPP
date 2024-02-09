// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Brokers.Files
{
    internal interface IFileBroker
    {
        ValueTask<bool> CheckIfFileExistsAsync(string path);
        ValueTask<bool> WriteToFileAsync(string path, string content);
        ValueTask<byte[]> ReadFileAsync(string path);
        ValueTask<bool> DeleteFileAsync(string path);
        ValueTask<bool> MoveFileAsync(string sourcePath, string destinationPath);
        ValueTask<List<string>> GetListOfFilesAsync(string path, string searchPattern = "*");
        ValueTask<bool> CheckIfDirectoryExistsAsync(string path);
        ValueTask<bool> CreateDirectoryAsync(string path);
        ValueTask<bool> DeleteDirectoryAsync(string path, bool recursive = false);
        ValueTask<string> GetDirectoryAsync(string path);
    }
}
