// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ISL.TPP.Core.Services.Foundations.Files
{
    internal interface IFileService
    {
        ValueTask<bool> CheckIfFileExistsAsync(string path);
        ValueTask<bool> WriteToFileAsync(string path, string content);
        ValueTask<byte[]> ReadFromFileAsync(string path);
        ValueTask ReadFromFileAsync(Stream output, string path);
        ValueTask<bool> DeleteFileAsync(string path);
        ValueTask<bool> CopyFileAsync(string sourcePath, string destinationPath);
        ValueTask<bool> MoveFileAsync(string sourcePath, string destinationPath);

        ValueTask<List<string>> RetrieveListOfFilesAsync(
            string path,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        ValueTask<List<string>> RetrieveListOfSubFoldersAsync(
            string path,
            string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly);

        ValueTask<bool> CheckIfDirectoryExistsAsync(string path);
        ValueTask<bool> CreateDirectoryAsync(string path);
        ValueTask<bool> DeleteDirectoryAsync(string path, bool recursive = false);
        ValueTask<string> GetDirectoryAsync(string path);
        ValueTask<bool> MoveDirectoryAsync(string sourceDirectory, string destinationDirectory);
        ValueTask<string> ComputeSHA256Hash(string filePath);
        ValueTask<string> GetTempFileNameAsync();
    }
}
