using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace MimeMagicSharp
{
    //  Rule representation
    public class Rule
    {
        [JsonProperty("Range")] int _range;
        [JsonProperty("Offset")] int _offset;
        [JsonProperty("Data")] string _data;
        [JsonProperty("DataUTF8")] string _dataUtf8;
        [JsonProperty("Level")] public int Level;

        //  Constructor section
        public Rule()
        {
            _range = _offset = Level = 0;
            _data = _dataUtf8 = "";
        }
        public Rule(int indent, int startOffset,
                            byte[] value, byte[] mask,
                            int wordSize, int rangeLength)
        {
            //  WordSize is not being used
            //  Mask is also derived, it could easily be replaced with multi-level Rule structure

            Level = indent;
            _range = rangeLength;
            _offset = startOffset;

            _data = ByteArrayToString(value);
            _dataUtf8 = Encoding.UTF8.GetString(value);
        }

        //  Check file header with given rule
        public bool CheckRule(byte[] inputArray)
        {
            if (inputArray != null)
            {
                //  Extract subarray [Offset; Range + Length of signature]
                //  Length must be divided by 2 in case byte array contatins elements of length 2
                byte[] cutArray = inputArray.Skip(_offset).Take(_data.Length / 2 + _range).ToArray();

                //  Find substring
                string cutArrayString = ByteArrayToString(cutArray);
                if (cutArrayString.Contains(_data))
                    return true;
            }
            return false;
        }

        //  Additional function to speed up array serach
        private string ByteArrayToString(byte[] array)
        {
            StringBuilder sb = new StringBuilder();
            string hexAlphabet = "0123456789ABCDEF";

            foreach (byte b in array)
            {
                sb.Append(hexAlphabet[(int)(b >> 4)]);
                sb.Append(hexAlphabet[(int)(b & 0xF)]);
            }

            return sb.ToString();
        }
    }
}
