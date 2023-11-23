using RulesEngine.Models;

namespace sampleRulesEngine.Models
{
    public class DefaultInput
    {
        public List<CatalogItem> CatalogItems { get; set; }
        public List<UserInfo> UsersInfo { get; set; }
        public string InputDisplayText { get; set; }
        public List<Rule> Rules { get; set; }
        public Guid InputId { get; set; }

        public DiscountCodes DiscontCodes { get; set; }

        public DefaultInput()
        {
            CatalogItems = new List<CatalogItem>();
            UsersInfo = new List<UserInfo>();
            Rules = new List<Rule>();
        }
    }
}