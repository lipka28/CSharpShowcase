using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xmlParser
{
    class Xml_Manipulator
    {
        private List<Xml_Invoice> invoices;
        private Dictionary<string, string> phone_name_db;
        private int customers = 0;

        private void count_cust()
        {
            customers = 0;
            foreach (Xml_Invoice invoice in invoices)
            {
                customers += invoice.Cust_count;
            }
        }

        private bool is_valid_name(string name)
        {
            bool parse_works;
            string tmp = name.Split(' ')[0];
            parse_works = int.TryParse(tmp, out int num);
            return parse_works;

        }

        private void generate_association_db()
        {
            phone_name_db = new Dictionary<string, string>();

            for (int i = invoices.Count - 1; i >= 0; i--)
            {
                Xml_Invoice tmp_inv = invoices[i];
                for (int j = tmp_inv.Sub_list.Count - 1; j >= 0; j--)
                {
                    Subscriber tmp_sub = tmp_inv.Sub_list[j];
                    string phoneNum = tmp_sub.Info.Phone_number;
                    string subName = tmp_sub.Info.Tarif_s_name;

                    if (phone_name_db.ContainsKey(phoneNum))
                    {
                        if (is_valid_name(subName)) phone_name_db[phoneNum] = subName;
                    }
                    else phone_name_db.Add(phoneNum, subName);

                }
            }
        }

        public Xml_Manipulator()
        {
            invoices = new List<Xml_Invoice>();
        }

        public void delete_all()
        {
            foreach (Xml_Invoice invoice in invoices)
            {
                invoice.Sub_list.Clear();
            }
            invoices.Clear();
            count_cust();
        }


        public error load_files(List<string> files)
        {
            foreach (string file in files)
            {
                Xml_Invoice invoice = new Xml_Invoice();
                bool unique = true;
                invoice.File_path = file;
                if (invoice.load_meta_from_file() == error.FAIL) continue;
                foreach (Xml_Invoice tmp in invoices)
                {
                    if (invoice.Invoice_num == tmp.Invoice_num)
                    {
                        unique = false;
                        break;
                    }
                }
                if (unique == false)
                    continue;
                if (invoice.load_data() == error.FAIL)
                {
                    return error.FAIL;
                }
                invoices.Add(invoice);
            }
            count_cust();
            generate_association_db();
            return error.SUCCESS;
        }

        public error save_to_csv_var1(string target_file)
        {
            if (invoices.Count == 0) return error.NO_DATA;

            StringBuilder csv = new StringBuilder();
            string header = "Období;Kód divize;Telefoní číslo;Jméno;Tarif;Kč bez DPH;" +
                            "Vyčerpaná data(MB);Provolané min. ČR;Provolané min. svět;Počet SMS v rámci ČR a EU;" +
                            "Počet SMS z ČR do zahraničí;Počet SMS ve světě;Počet MMS;Jednorázové data(kč bez DPH);" +
                            "Počet obnovení;Název balíku;Změna tarifu";
            csv.AppendLine(header);

            try
            {
                using (var sw = new StreamWriter(target_file, false, Encoding.UTF8))
                {
                    foreach (Xml_Invoice inv in invoices)
                    {
                        foreach (Subscriber sub in inv.Sub_list)
                        {
                            csv.AppendLine(inv.Bill_month + ";"
                                         + sub.generate_csv_var1_line(inv.Cust_acc_name, phone_name_db));
                        }
                    }
                    sw.WriteLine(csv.ToString().TrimEnd('\r', '\n'));
                }
                return error.SUCCESS;
            }
            catch (System.IO.IOException)
            {
                return error.FILE_OPEN;
            }
            
        }

        public int Customers_count { get => customers; set => customers = value; }
    }

}
