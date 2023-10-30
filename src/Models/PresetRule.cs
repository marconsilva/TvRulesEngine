namespace sampleRulesEngine.Models
{
    public class PresetRule
    {
        public Guid Id { get; set; }
        public string DisplayText { get; set; } 

        public string RuleJson { get; set; }
    }
}