using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MimeMagicSharp
{
    public class MimeTypeGuess
    {
        [JsonProperty("Name")] public string Name;
        [JsonProperty("RuleSet")] internal List<RuleSet> RuleSet;
        [JsonProperty("Description")] public string Description;
        [JsonProperty("Extensions")] internal List<string> Extensions;

        //  Constructor section
        public MimeTypeGuess()
        {
            Name = Description = "";
            RuleSet = new List<RuleSet>();
            Extensions = new List<string>();
        }
        public MimeTypeGuess(string name) : this() => Name = name;

        public MimeTypeGuess(string name, string description) : this()
        {
            Name = name;
            Description = description;
        }

        internal void AddNewRuleSet(Rule rule) => RuleSet.Add(new RuleSet(rule));
        internal void AppendLastRuleSet(Rule rule) => RuleSet.Last().Rules.Add(rule);

        internal bool CheckType(byte[] inputArray)
        {
            //  If one of the rule sets succeed, Mime is guessed
            return (RuleSet.Count > 0) &&
                   RuleSet.Select(x => x.CheckType(inputArray)).
                       Aggregate((a, b) => a || b);
        }
    }
}
