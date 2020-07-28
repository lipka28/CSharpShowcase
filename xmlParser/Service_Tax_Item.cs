using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlParser
{
    class Service_Tax_Item : Item
    {
        private string tax_code = string.Empty;

        public Service_Tax_Item(float tax, float p_tax, float p_wo_tax, float p_w_tax, string tax_code)
        {
            this.tax_code = tax_code;
            this.tax = tax;
            this.price_tax = p_tax;
            this.price_witout_tax = p_wo_tax;
            this.price_with_tax = p_w_tax;
        }

        public string Tax_code { get => tax_code; }
    }
}
