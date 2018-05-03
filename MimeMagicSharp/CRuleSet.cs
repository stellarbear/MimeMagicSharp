using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MimeMagicSharp
{
    //  Rule set representation
    class CRuleSet
    {
        [JsonProperty("Rule")]  public List<CRule> Rules;

        //  Constructor section
        public CRuleSet() { Rules = new List<CRule>(); }
        public CRuleSet(CRule Rule)
        {
            Rules = new List<CRule>() { Rule };
        }

        //  Check file header with given rule set
        public bool CheckType(byte[] InputArray)
        {
            int LevelPointer = 0;
            List<CRule> LevelRules;

            //  Rule set can contain several rules with sertain hierarchy
            //  Level 0. indent 0
            //  Level 1. indent 1, indent 2, indent 3
            //  Level 2. indent 4
            //  Level 3. indent 5
            //  Result = 0 && (1 || 2 || 3) && 4 && 5 ...
            do
            {
                //  Get list of rules for the specified level
                LevelRules = Rules.Where(x => x.Level == LevelPointer).ToList();

                //  If level rules are not empty
                if (LevelRules.Count > 0)
                {
                    bool IndentRuleResult = LevelRules.Select(x => x.CheckRule(InputArray)).Aggregate((a, b) => a || b);

                    //  If false => no need to check any further
                    if (!IndentRuleResult) return false;
                }

                LevelPointer++;
            }
            while (LevelRules.Count > 0);

            return true;
        }
    }
}
