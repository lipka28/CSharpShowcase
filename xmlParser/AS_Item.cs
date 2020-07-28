using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlParser
{
    class AS_Item : Item
    {
        private string name;
        private string unit_type;
        private string amount;

        public AS_Item(string name, string unit_name, string amount, float tax,
                        float p_tax, float p_wo_tax, float p_w_tax)
        {
            this.name = name;
            this.unit_type = unit_name;
            this.amount = amount;
            this.tax = tax;
            this.price_tax = p_tax;
            this.price_witout_tax = p_wo_tax;
            this.price_with_tax = p_w_tax;
                       
        }

        public string Name { get => name; set => name = value; }
        public string Unit_type { get => unit_type; set => unit_type = value; }
        public string Amount { get => amount; set => amount = value; }
    }
}
