using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MimeSharp
{
    //  Mime type representation
    public class CType
    {
        [JsonProperty("Name")]          public string Name;
        [JsonProperty("RuleSet")]       List<CRuleSet> RuleSet;
        [JsonProperty("Description")]   public string Description;
        [JsonProperty("Extensions")]    public List<string> Extensions;

        //  Constructor section
        public CType()
        {
            Name = Description = "";
            Extensions = new List<string>();
            RuleSet = new List<CRuleSet>();
        }
        public CType(string IName)
        {
            Name = IName;
            Description = "";
            Extensions = new List<string>();
            RuleSet = new List<CRuleSet>();
        }

        //  Add new set of rules
        public void AddNewRuleSet(CRule Rule)
        {
            RuleSet.Add(new CRuleSet(Rule));
        }
        //  Append a rule to the rule set
        public void AppendLastRuleSet(CRule Rule)
        {
            RuleSet.Last().Rules.Add(Rule);
        }

        //  Check file header with given mime type
        public bool CheckType(byte[] InputArray)
        {
            //  If one of the rule sets succeed, Mime is defined
            return (RuleSet.Count > 0) ? RuleSet.Select(x => x.CheckType(InputArray)).Aggregate((a, b) => a || b) : false;
        }
    }
}
