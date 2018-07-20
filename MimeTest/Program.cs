using System;
using System.Collections.Generic;
using System.IO;
using MimeMagicSharp;

namespace MimeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string originalMagicFile = Path.Combine(Environment.CurrentDirectory, "magic_original"),
                jsonMagicFile = Path.Combine(Environment.CurrentDirectory, "magic_json");

            MimeMagicSharp.MimeMagicSharp.ConvertFromOriginalToJson("magic_original", "magic_json_convert_test");

            //  Read and detect
            using (MimeMagicSharp.MimeMagicSharp ms = 
                new MimeMagicSharp.MimeMagicSharp(EMagicFileType.Json, jsonMagicFile))
            {
                try
                {
                    foreach (MimeTypeGuess mimeTypeGuess in ms.AssumeMimeType(EMimeTypeBy.Content,
                        Path.Combine(Environment.CurrentDirectory, "Newtonsoft.Json.xml")))
                    {
                        //  Iterate over results
                    }
                }
                catch (Exception ex)
                {
                    //  Hande errors here
                }
            }
        }
    }
}
