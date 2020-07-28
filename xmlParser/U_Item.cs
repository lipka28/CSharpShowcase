using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlParser
{
    class U_Item : Item
    {
        private string name = string.Empty;
        private string num_connections = string.Empty;
        private float charged_units = 0;
        private float free_units = 0;
        private float total_units = 0;
        private string unit_type = string.Empty;

        public U_Item(string name, string num_connections, float charged_units, float free_units, float total_units,
                      string unit_type, float tax, float p_tax, float p_wo_tax, float p_w_tax)
        {
            this.name = name;
            this.num_connections = num_connections;
            this.charged_units = charged_units;
            this.total_units = total_units;
            this.free_units = free_units;
            this.unit_type = unit_type;
            this.tax = tax;
            this.price_witout_tax = p_wo_tax;
            this.price_with_tax = p_w_tax;
            this.price_tax = p_tax;

        }

        public string Name { get => name; set => name = value; }
        public string Num_conections { get => num_connections; set => num_connections = value; }
        public float Charged_units { get => charged_units; set => charged_units = value; }
        public string Unit_type { get => unit_type; set => unit_type = value; }
        public float Free_units { get => free_units; set => free_units = value; }
        public float Total_units { get => total_units; set => total_units = value; }
    }
}
