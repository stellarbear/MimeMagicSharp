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

            //  Convert from original format to json
            MimeSharp.CMimeSharp.ConvertFromOriginalToJson("magic_original", "magic_json_convert_test", out string ConvertError);

            //  Read and detect
            using (MimeSharp.CMimeSharp MS = new MimeSharp.CMimeSharp(JsonMagicFile, MimeSharp.EMagicFileType.Json, out string ErrorMessage))
            {
                //  If no errors occured during reading
                if (ErrorMessage == null)
                {
                    try
                    {
                        List<MimeSharp.CType> Results = MS.ByContent(Path.Combine(Environment.CurrentDirectory, "MimeSharp.dll"));
                    }
                    catch (IOException) { }
                }
                else
                { }
            }
        }
    }
}
