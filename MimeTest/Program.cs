using System;
using System.Collections.Generic;
using System.IO;

namespace MimeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string OriginalMagicFile = Path.Combine(Environment.CurrentDirectory, "magic_original"),
                JsonMagicFile = Path.Combine(Environment.CurrentDirectory, "magic_json");

            MimeMagicSharp.CMimeMagicSharp.GetVersion();
            MimeMagicSharp.CMimeMagicSharp.ConvertFromOriginalToJson("magic_original", "magic_json_convert_test");

            //  Read and detect
            using (MimeMagicSharp.CMimeMagicSharp MS = new MimeMagicSharp.CMimeMagicSharp(JsonMagicFile, MimeMagicSharp.EMagicFileType.Json))
            {
                try
                {
                    List<MimeMagicSharp.CType> Results = MS.ByContent(Path.Combine(Environment.CurrentDirectory, "Newtonsoft.Json.xml"));
                }
                catch (Exception Ex)
                {
                    //  Hande errors here
                }
            }
        }
    }
}
