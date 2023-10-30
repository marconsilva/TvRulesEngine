using System.Collections.Generic;

namespace sampleRulesEngine.Models
{
    public class ResultOutput
    {
        public List<Product> Products { get; set; }

        public ResultOutput()
        {
            Products = new List<Product>();
        }
    }
}
