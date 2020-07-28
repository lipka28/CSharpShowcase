using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlParser
{
    class Free_Unit
    {
        private string name = string.Empty;
        private string display_unit = string.Empty;
        private float actual_period = 0;
        private float actual_period_used = 0;

        public Free_Unit(string name, string display_unit, float ap, float ap_used)
        {
            this.name = name;
            this.display_unit = display_unit;
            this.actual_period = ap;
            this.actual_period_used = ap_used;

        }

        public string Name { get => name; set => name = value; }
        public string Display_unit { get => display_unit; set => display_unit = value; }
        public float Actual_period { get => actual_period; set => actual_period = value; }
        public float Actual_period_used { get => actual_period_used; set => actual_period_used = value; }
    }
}
