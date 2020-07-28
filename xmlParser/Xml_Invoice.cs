using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

enum error
{
    SUCCESS = 0,
    FAIL,
    FILE_OPEN,
    NO_DATA,
};

namespace xmlParser
{
    class Xml_Invoice
    {
        private string file_path = string.Empty;
        private XmlDocument input_doc;
        private List<Subscriber> sub_list;

        private string bill_month = string.Empty;
        private string bill_year = string.Empty;
        private string invoice_date = string.Empty;
        private string invoice_num = string.Empty;
        private string cust_acc_name = string.Empty;
        private string cust_acc_num = string.Empty;
        //private string currency = string.Empty;
        private int cust_count = 0;

        public string Invoice_date { get => invoice_date; }
        public string Invoice_num { get => invoice_num; }
        public string Cust_acc_name { get => cust_acc_name; }
        public string Cust_acc_num { get => cust_acc_num; }
        //public string Currency { get => currency; }
        public string File_path { get => file_path; set => file_path = value; }
        public int Cust_count { get => cust_count; }
        public List<Subscriber> Sub_list { get => sub_list; }
        public string Bill_month { get => bill_month; }
        public string Bill_year { get => bill_year; }

        public Xml_Invoice()
        {
            input_doc = new XmlDocument();
            sub_list = new List<Subscriber>();
        }

        public error load_meta_from_file()
        {
            if (file_path == string.Empty) return error.FAIL;
            try
            {
                input_doc.Load(file_path);
            }
            catch (XmlException)
            {
                return error.FAIL;
            }

            try
            {
                XmlNode node = input_doc.DocumentElement.ChildNodes[0];
                invoice_date = node.Attributes["day"].Value + "."
                              + node.Attributes["month"].Value + "."
                              + node.Attributes["year"].Value;

                string tmp = node.ChildNodes[0].Attributes["from"].Value;
                bill_month = tmp.Split('-')[1] + "-" + tmp.Split('-')[0];

                invoice_num = node.Attributes["invoiceNumber"].Value;
                cust_acc_name = node.Attributes["customerAccountName"].Value;
                cust_acc_num = node.Attributes["customerAccountNumber"].Value;
                //currency = node.Attributes["currency"].Value;
                cust_count = 0;

                foreach (XmlNode s_node in input_doc.DocumentElement.ChildNodes[1].ChildNodes)
                {
                    cust_count++;
                }
            }
            catch (System.NullReferenceException)
            {
                return error.FAIL;
            }

            return error.SUCCESS;
        }

        public error load_data()
        {
            foreach (XmlNode sub_node in input_doc.DocumentElement.ChildNodes[1].ChildNodes)
            {
                Subscriber sub = new Subscriber();
                if (sub.read_subscriber_from_node(sub_node) == error.FAIL)
                {
                    return error.FAIL;
                }
                sub_list.Add(sub);
            }
            
            return error.SUCCESS;
        }
    }
}
