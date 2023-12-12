// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Services.Foundations.Files
{
    internal interface IFileService
    {
        ValueTask<bool> CheckIfFileExistsAsync(string path);
        ValueTask<bool> WriteToFileAsync(string path, string content);
        ValueTask<byte[]> ReadFromFileAsync(string path);
        ValueTask<bool> DeleteFileAsync(string path);
        ValueTask<List<string>> RetrieveListOfFilesAsync(string path, string searchPattern = "*");
        ValueTask<bool> CheckIfDirectoryExistsAsync(string path);
        ValueTask<bool> CreateDirectoryAsync(string path);
        ValueTask<bool> DeleteDirectoryAsync(string path, bool recursive = false);
        ValueTask<string> ComputeSHA256Hash(string filePath);
    }
}
