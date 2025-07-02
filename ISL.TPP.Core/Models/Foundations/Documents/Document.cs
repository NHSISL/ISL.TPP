// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

using System.IO;

namespace ISL.TPP.Core.Models.Foundations.Documents
{
    public class Document
    {
        public string FileName { get; set; } = string.Empty;
        public Stream? DocumentData { get; set; }
        public string SHA256Hash { get; set; } = string.Empty;
    }
}
