using System.Collections.Generic;

namespace sampleRulesEngine.Models
{
    public class ResultOutput
    {
        public List<Product> Products { get; set; }
        public string OutputError { get; set; }

        public string Message { get; set; }

        public ResultOutput()
        {
            Products = new List<Product>();
        }
    }
}
