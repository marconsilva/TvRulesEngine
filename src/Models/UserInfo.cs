namespace sampleRulesEngine.Models
{
    public class UserInfo
    {
        public string Username { get; set; }
        public List<string> Tags { get; set; }
        public bool IsCatalogItemSubscribed { get; set; }
        public bool IsCatalgItemPreviousSubscribed { get; set; }
        public DateTime? CatalogItemEndSubscriptionDate { get; set; }
        public bool IsCatalogItemPreviousPurchase { get; set; }
        
        public DateTime? CatalogItemLastPurchaseDate { get; set; }
        public DateTime UserSinceDate { get; set; }

        public List<string> UserAssignedDiscountCodes { get; set; }

        public string SubscribedOfferName { get; set; }
    }
    
}