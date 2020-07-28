using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlParser
{
    class Timed_Item : Item
    {
        private string fee_name = string.Empty;
        private string valid_from = string.Empty;
        private string valid_to = string.Empty;
        private string count = string.Empty;

        public Timed_Item(string fee_name, string valid_from, string valid_to
                    , string count, float tax, float p_tax, float p_wo_tax, float p_w_tax)
        {
            this.fee_name = fee_name;
            this.valid_from = valid_from;
            this.valid_to = valid_to;
            this.count = count;
            this.tax = tax;
            this.price_witout_tax = p_wo_tax;
            this.price_with_tax = p_w_tax;
            this.price_tax = p_tax;
        }

        public string Fee_name { get => fee_name; }
        public string Valid_from { get => valid_from; }
        public string Valid_to { get => valid_to; }
        public string Count { get => count; }
    }
}
