using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlParser
{
    class Subscriber_Info
    {
        private string acc_number = string.Empty;
        private string service_number = string.Empty;
        private string tarif_s_number = string.Empty;
        private string tarif_s_name = string.Empty;
        private string phone_number = string.Empty;
        private float sum_price_tax;
        private float sum_price_wo_tax;
        private float sum_price_w_tax;


        public Subscriber_Info(string acc_number, string service_number, string tarif_s_number, string tarif_s_name,
                               string phone_number, float sum_price_tax, float sum_price_wo_tax, float sum_price_w_tax)
        {
            this.acc_number = acc_number;
            this.service_number = service_number;
            this.tarif_s_number = tarif_s_number;
            this.tarif_s_name = tarif_s_name;
            this.phone_number = phone_number;
            this.sum_price_tax = sum_price_tax;
            this.sum_price_wo_tax = sum_price_wo_tax;
            this.sum_price_w_tax = sum_price_w_tax;

        }

        public string Tarif_s_number { get => tarif_s_number; }
        public string Service_number { get => service_number; }
        public string Acc_number { get => acc_number; }
        public float Sum_price_tax { get => sum_price_tax; }
        public float Sum_price_wo_tax { get => sum_price_wo_tax; }
        public float Sum_price_w_tax { get => sum_price_w_tax; }
        public string Phone_number { get => phone_number; }
        public string Tarif_s_name { get => tarif_s_name; }
    }
}
