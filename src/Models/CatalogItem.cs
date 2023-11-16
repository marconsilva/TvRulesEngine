using Microsoft.VisualBasic;

namespace sampleRulesEngine.Models
{
    public class CatalogItem
    {
        public string ItemID { get; set; }
        public double Price { get; set; } 
        public string Category { get; set; }
        public string CategoryPath { get; set; }
        public string Title { get; set; }
        public bool IsSubscription { get; set; }
        public bool IsDiscountable { get; set; }
        public bool IsBundle { get; set; }
        public List<string> Tags { get; set; }
    
        public CatalogItem()
        {
            Tags = new List<string>();
        }
    }
}