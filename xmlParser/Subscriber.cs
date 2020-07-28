using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace xmlParser
{
    class Subscriber
    {
        private Subscriber_Info info;                       // třída obsahující základní info o zákazníkovi včetně celkové ceny za služby
        private List<Tarif> tarifs;                         // seznam tarifů zákazníka
        private List<Timed_Item> discounts;                 // seznam slev zákazníka
        private List<WatFree> wat_free_p;                   // seznam plateb nepostihnuté daní (VAT)
        //------------Sumary data-----------------//
        private List<Timed_Item> one_time_charges;          // seznam jednorázových plateb
        private List<Timed_Item> regular_charges;           // seznam pravidelných plateb
        private List<U_Item> voice_connection_charges;      // seznam plateb za volání
        private List<U_Item> mess_connection_charges;       // seznam plateb za zprávy
        private List<U_Item> data_connection_charges;       // seznam plateb za data
        private List<Free_Unit> free_units;                 // seznam volných jednotek (pro hlas, zprávy nebo data)
        private List<Payment> payments;                     // seznam dalších plateb většinou za služby třetích stran
        private List<AS_Item> additional_services;          // seznam doplňkových služeb
        //------------Sumary data-----------------//
        private List<Service_Tax_Item> service_tax_groups;  // seznam daňových skupin (většinou jen jedna 21%) 
        private List<Service_Tax_Item> service_tax_g_ts;    // taky seznam daňových skupin.. netuším jak se liší od ^

        private float str_to_minutes_f(string sec)       // konverze ze stringu sekund na string minut
        {
            int in_sec = int.Parse(sec);
            float minutes = (float)(in_sec / 60.0);

            return minutes;
        }

        private float str_to_float(string str)              // konverze stringu na float
        {
            var s = Convert.ToDecimal(str, CultureInfo.InvariantCulture);
            float st = (float)s;

            return st;
        }

        private float kbyte_to_mbyte(string str)           // konverze dat o 3 místa nahoru KB -> MB atd..
        {
            var s = Convert.ToDecimal(str, CultureInfo.InvariantCulture);
            float st = ( (float)s / 1024);
            return st;
        }

        private int count_sms_czeu()
        {
            int count = 0;
            foreach (U_Item messPack in mess_connection_charges)
            {
                if (messPack.Unit_type == "SMS" 
                    && (messPack.Name == "SMS v rámci ČR do O2" 
                    || messPack.Name == "SMS v rámci ČR mimo O2" 
                    || messPack.Name == "SMS v rámci EU"))
                {
                    count += (int)messPack.Total_units;
                }
            }

            return count;
        }

        private int count_sms_cz_to_noneu()
        {
            int count = 0;
            foreach (U_Item messPack in mess_connection_charges)
            {
                if (messPack.Unit_type == "SMS"
                    && messPack.Name == "SMS z ČR do zahraničí")
                {
                    count += (int)messPack.Total_units;
                }
            }

            return count;
        }

        private int count_sms_world()
        {
            int count = 0;
            foreach (U_Item messPack in mess_connection_charges)
            {
                if (messPack.Unit_type == "SMS"
                    && messPack.Name == "SMS mimo EU")
                {
                    count += (int)messPack.Total_units;
                }
            }

            return count;
        }

        private int count_mms()
        {
            int count = 0;
            foreach (U_Item messPack in mess_connection_charges)
            {
                if (messPack.Unit_type == "MMS")
                {
                    count += (int)messPack.Total_units;
                }
            }

            return count;
        }

        private error load_discounts(XmlNode d_node)        // funkce pro naprsování slev z xml souboru
        {
            foreach (XmlNode node in d_node)
            {
                Timed_Item dis = new Timed_Item(node.Attributes["discountItemName"].Value,
                                                    node.Attributes["validFrom"].Value,
                                                    node.Attributes["validTo"].Value,
                                                    node.Attributes["count"].Value,
                                                    str_to_float(node.Attributes["tax"].Value),
                                                    str_to_float(node.Attributes["priceTax"].Value),
                                                    str_to_float(node.Attributes["priceWithoutTax"].Value),
                                                    str_to_float(node.Attributes["priceWithTax"].Value));
                discounts.Add(dis);
            }
            return error.SUCCESS;
        }

        private error load_vat_free(XmlNode vaf_node)       // funkce pro naprsování plateb mimo VAT z xml souboru
        {
            foreach (XmlNode node in vaf_node.ChildNodes)
            {
                WatFree no_vat = new WatFree(node.Attributes["paymentsFovItemName"].Value,
                                            node.Attributes["validFrom"].Value,
                                            node.Attributes["validTo"].Value,
                                            str_to_float(node.Attributes["amount"].Value));
                wat_free_p.Add(no_vat);
            }
            return error.SUCCESS;
        }

        private error load_tarifs(XmlNode node)             // funkce pro naprsování tarifů z xml souboru
        {
            foreach (XmlNode t_node in node.ChildNodes)
            {
                Tarif t_tariff = new Tarif(t_node.Attributes["tariffName"].Value,
                                           t_node.Attributes["validFrom"].Value,
                                           t_node.Attributes["validTo"].Value,
                                           t_node.Attributes["tariffType"].Value);
                tarifs.Add(t_tariff);
            }
            return error.SUCCESS;
        }

        private error load_service_taxes(XmlNode t_node)    // funkce pro naprsování daňových skupin z xml souboru
        {
            foreach (XmlNode node in t_node.ChildNodes)
            {
                Service_Tax_Item s_tax = new Service_Tax_Item(str_to_float(node.Attributes["tax"].Value),
                                                    str_to_float(node.Attributes["priceTax"].Value),
                                                    str_to_float(node.Attributes["priceWithoutTax"].Value),
                                                    str_to_float(node.Attributes["priceWithTax"].Value),
                                                    node.Attributes["taxCodeGroup"].Value);

                switch (t_node.Name)
                {
                    case "serviceTax":
                        service_tax_groups.Add(s_tax);
                        break;
                    case "serviceTaxTS":
                        service_tax_g_ts.Add(s_tax);
                        break;
                    default:
                        return error.FAIL;
                }
            }
            return error.SUCCESS;
        }

        private error load_one_time_regular_charges(XmlNode o_node) // funkce pro naprsování jednorázových a pravidelných plateb z xml souboru
        {
            foreach (XmlNode node in o_node.ChildNodes)
            {

                Timed_Item charge = new Timed_Item(node.Attributes["feeName"].Value,
                                                    node.Attributes["validFrom"].Value,
                                                    node.Attributes["validTo"].Value,
                                                    node.Attributes["count"].Value,
                                                    str_to_float(node.Attributes["tax"].Value),
                                                    str_to_float(node.Attributes["priceTax"].Value),
                                                    str_to_float(node.Attributes["priceWithoutTax"].Value),
                                                    str_to_float(node.Attributes["priceWithTax"].Value));

                if (o_node.Name == "regularCharges") regular_charges.Add(charge);
                else if (o_node.Name == "oneTimeCharges") one_time_charges.Add(charge);
            }
            return error.SUCCESS;
        }

        private error load_payments(XmlNode p_node)         // funkce pro naprsování plateb (třtím stranám) z xml souboru
        {
            foreach (XmlNode node in p_node.ChildNodes[0].ChildNodes)
            {
                Payment p_item = new Payment(node.Attributes["paymentItemName"].Value,
                                             node.Attributes["displayedUom"].Value,
                                             node.Attributes["amount"].Value,
                                             str_to_float(node.Attributes["price"].Value));
                payments.Add(p_item);
            }
            return error.SUCCESS;
        }

        private error load_additional_services(XmlNode as_node)// funkce pro naprsování dolňkových služeb z xml souboru
        {
            foreach (XmlNode node in as_node.ChildNodes)
            {
                AS_Item a_service = new AS_Item(node.Attributes["feeName"].Value,
                                                node.Attributes["displayedUom"].Value,
                                                node.Attributes["amount"].Value,
                                                str_to_float(node.Attributes["tax"].Value),
                                                str_to_float(node.Attributes["priceTax"].Value),
                                                str_to_float(node.Attributes["priceWithoutTax"].Value),
                                                str_to_float(node.Attributes["priceWithTax"].Value));

                additional_services.Add(a_service);

            }
            return error.SUCCESS;
        }

        private error load_free_units(XmlNode fu_node)      // funkce pro naprsování volných jednotek (volání,správy,data) z xml souboru
        {
            foreach (XmlNode a_node in fu_node.ChildNodes)
            {
                switch (a_node.Attributes["fuType"].Value)
                {
                    case "V":
                        foreach (XmlNode node in a_node.ChildNodes)
                        {
                            Free_Unit f_unit = new Free_Unit(node.Attributes["fuItemName"].Value,
                                                             node.Attributes["displayedUom"].Value,
                                                             str_to_minutes_f(node.Attributes["actualPeriod"].Value),
                                                             str_to_minutes_f(node.Attributes["actualPeriodUsed"].Value));

                            free_units.Add(f_unit);
                        }
                        break;
                    case "M":
                        foreach (XmlNode node in a_node.ChildNodes)
                        {
                            Free_Unit f_unit = new Free_Unit(node.Attributes["fuItemName"].Value,
                                                             node.Attributes["displayedUom"].Value,
                                                             str_to_float(node.Attributes["actualPeriod"].Value),
                                                             str_to_float(node.Attributes["actualPeriodUsed"].Value));

                            free_units.Add(f_unit);
                        }
                        break;
                    case "D":
                        foreach (XmlNode node in a_node.ChildNodes)
                        {
                            Free_Unit f_unit = new Free_Unit(node.Attributes["fuItemName"].Value,
                                                             node.Attributes["displayedUom"].Value,
                                                             kbyte_to_mbyte(node.Attributes["actualPeriod"].Value),
                                                             kbyte_to_mbyte(node.Attributes["actualPeriodUsed"].Value));

                            free_units.Add(f_unit);
                        }
                        break;
                    default:
                        return error.SUCCESS;
                }
            }
            return error.SUCCESS;
        }

        private error load_usage_charges(XmlNode u_node)    // funkce pro naprsování jednotlivých plateb za volání, správy a data z xml souboru
        {
            foreach (XmlNode node in u_node.ChildNodes)
            {
                switch (node.Attributes["usageType"].Value)
                {
                    case "V":
                        foreach (XmlNode in_node in node.ChildNodes)
                        {
                            U_Item vcharge = new U_Item(in_node.Attributes["name"].Value,
                                                        in_node.Attributes["quantityOfConnect"].Value,
                                                        str_to_minutes_f(in_node.Attributes["chargedUnits"].Value),
                                                        str_to_minutes_f(in_node.Attributes["freeUnits"].Value),
                                                        str_to_minutes_f(in_node.Attributes["totalUnits"].Value),
                                                        in_node.Attributes["displayedUom"].Value,
                                                        str_to_float(in_node.Attributes["tax"].Value),
                                                        str_to_float(in_node.Attributes["priceTax"].Value),
                                                        str_to_float(in_node.Attributes["priceWithoutTax"].Value),
                                                        str_to_float(in_node.Attributes["priceWithTax"].Value));
                            voice_connection_charges.Add(vcharge);
                        }
                        break;

                    case "M":
                        foreach (XmlNode in_node in node.ChildNodes)
                        {
                            U_Item mcharge = new U_Item(in_node.Attributes["name"].Value,
                                                        in_node.Attributes["quantityOfConnect"].Value,
                                                        str_to_float(in_node.Attributes["chargedUnits"].Value),
                                                        str_to_float(in_node.Attributes["freeUnits"].Value),
                                                        str_to_float(in_node.Attributes["totalUnits"].Value),
                                                        in_node.Attributes["displayedUom"].Value,
                                                        str_to_float(in_node.Attributes["tax"].Value),
                                                        str_to_float(in_node.Attributes["priceTax"].Value),
                                                        str_to_float(in_node.Attributes["priceWithoutTax"].Value),
                                                        str_to_float(in_node.Attributes["priceWithTax"].Value));
                            mess_connection_charges.Add(mcharge);
                        }
                        break;

                    case "D":
                        foreach (XmlNode in_node in node.ChildNodes)
                        {
                            U_Item dcharge = new U_Item(in_node.Attributes["name"].Value,
                                                        in_node.Attributes["quantityOfConnect"].Value,
                                                        kbyte_to_mbyte(in_node.Attributes["chargedUnits"].Value),
                                                        kbyte_to_mbyte(in_node.Attributes["freeUnits"].Value),
                                                        kbyte_to_mbyte(in_node.Attributes["totalUnits"].Value),
                                                        in_node.Attributes["displayedUom"].Value,
                                                        str_to_float(in_node.Attributes["tax"].Value),
                                                        str_to_float(in_node.Attributes["priceTax"].Value),
                                                        str_to_float(in_node.Attributes["priceWithoutTax"].Value),
                                                        str_to_float(in_node.Attributes["priceWithTax"].Value));
                            data_connection_charges.Add(dcharge);
                        }
                        break;

                    default:
                        return error.SUCCESS;
                }
            }
            return error.SUCCESS;
        }

        private string get_total_data_units()               // funkce pro výpočet celkem spotřebovaných dat zákazníkem (v MB)
        {
            float sum = 0;
            if (data_connection_charges.Count == 0) return "0" ;
            foreach (U_Item data in data_connection_charges)
            {
                sum += data.Total_units;
            }
            return sum.ToString();
        }

        private string get_total_voice_cz()                 // funkce pro výpočet celkem zpotřebovaných minut volání v rámci ČR (v minutach)
        {
            float sum = 0;
            if (voice_connection_charges.Count == 0) return "0";
            foreach (U_Item voice in voice_connection_charges)
            {
                if (voice.Name == "V rámci ČR do O2" || voice.Name == "V rámci ČR mimo O2")
                    sum += voice.Total_units;

            }
            return sum.ToString();
        }

        private string get_total_voice_noncz()              // funkce pro výpočet celkem zpotřebovaných minut volání mimo ČR (v minutach)
        {
            float sum = 0;
            if (voice_connection_charges.Count == 0) return "0";
            foreach (U_Item voice in voice_connection_charges)
            {
                if (voice.Name != "V rámci ČR do O2" && voice.Name != "V rámci ČR mimo O2" && voice.Name != "V rámci Team Combi")
                    sum += voice.Total_units;

            }
            return sum.ToString();
        }

        private error load_summary_data(XmlNode node)       // hlavní funkce pro parsování plateb zákazníka
        {
            foreach (XmlNode s_node in node.ChildNodes)
            {
                switch (s_node.Name)
                {
                    case "oneTimeCharges":
                        load_one_time_regular_charges(s_node);
                        break;

                    case "regularCharges":
                        load_one_time_regular_charges(s_node);
                        break;

                    case "usageCharges":
                        load_usage_charges(s_node);
                        break;

                    case "discounts":
                        load_discounts(s_node);
                        break;

                    case "paymentsFreeOfVAT":
                        load_vat_free(s_node);
                        break;

                    case "payments":
                        load_payments(s_node);
                        break;

                    case "freeUnits":
                        load_free_units(s_node);
                        break;

                    case "additionalServices":
                        load_additional_services(s_node);
                        break;

                    default:
                        return error.SUCCESS;
                }
            }
            return error.SUCCESS;
        }
        //---------------------------------------------------------Veřejné metody--------------------------------------------------//
        public Subscriber()                                 // konstruktor a jednotlivé inicializace
        {
            tarifs = new List<Tarif>();
            discounts = new List<Timed_Item>();
            wat_free_p = new List<WatFree>();
            one_time_charges = new List<Timed_Item>();
            regular_charges = new List<Timed_Item>();
            voice_connection_charges = new List<U_Item>();
            mess_connection_charges = new List<U_Item>();
            data_connection_charges = new List<U_Item>();
            service_tax_groups = new List<Service_Tax_Item>();
            service_tax_g_ts = new List<Service_Tax_Item>();
            payments = new List<Payment>();
            additional_services = new List<AS_Item>();
            free_units = new List<Free_Unit>();
        }
        //------------------------------------------------------getters / setters---------------------------------------------------//
        public Subscriber_Info Info { get => info; }
        public List<Tarif> Tarifs { get => tarifs; }
        public List<Timed_Item> Discounts { get => discounts; }
        public List<WatFree> Wat_free_p { get => wat_free_p; }
        public List<Timed_Item> One_time_charges { get => one_time_charges; }
        public List<Timed_Item> Regular_charges { get => regular_charges; }
        public List<U_Item> Voice_connection_charges { get => voice_connection_charges; }
        public List<U_Item> Mess_connection_charges { get => mess_connection_charges; }
        public List<U_Item> Data_connection_charges { get => data_connection_charges; }
        public List<Free_Unit> Free_units { get => free_units; }
        public List<Payment> Payments { get => payments; }
        public List<AS_Item> Additional_services { get => additional_services; }
        public List<Service_Tax_Item> Service_tax_groups { get => service_tax_groups; }
        public List<Service_Tax_Item> Service_tax_g_ts { get => service_tax_g_ts; }

        //------------------------------------------------------getters / setters---------------------------------------------------//

        public Item get_sum_one_time_charges()              // veřejná funkce pro získání součtu všech jednorázových plateb
        {
            Item i_sum = new Item();
            i_sum.price_tax = 0;
            i_sum.price_with_tax = 0;
            i_sum.price_witout_tax = 0;
            i_sum.tax = 0;

            foreach (Timed_Item stuff in one_time_charges)
            {
                i_sum.price_tax += stuff.price_tax;
                i_sum.price_with_tax += stuff.price_with_tax;
                i_sum.price_witout_tax += stuff.price_witout_tax;
                i_sum.tax = stuff.tax;
            }
            return i_sum;
        }

        public Item get_sum_regular_charges()               // veřejná funkce pro získání součtu všech pravidelných plateb
        {
            Item i_sum = new Item();
            i_sum.price_tax = 0;
            i_sum.price_with_tax = 0;
            i_sum.price_witout_tax = 0;
            i_sum.tax = 0;

            foreach (Timed_Item stuff in regular_charges)
            {
                i_sum.price_tax += stuff.price_tax;
                i_sum.price_with_tax += stuff.price_with_tax;
                i_sum.price_witout_tax += stuff.price_witout_tax;
                i_sum.tax = stuff.tax;
            }
            return i_sum;
        }

        public Item get_sum_voice_charges()                 // veřejná funkce pro získání součtu všech poplatků za volání
        {
            Item i_sum = new Item();
            i_sum.price_tax = 0;
            i_sum.price_with_tax = 0;
            i_sum.price_witout_tax = 0;
            i_sum.tax = 0;

            foreach (U_Item stuff in voice_connection_charges)
            {
                i_sum.price_tax += stuff.price_tax;
                i_sum.price_with_tax += stuff.price_with_tax;
                i_sum.price_witout_tax += stuff.price_witout_tax;
                i_sum.tax = stuff.tax;
            }
            return i_sum;
        }

        public Item get_sum_message_charges()               // veřejná funkce pro získání součtu všech poplatků za správy
        {
            Item i_sum = new Item();
            i_sum.price_tax = 0;
            i_sum.price_with_tax = 0;
            i_sum.price_witout_tax = 0;
            i_sum.tax = 0;

            foreach (U_Item stuff in mess_connection_charges)
            {
                i_sum.price_tax += stuff.price_tax;
                i_sum.price_with_tax += stuff.price_with_tax;
                i_sum.price_witout_tax += stuff.price_witout_tax;
                i_sum.tax = stuff.tax;
            }
            return i_sum;
        }

        public Item get_sum_data_charges()                  // veřejná funkce pro získání součtu všech poplatků za data
        {
            Item i_sum = new Item();
            i_sum.price_tax = 0;
            i_sum.price_with_tax = 0;
            i_sum.price_witout_tax = 0;
            i_sum.tax = 0;

            foreach (U_Item stuff in data_connection_charges)
            {
                i_sum.price_tax += stuff.price_tax;
                i_sum.price_with_tax += stuff.price_with_tax;
                i_sum.price_witout_tax += stuff.price_witout_tax;
                i_sum.tax = stuff.tax;
            }
            return i_sum;
        }

        public Item get_sum_usage_charges()                 // veřejná funkce pro získání součtu všech poplatků za užití (volání, sprývy, data)
        {
            Item i_sum = new Item();
            i_sum.price_tax = 0;
            i_sum.price_with_tax = 0;
            i_sum.price_witout_tax = 0;
            i_sum.tax = 0;

            Item v_c = get_sum_voice_charges();
            Item m_c = get_sum_message_charges();
            Item d_c = get_sum_data_charges();

            i_sum.price_tax = v_c.price_tax + m_c.price_tax + d_c.price_tax;
            i_sum.price_with_tax = v_c.price_with_tax + m_c.price_with_tax + d_c.price_with_tax;
            i_sum.price_witout_tax = v_c.price_witout_tax + m_c.price_witout_tax + d_c.price_witout_tax;
            i_sum.tax = v_c.tax;
            
            return i_sum;
        }

        public float get_sum_payments()                     // veřejná funkce pro získání součtu všech plateb (třetích stran)
        {
            float f_sum = 0;
            foreach (Payment stuff in payments)
            {
                f_sum += stuff.Price;
            }
            return f_sum;
        }

        public Item get_sum_a_services()                    // veřejná funkce pro získání součtu všech poplatků další služby
        {
            Item i_sum = new Item();
            i_sum.price_tax = 0;
            i_sum.price_with_tax = 0;
            i_sum.price_witout_tax = 0;
            i_sum.tax = 0;

            foreach (AS_Item stuff in additional_services)
            {
                i_sum.price_tax += stuff.price_tax;
                i_sum.price_with_tax += stuff.price_with_tax;
                i_sum.price_witout_tax += stuff.price_witout_tax;
                i_sum.tax = stuff.tax;
            }
            return i_sum;
        }

        public string generate_csv_var1_line(string head_divizion_code, Dictionary<string, string> num_db) // veřejná funkce pro generování csv
        {
            string h_d_c = head_divizion_code;
            string divizion_code;
            string sub_name;
            string tariff_name;

            sub_name = num_db[info.Phone_number];           // jméno poplatníka
            if (tarifs.Count == 0) tariff_name = "-";
            else tariff_name = tarifs[0].Tarif_name;        // užívaný tarif


            divizion_code = sub_name.Split(' ')[0];
            bool parse_works = int.TryParse(divizion_code,out int num);
            if (!parse_works && tariff_name.Contains("Car Control")) divizion_code = "NCC";
            else if(!parse_works)divizion_code = head_divizion_code;
            h_d_c = divizion_code;
            if (sub_name == "-") sub_name = info.Phone_number;


            string str = h_d_c + ";";                       // kód hlavní divize nebo sub divize
            str += info.Phone_number + ";";                 // telefoní číslo poplatníka
            str += sub_name + ";";
            str += tariff_name + ";";
            str += info.Sum_price_wo_tax.ToString() + ";";  // celková cena bez daně
            str += get_total_data_units() + ";";            // zpoplatněná data v MB
            str += get_total_voice_cz() + ";";              // provolano v minutach v cz
            str += get_total_voice_noncz() + ";";           // provolano v minutach mimo cz
            str += count_sms_czeu() + ";";                  // Počet SMS v rámci cz a eu
            str += count_sms_cz_to_noneu() + ";";           // Počet SMS do světa
            str += count_sms_world() + ";";                 // Počet SMS ve světě
            str += count_mms() + ";";                       // Počet MMS

            bool empty = true;
            foreach (Timed_Item charge in one_time_charges) // poplatky za navýšení dat
            {
                if (charge.Fee_name.Contains("dat"))        // trošku hack, hledá poplatky obsahující řetězec "dat"
                {
                    str += charge.price_witout_tax;         // cena poplatku za navýšení
                    str += ";";
                    str += charge.Count;                    // počet navýšení
                    str += ";";
                    str += charge.Fee_name;                 // název poplatku
                    str += ";";
                    empty = false;
                    break;
                }
            }

            if (empty) str += "0;0;-;";

            if (tarifs.Count > 1 && tarifs[1].Tarif_type == "C")                           // kontroluje zda tento měsíc proběhla změna tarifu
            {
                if (tarifs[1].Tarif_name != tarifs[0].Tarif_name)
                    str += tarifs[1].Tarif_name;            // tarif na který bylo změněno
                else str += "-";
            } 
            else str += "-";

            return str;
        }

        public error read_subscriber_from_node(XmlNode sub_node) // hlavní veřejná funkce pro načtení jednoho zákazníka z xml souboru
        {

            string name;

            try
            {
                name = sub_node.Attributes["tariffSpaceName"].Value; // zjištění jména zákazníka, které ale nemusí existovat
            }
            catch (System.NullReferenceException)
            {
                name = "-";
            }

            info = new Subscriber_Info(sub_node.Attributes["customerAccountNumber"].Value,
                                       sub_node.Attributes["serviceNumber"].Value,
                                       sub_node.Attributes["tariffSpaceNumber"].Value,
                                       name,
                                       sub_node.Attributes["phoneNumber"].Value,
                                       str_to_float(sub_node.Attributes["summaryPriceTax"].Value),
                                       str_to_float(sub_node.Attributes["summaryPrice"].Value),
                                       str_to_float(sub_node.Attributes["summaryPriceWithTax"].Value));

            foreach(XmlNode node in sub_node.ChildNodes)
            switch (node.Name)
            {
                case "tariffs":
                    load_tarifs(node);
                    break;

                case "summaryData":
                    load_summary_data(node);
                    break;

                case "serviceTax":
                        load_service_taxes(node);
                        break;

                case "serviceTaxTS":
                        load_service_taxes(node);
                        break;

                default:
                    return error.SUCCESS;
            }

            return error.SUCCESS;
        }
    }
}
