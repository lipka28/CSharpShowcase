using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlParser
{
    class Payment
    {
        private string payment_name;
        private string unit_type;
        private string amount;
        private float price;

        public Payment(string p_name, string unit_type, string amount, float price)
        {
            this.payment_name = p_name;
            this.unit_type = unit_type;
            this.amount = amount;
            this.price = price;

        }

        public string Payment_name { get => payment_name; set => payment_name = value; }
        public string Unit_type { get => unit_type; set => unit_type = value; }
        public string Amount { get => amount; set => amount = value; }
        public float Price { get => price; set => price = value; }
    }
}
