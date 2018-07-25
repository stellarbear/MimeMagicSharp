using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MimeMagicSharp
{
    class Reader : IDisposable
    {
        [JsonProperty("MimeTypes")]
        private IEnumerable<MimeTypeGuess> _types;

        public Reader(string magicFilename, EMagicFileType fileType)
        {
            if (File.Exists(magicFilename))
            {
                switch (fileType)
                {
                    case EMagicFileType.Json:
                        _types = ReadJson(magicFilename);
                        break;
                    case EMagicFileType.Original:
                        _types = ReadOriginal(magicFilename);
                        break;
                }
            }
            else
            {
                throw new FileNotFoundException($"File does not exist: {magicFilename}");
            }
        }
        public void Dispose()
        {
            _types = null;
        }


        private List<MimeTypeGuess> ReadJson(string filename)
        {
            List<MimeTypeGuess> resultMimeSet;

            try
            {
                resultMimeSet = JsonConvert.DeserializeObject<List<MimeTypeGuess>>(ReadFile(filename));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during deserialization: {ex.Message}");
            }

            return resultMimeSet;
        }
        //  Read old format
        private List<MimeTypeGuess> ReadOriginal(string filename)
        {
            int magicSize = Convert.ToInt32(new FileInfo(filename).Length);
            byte[] fileHeader = ReadNBytes(filename, magicSize);

            List<MimeTypeGuess> resultMimeSet = new List<MimeTypeGuess>();
            MimeTypeGuess mimeType = new MimeTypeGuess();

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

            int indent, startOffset, wordSize, rangeLength, valueLength;
            byte[] value, mask;
            string name = "";

            //  Skeep header "MIME-Magic.. (4D 49 4D 45 2D 4D 61 67 69 63 00 0A)"
            int pointer = 12;
            while (pointer < magicSize)
            {
                indent = startOffset = wordSize = rangeLength = valueLength = 0;
                value = mask = null;

                //  Pointer is always set to the beginning of Mime signature
                //  InnerPointer goes forward on signature body only
                int innerPointer = pointer;

                if (fileHeader[innerPointer] == 0x0A /*newline*/)
                    innerPointer++;

                //  Signature head
                if (fileHeader[innerPointer] == 0x5B /*[*/)
                {
                    name = Encoding.UTF8.GetString(GetUntilByteMetFromByteArray(fileHeader, ref innerPointer, 0x0A /*newline*/));
                    mimeType = new MimeTypeGuess(name);
                    resultMimeSet.Add(mimeType);
                }

                //  Indent may be absent. In this case ">" is expected
                if (fileHeader[innerPointer] != 0x3E /*>*/)
                    indent = GetNumberFromByteArrayAtOffset(fileHeader, ref innerPointer);
                if (fileHeader[innerPointer] == 0x3E /*>*/)
                    innerPointer++;
                else
                    throw new Exception($"Error during parsing at: {innerPointer}. Expected: >(0x3E)");

                //  Before StartOffset is always expected "="
                startOffset = GetNumberFromByteArrayAtOffset(fileHeader, ref innerPointer);
                if (fileHeader[innerPointer] == 0x3D /*=*/)
                    innerPointer++;
                else
                    throw new Exception($"Error during parsing at: {innerPointer}. Expected: =(0x3D)");

                //  Get body length
                valueLength = GetByteArrayByLength(fileHeader, ref innerPointer, 2)[1];
                value = GetByteArrayByLength(fileHeader, ref innerPointer, valueLength);

                //  We deal with mask or word-size or range-length or end of body depending on a separator
                switch (fileHeader[innerPointer])
                {
                    case (0x0A /*newline*/):
                        innerPointer++;
                        break;
                    case (0x26 /*&*/):
                        innerPointer++;
                        mask = GetByteArrayByLength(fileHeader, ref innerPointer, valueLength);
                        break;
                    case (0x7E /*~*/):
                        innerPointer++;
                        wordSize = GetNumberFromByteArrayAtOffset(fileHeader, ref innerPointer);
                        break;
                    case (0x2B /*+*/):
                        innerPointer++;
                        rangeLength = GetNumberFromByteArrayAtOffset(fileHeader, ref innerPointer);
                        break;
                    default:
                        throw new Exception($"Error during parsing at: {innerPointer}. Expected (one from): \n(0x0A), &(0x26), ~(0x7E), +(0x2B)");
                }

                //  If Indent was absent, new rule set, containing rule, is created, elsecase rule is appended to the last rule set
                if (indent == 0)
                    mimeType.AddNewRuleSet(new Rule(indent, startOffset, value, mask, wordSize, rangeLength));
                else
                    mimeType.AppendLastRuleSet(new Rule(indent, startOffset, value, mask, wordSize, rangeLength));

                //  Pointer jumps to the end of signature's body
                pointer = innerPointer;
            }
            return resultMimeSet;
        }
        #region ByteArrayWalkThrough
        private int GetNumberFromByteArrayAtOffset(byte[] data, ref int offset)
        {
            int result = 0, iterator = 1;
            byte[] cutData = data.Skip(offset).ToArray();

            while (int.TryParse(Encoding.UTF8.GetString(cutData.Take(iterator++).ToArray()), out var temp))
                result = temp;

            offset += iterator - 2;
            return result;
        }
        private byte[] GetUntilByteMetFromByteArray(byte[] data, ref int offset, byte to)
        {
            int pointer = 0;
            byte[] cutData = data.Skip(offset).ToArray();
            while (cutData[pointer++] != to) { }

            offset += pointer--;
            return cutData.Take(pointer).ToArray();
        }
        private byte[] GetByteArrayByLength(byte[] data, ref int offset, int length)
        {
            int dataOffset = offset;
            offset += length;

            return data.Skip(dataOffset).Take(length).ToArray();
        }
        #endregion

        //  Read first N byte
        private byte[] ReadNBytes(string filename, int n)
        {
            byte[] buffer = new byte[n];

            //  Read if exists
            if (File.Exists(filename))
            {
                using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read))
                { fs.Read(buffer, 0, buffer.Length); }
            }

            return buffer;
        }
        //  Read entire file
        private string ReadFile(string filename)
        {
            string result = null;

            //  Read if exists
            if (File.Exists(filename))
            {
                using (StreamReader sr = new StreamReader(filename, Encoding.UTF8))
                {
                    result = sr.ReadToEnd();
                }
            }

            return result;
        }

        //  Detect mime type base on file content
        public IEnumerable<MimeTypeGuess> GetMimeTypeByContent(string filename)
        {
            bool typeGuessed = false;
            byte[] fileHeader = ReadNBytes(filename, 4096);

            //  Iteratively check Mime type
            foreach (MimeTypeGuess type in _types)
            {
                if (type.CheckType(fileHeader))
                {
                    typeGuessed = true;
                    yield return type;
                }
            }

            if (!typeGuessed)
                yield return new MimeTypeGuess(MimeMagicSharp.UnknownMimeType);
        }
        //  Detect mime type base on extension (new format only)
        public IEnumerable<MimeTypeGuess> GetMimeTypeByExtension(string filename)
        {
            bool typeGuessed = false;
            string ext = Path.GetExtension(filename)?.ToLower().Replace(".", "");

            //  Iteratively check Mime type
            foreach (MimeTypeGuess type in _types)
            {
                if (type.Extensions != null)
                {
                    if (type.Extensions.Contains(ext))
                    {
                        typeGuessed = true;
                        yield return type;
                    }
                }
            }

            if (!typeGuessed)
                yield return new MimeTypeGuess(MimeMagicSharp.UnknownMimeType);
        }

        //  Save class to disk (conversion)
        public void SaveLocal(string filenameTo)
        {
            File.WriteAllText(filenameTo, JsonConvert.SerializeObject(_types, Formatting.Indented));
        }
    }
}
