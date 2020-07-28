using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlParser
{
    class WatFree
    {
        private string fee_name = string.Empty;
        private string valid_from = string.Empty;
        private string valid_to = string.Empty;
        private float amount = 0;

        public WatFree(string fee_name, string valid_from, string valid_to, float amount)
        {
            this.fee_name = fee_name;
            this.valid_from = valid_from;
            this.valid_to = valid_to;
            this.amount = amount;
        }

        public string Fee_name { get => fee_name; }
        public string Valid_from { get => valid_from; }
        public string Valid_to { get => valid_to; }
        public float Amount { get => amount; }
        public float Price_with_tax { get => amount; }
        public float Price_without_tax { get => amount; }

    }
}
