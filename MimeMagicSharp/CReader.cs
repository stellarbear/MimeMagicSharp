using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MimeMagicSharp
{
    //  "Read mime data file" class
    class CReader : IDisposable
    {
        //  List of mime types
        [JsonProperty("MimeTypes")] List<CType> Types;

        //  Constructor section
        public CReader(string MagicFilename, EMagicFileType FileType, out string ErrorMessage)
        {
            ErrorMessage = null;
            if (File.Exists(MagicFilename))
            {
                switch (FileType)
                {
                    case EMagicFileType.Json:
                        Types = ReadJson(MagicFilename, out ErrorMessage);
                        break;
                    case EMagicFileType.Original:
                        Types = ReadOriginal(MagicFilename, out ErrorMessage);
                        break;
                }
            }
            else
            {
                ErrorMessage = "File does not exist";
            }
        }
        //  Destructor section
        public void Dispose()
        {
            Types.Clear();
            Types = null;
        }

        //  Read new format
        private List<CType> ReadJson(string Filename, out string ErrorMessage)
        {
            ErrorMessage = null;
            List<CType> ResultMimeSet = new List<CType>();

            try { ResultMimeSet = JsonConvert.DeserializeObject<List<CType>>(ReadFile(Filename)); }
            catch (Exception Ex) { ErrorMessage = Ex.Message; return null; }

            return ResultMimeSet;
        }
        //  Read old format
        private List<CType> ReadOriginal(string Filename, out string ErrorMessage)
        {
            int MagicSize = Convert.ToInt32((new FileInfo(Filename)).Length);
            byte[] FileHeader = ReadNBytes(Filename, MagicSize);
            ErrorMessage = null;

            List<CType> ResultMimeSet = new List<CType>();
            CType MimeType = new CType();

            //  https://developer.gnome.org/shared-mime-info-spec/

            //  [ indent ] ">" start-offset "=" value [ "&" mask] [ "~" word-size] [ "+" range-length] "\n"
            /*
                indent	            1                   The nesting depth of the rule, corresponding to the number of '>' characters in the traditional file format.
                ">" start-offset    >4                  The offset into the file to look for a match.
                "=" value           =\0x0\0x2\0x55\0x40 Two bytes giving the (big-endian) length of the value, followed by the value itself.
                "&" mask            &\0xff\0xf0         The mask, which (if present) is exactly the same length as the value.
                "~" word-size	    ~2	                On little-endian machines, the size of each group to byte-swap.
                "+" range-length	+8                  The length of the region in the file to check.
             */

            int Indent, StartOffset, WordSize, RangeLength, ValueLength;
            byte[] Value, Mask;
            string Name = "";

            //  Skeep header "MIME-Magic.. (4D 49 4D 45 2D 4D 61 67 69 63 00 0A)"
            int Pointer = 12;
            while (Pointer < MagicSize)
            {
                Indent = StartOffset = WordSize = RangeLength = ValueLength = 0;
                Value = Mask = null;

                //  Pointer is always set to the beginning of Mime signature
                //  InnerPointer goes forward on signature body only
                int InnerPointer = Pointer;

                if (FileHeader[InnerPointer] == 0x0A /*newline*/)
                    InnerPointer++;

                //  Signature head
                if (FileHeader[InnerPointer] == 0x5B /*[*/)
                {
                    Name = Encoding.UTF8.GetString(GetUntilByteMetFromByteArray(FileHeader, ref InnerPointer, 0x0A /*newline*/));
                    MimeType = new CType(Name);
                    ResultMimeSet.Add(MimeType);
                }

                //  Indent may be absent. In this case ">" is expected
                if (FileHeader[InnerPointer] != 0x3E /*>*/)
                    Indent = GetNumberFromByteArrayAtOffset(FileHeader, ref InnerPointer);
                if (FileHeader[InnerPointer] == 0x3E /*>*/)
                    InnerPointer++;
                else
                {
                    ErrorMessage = "Error at " + (InnerPointer) + ": Expected: >(0x3E)";
                    return null;
                }

                //  Before StartOffset is always expected "="
                StartOffset = GetNumberFromByteArrayAtOffset(FileHeader, ref InnerPointer);
                if (FileHeader[InnerPointer] == 0x3D /*=*/)
                    InnerPointer++;
                else
                {
                    ErrorMessage = "Error at " + (InnerPointer) + ": Expected: =(0x3D)";
                    return null;
                }

                //  Get body length
                ValueLength = GetByteArrayByLength(FileHeader, ref InnerPointer, 2)[1];
                Value = GetByteArrayByLength(FileHeader, ref InnerPointer, ValueLength);

                //  We deal with mask or word-size or range-length or end of body depending on a separator
                switch (FileHeader[InnerPointer])
                {
                    case (0x0A /*newline*/):
                        InnerPointer++;
                        break;
                    case (0x26 /*&*/):
                        InnerPointer++;
                        Mask = GetByteArrayByLength(FileHeader, ref InnerPointer, ValueLength);
                        break;
                    case (0x7E /*~*/):
                        InnerPointer++;
                        WordSize = GetNumberFromByteArrayAtOffset(FileHeader, ref InnerPointer);
                        break;
                    case (0x2B /*+*/):
                        InnerPointer++;
                        RangeLength = GetNumberFromByteArrayAtOffset(FileHeader, ref InnerPointer);
                        break;
                    default:
                        ErrorMessage = "Error at " + (InnerPointer) + ": Expected: \n(0x0A), &(0x26), ~(0x7E), +(0x2B)";
                        return null;
                }

                //  If Indent was absent, new rule set, containing rule, is created, elsecase rule is appended to the last rule set
                if (Indent == 0)
                    MimeType.AddNewRuleSet(new CRule(Indent, StartOffset, Value, Mask, WordSize, RangeLength));
                else
                    MimeType.AppendLastRuleSet(new CRule(Indent, StartOffset, Value, Mask, WordSize, RangeLength));

                //  Pointer jumps to the end of signature's body
                Pointer = InnerPointer;
            }
            return ResultMimeSet;
        }
        #region ByteArrayWalkThrough
        private int GetNumberFromByteArrayAtOffset(byte[] Data, ref int Offset)
        {
            int Result = 0, Temp, Iterator = 1;
            byte[] CutData = Data.Skip(Offset).ToArray();

            while (int.TryParse(Encoding.UTF8.GetString(CutData.Take(Iterator++).ToArray()), out Temp))
                Result = Temp;

            Offset += Iterator - 2;
            return Result;
        }
        private byte[] GetUntilByteMetFromByteArray(byte[] Data, ref int Offset, byte To)
        {
            int Pointer = 0;
            byte[] CutData = Data.Skip(Offset).ToArray();
            while (CutData[Pointer++] != To) { }

            Offset += Pointer--;
            return CutData.Take(Pointer).ToArray();
        }
        private byte[] GetByteArrayByLength(byte[] Data, ref int Offset, int Length)
        {
            int DataOffset = Offset;
            Offset += Length;

            return Data.Skip(DataOffset).Take(Length).ToArray();
        }
        #endregion

        //  Read first N byte
        private byte[] ReadNBytes(string Filename, int N)
        {
            byte[] Buffer = new byte[N];

            //  Read if exists
            if (File.Exists(Filename))
            {
                using (System.IO.FileStream FS = File.Open(Filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                { FS.Read(Buffer, 0, Buffer.Length); }
            }

            return Buffer;
        }
        //  Read entire file
        private string ReadFile(string Filename)
        {
            string Result = null;

            //  Read if exists
            if (File.Exists(Filename))
            {
                using (System.IO.StreamReader SR = new System.IO.StreamReader(Filename, Encoding.UTF8))
                {
                    Result = SR.ReadToEnd();
                }
            }

            return Result;
        }

        //  Detect mime type base on file content
        public List<CType> GetMimeTypeByContent(string Filename, bool Single = false)
        {
            byte[] FileHeader = ReadNBytes(Filename, 4096);
            List<CType> Result = new List<CType>();

            //  Iteratively check Mime type
            foreach (CType Type in Types)
            {
                if (Type.CheckType(FileHeader))
                {
                    Result.Add(Type);
                    if (Single) return Result;
                }
            }

            return (Result.Count > 0) ? Result : new List<CType>() { new CType(CMimeMagicSharp.Unknown) };
        }
        //  Detect mime type base on extension (new format only)
        public List<CType> GetMimeTypeByExtension(string Filename, bool Single = false)
        {
            string Ext = Path.GetExtension(Filename).ToLower().Replace(".", "");
            List<CType> Result = new List<CType>();

            //  Iteratively check Mime type
            foreach (CType Type in Types)
            {
                if (Type.Extensions != null)
                {
                    if (Type.Extensions.Contains(Ext))
                    {
                        Result.Add(Type);
                        if (Single) return Result;
                    }
                }
            }

            return (Result.Count > 0) ? Result : new List<CType>() { new CType(CMimeMagicSharp.Unknown) };
        }

        //  Save class to disk (conversion)
        public void SaveLocal(string FilenameTo)
        {
            File.WriteAllText(FilenameTo, JsonConvert.SerializeObject(Types, Formatting.Indented));
        }
    }
}
