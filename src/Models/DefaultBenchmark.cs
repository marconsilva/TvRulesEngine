using RulesEngine.Models;

namespace sampleRulesEngine.Models
{
    public class DefaultBenchmark
    {
        public Guid BenchmarkId { get; set; }
        public string BenchmarkTestDisplayText { get; set; }
        public bool IsParallel { get; set; }
        public List<CatalogItem> CatalogItems { get; set; }
        public List<UserInfo> UsersInfo { get; set; }
        public List<Rule> Rules { get; set; }

        public DiscountCodes DiscountCodes { get; set; }

        public DefaultBenchmark()
        {
            CatalogItems = new List<CatalogItem>();
            UsersInfo = new List<UserInfo>();
            Rules = new List<Rule>();
        }
    }
}