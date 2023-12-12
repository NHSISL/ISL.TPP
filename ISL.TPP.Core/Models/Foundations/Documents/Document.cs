// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

namespace ISL.TPP.Core.Models.Foundations.Documents
{
    public class Document
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] DocumentData { get; set; } = new byte[0];
        public string SHA256Hash { get; set; } = string.Empty;
    }
}
