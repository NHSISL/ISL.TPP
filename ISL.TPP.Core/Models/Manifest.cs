// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

namespace ISL.TPP.Core.Models
{
    internal class Manifest
    {
        public string FileName { get; set; } = string.Empty;
        public string IsDelta { get; set; } = string.Empty;
        public string IsReference { get; set; } = string.Empty;
        public string DateExtractFrom { get; set; } = string.Empty;
        public string DateExtractTo { get; set; } = string.Empty;
    }
}
