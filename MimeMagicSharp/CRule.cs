using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MimeMagicSharp
{
    //  Rule representation
    public class CRule
    {
        [JsonProperty("Range")]     int Range;
        [JsonProperty("Offset")]    int Offset;
        [JsonProperty("Data")]      string Data;
        [JsonProperty("DataUTF8")]  string DataUTF8;
        [JsonProperty("Level")]     public int Level;

        //  Constructor section
        public CRule()
        { Range = Offset = Level = 0; Data = DataUTF8 = ""; }
        public CRule(int Indent, int StartOffset,
                            byte[] Value, byte[] Mask,
                            int WordSize, int RangeLength)
        {
            //  WordSize is not being used
            //  Mask is also derived, it could easily be replaced with multi-level Rule structure

            Level = Indent;
            Range = RangeLength;
            Offset = StartOffset;

            Data = ByteArrayToString(Value);
            DataUTF8 = Encoding.UTF8.GetString(Value);
        }

        //  Check file header with given rule
        public bool CheckRule(byte[] InputArray)
        {
            if (InputArray != null)
            {
                //  Extract subarray [Offset; Range + Length of signature]
                byte[] CutArray = InputArray.Skip(Offset).Take(Data.Length + Range).ToArray();

                //  Find substring
                string CutArrayString = ByteArrayToString(CutArray);
                if (CutArrayString.Contains(Data))
                    return true;

            }
            return false;
        }

        //  Additional function to speed up array serach
        protected string ByteArrayToString(byte[] Array)
        {
            StringBuilder SB = new StringBuilder();
            string hexAlphabet = "0123456789ABCDEF";

            foreach (byte b in Array)
            {
                SB.Append(hexAlphabet[(int)(b >> 4)]);
                SB.Append(hexAlphabet[(int)(b & 0xF)]);
            }

            return SB.ToString();
        }
    }
}
