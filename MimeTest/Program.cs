using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MimeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string OriginalMagicFile = Path.Combine(Environment.CurrentDirectory, "magic_original"),
                JsonMagicFile = Path.Combine(Environment.CurrentDirectory, "magic_json");

            //  Get version
            MimeMagicSharp.CMimeMagicSharp.GetVersion();

            //  Convert from original format to json
            MimeMagicSharp.CMimeMagicSharp.ConvertFromOriginalToJson("magic_original", "magic_json_convert_test", out string ConvertError);

            //  Read and detect
            using (MimeMagicSharp.CMimeMagicSharp MS = new MimeMagicSharp.CMimeMagicSharp(JsonMagicFile, MimeMagicSharp.EMagicFileType.Json, out string ErrorMessage))
            {
                //  If no errors occured during reading
                if (ErrorMessage == null)
                {
                    try
                    {
                        List<MimeMagicSharp.CType> Results = MS.ByContent(Path.Combine(Environment.CurrentDirectory, "MimeMagicSharp.dll"));
                    }
                    catch (IOException) { }
                }
                else
                { }
            }
        }
    }
}
