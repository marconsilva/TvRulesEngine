namespace sampleRulesEngine.Models
{
    public class InputObject
    {
        public string UserID { get; set; }
        public string UserLocation { get; set; } 
        public List<CatalogItemPurchase> PreviousPurchases { get; set; }
        public Offer CurrentOffer { get; set; } 
        public List<CatalogItem> SubscribedServices { get; set; } 
        public CatalogItem ItemToPurchase { get; set; } 

        public InputObject()
        {
            PreviousPurchases = new List<CatalogItemPurchase>();
            CurrentOffer = new Offer();
            SubscribedServices = new List<CatalogItem>();
            ItemToPurchase = new CatalogItem();
        }
    }
}