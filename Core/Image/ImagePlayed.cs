using System;

namespace FamilyBoard.Core.Image
{
    public class ImagePlayed
    {
        public bool Exists { get; set; }
        public int Counter { get; set; }
        public DateTime LastPlayed { get; set; }
    }
}
