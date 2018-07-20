using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MimeMagicSharp
{
    public class MimeTypeGuess
    {
        [JsonProperty("Name")]            public string Name;
        [JsonProperty("RuleSet")]         internal List<RuleSet> _ruleSet;
        [JsonProperty("Description")]     public string Description;
        [JsonProperty("Extensions")]      internal List<string> Extensions;

        //  Constructor section
        public MimeTypeGuess()
        {
            Name = Description = "";
            _ruleSet = new List<RuleSet>();
            Extensions = new List<string>();
        }
        public MimeTypeGuess(string name) : this() => Name = name;

        internal void AddNewRuleSet(Rule rule) => _ruleSet.Add(new RuleSet(rule));
        internal void AppendLastRuleSet(Rule rule) => _ruleSet.Last().Rules.Add(rule);

        internal bool CheckType(byte[] inputArray)
        {
            //  If one of the rule sets succeed, Mime is guessed
            return (_ruleSet.Count > 0) &&
                   _ruleSet.Select(x => x.CheckType(inputArray)).
                       Aggregate((a, b) => a || b);
        }
    }
}
