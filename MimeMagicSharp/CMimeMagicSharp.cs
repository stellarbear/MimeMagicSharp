using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MimeMagicSharp
{
    //  New (json) or old (original) mime database format
    public enum EMagicFileType { Json, Original }

    //  Interface to MimeMagicSharp library
    public class CMimeMagicSharp : IDisposable
    {
        //  Default mime type
        public static string Unknown = "[application/unknown]";

        //  Get dll version
        public static Version GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        //  Mime database reader
        private CReader MimeReader;

        //  Constructor section
        public CMimeMagicSharp(string MagicFilePath, EMagicFileType MagicFileType, out string ErrorMessage)
        {
            MimeReader = new CReader(MagicFilePath, MagicFileType, out ErrorMessage);
        }
        //  Destructor section
        public void Dispose()
        {
            MimeReader.Dispose();
        }

        //  Detect mime type base on file content
        public List<CType> ByContent(string Filename, bool Single = false)
        {
            return MimeReader.GetMimeTypeByContent(Filename, Single);
        }
        //  Detect mime type base on file extension (new format only)
        public List<CType> ByExtension(string Filename, bool Single = false)
        {
            return MimeReader.GetMimeTypeByExtension(Filename, Single);
        }

        //  Convert from old (original) format to new (json)
        public static void ConvertFromOriginalToJson(string FilenameFrom, string FilenameTo, out string ErrorMessage)
        {
            CReader MimeReader = new CReader(FilenameFrom, EMagicFileType.Original, out ErrorMessage);
            MimeReader.SaveLocal(FilenameTo);
        }
    }
}
