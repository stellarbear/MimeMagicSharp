using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MimeMagicSharp
{
    public enum EMagicFileType { Json, Original }
    public enum EMimeTypeBy { Extension, Content }

    public class MimeMagicSharp : IDisposable
    {
        private readonly Reader _mimeReader;
        public static string UnknownMimeType = "application/unknown";
        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public MimeMagicSharp(EMagicFileType magicFileType, string magicFilePath)
        {
            _mimeReader = new Reader(magicFilePath, magicFileType);
        }
        void IDisposable.Dispose()
        {
            _mimeReader?.Dispose();
        }

        public IEnumerable<MimeTypeGuess> AssumeMimeType(EMimeTypeBy detectionMethod, string filename)
        {
            switch (detectionMethod)
            {
                case EMimeTypeBy.Content:
                    return _mimeReader.GetMimeTypeByContent(filename);
                case EMimeTypeBy.Extension:
                    return _mimeReader.GetMimeTypeByExtension(filename);
                default:
                    throw new ArgumentException($"Ivalid property: {detectionMethod}");
            }
        }

        public static void ConvertFromOriginalToJson(string filenameFrom, string filenameTo)
        {
            if (File.Exists(filenameFrom))
            {
                Reader mimeReader = new Reader(filenameFrom, EMagicFileType.Original);
                mimeReader.SaveLocal(filenameTo);
            }
            else
            {
                throw new FileNotFoundException($"File does not exist: {filenameFrom}");
            }
        }
    }
}
