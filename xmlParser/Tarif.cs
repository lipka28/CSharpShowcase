using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlParser
{
    class Tarif
    {
        private string tarif_name = string.Empty;
        private string valid_from = string.Empty;
        private string valid_to = string.Empty;
        private string tarif_type;

        public Tarif(string tarif_name, string valid_from, string valid_to, string tarif_type)
        {
            this.tarif_name = tarif_name;
            this.valid_from = valid_from;
            this.valid_to = valid_to;
            this.tarif_type = tarif_type;

        }

        public string Valid_to { get => valid_to; }
        public string Valid_from { get => valid_from; }
        public string Tarif_name { get => tarif_name; }
        public string Tarif_type { get => tarif_type; }
    }
}
