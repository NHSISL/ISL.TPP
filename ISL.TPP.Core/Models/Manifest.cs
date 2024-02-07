// ---------------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------------

namespace ISL.TPP.Core.Models
{
    internal class Manifest
    {
        public string FileName { get; set; }
        public string IsDelta { get; set; }
        public string IsReference { get; set; }
        public string DateExtractFrom { get; set; }
        public string DateExtractTo { get; set; }
    }
}
