using System;
using System.IO;
using System.Linq;

namespace ScanMusic.Core
{
    public static class FileFilter
    {
        private static readonly string[] AllowedExtensions = { ".mp3", ".wav" };

        public static bool IsValidAudioFile(string filePath)
        {
            if (!File.Exists(filePath)) return false;

            var ext = Path.GetExtension(filePath).ToLower();
            return AllowedExtensions.Contains(ext);
        }
    }
}
