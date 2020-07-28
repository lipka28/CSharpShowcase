using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace xmlParser
{
    /// <summary>
    /// Interakční logika pro App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string in_folder = "\"\"";
        private string out_csv = "\"\"";
        private int data_limit = 0;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow gui = new MainWindow();
            if (e.Args.Length == 1)
            {
                gui.Close();
                if (argument_is_valid(e.Args[0]))
                {

                    try
                    {
                        using (var sr = new StreamReader("./config.cfg", Encoding.UTF8))
                        {
                            Console.WriteLine("Soubor config.cfg načten.");
                            parse_config(sr);
                        }

                        start_headless();
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        string anwser = "";
                        Console.Write("Soubor config.cfg nenalezen!\n"
                            + "Chcete jej vytvořit? (Y/n):");
                        anwser = Console.ReadLine();
                        if (anwser.ToLower() == "n")
                        {
                            Console.WriteLine("Automatické vytváření configuračního souboru zrušeno!\n\n");
                            explain();
                        }
                        else
                        {
                            generate_config();
                        }
                    }
                }

            }
            else if (e.Args.Length == 0)
            {
                Console.WriteLine("Aplikace se spouští s GUI");
                IntPtr hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                ShowWindow(hWnd, 0);
                gui.Show();
            }
            else
            {
                gui.Close();
                Console.WriteLine("Špatný počet argumentů");
            }

        }

        private error start_headless()
        {
            Xml_Manipulator xml_manage = new Xml_Manipulator();
            string[] folders;

            try
            {
                folders = Directory.GetDirectories(in_folder);
            }
            catch (Exception)
            {
               Console.WriteLine("Nepovedlo se načíst soubory\n"
                   + "Špatně nastavený parametr IN_FOLDER v konfiguračním souboru");
               return error.FAIL;
            }

            int count = folders.Length;
            int i = 1;
            int start;
            if (data_limit >= count || data_limit <= 0)
            {
                start = 0;
            }
            else
            {
                start = count - data_limit;
            }


            Console.WriteLine("Začátek nahrávání dat.");
            foreach (var folder in folders)
            {
                if (i - 1 >= start)
                {
                    List<string> files = new List<string>(Directory.GetFiles(folder));
                    Console.WriteLine("Zpracovávání složky {0} / {1}:", i, count);
                    if (xml_manage.load_files(files) == error.FAIL) { Console.WriteLine("Načtení xml souborů se nepovedlo!"); }
                    else Console.WriteLine("Ok.\n");
                }
                
                i++;
            }

            Console.WriteLine("Nahrávání dat dokončeno.");
            Console.WriteLine("Ukládání dat do {0}.", out_csv);
            if (!out_csv.Contains(".csv") && out_csv.Length < 5)
            {
                Console.WriteLine("Soubor {0} není validním výstupním souborem, soubor musí mít koncovku .csv", out_csv);
                return error.FAIL;
            }
            error err = xml_manage.save_to_csv_var1(out_csv);
            switch (err)
            {
                case error.SUCCESS:
                    Console.WriteLine("Data úspěšně zapsána do {0}.", out_csv);
                    break;

                case error.FILE_OPEN:
                    Console.WriteLine("Data se nepovedlo zapsat, soubor je nejspíše otevřen v jiném programu!");
                    break;

                case error.NO_DATA:
                    Console.WriteLine("Chybí data pro zapsání do csv!");
                    break;

                default:
                    Console.WriteLine("Neočekávaná chyba!");
                    break;
            }

            Console.WriteLine("\nHotovo!\n");
            return error.SUCCESS;

        }

        private bool argument_is_valid(string arg)
        {
            if (arg == "-c" || arg == "--console") return true;
            else
            {
                Console.WriteLine("Špatný argmunet!\n\n"
                    + "Validnímy argumenty jsou:\n"
                    + "-c | --console (Pro zpuštění aplikace bez grafického rozhraní)\n");
                return false;
            }
        }

        private void explain()
        {
            Console.WriteLine("Pro správné fungování programu vytvořte nebo upravte soubor config.cfg. "
                + "Obsahující minimálně tyto parametry:\n");
            Console.WriteLine("IN_FOLDER = \"Cesta\\ke\\složce\"\n"
                + "OUT_CSV = \"Cesta\\k\\uložení\\dat.csv\"\n\n");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Možné nastavení:\n");
            Console.WriteLine("IN_FOLDER - Cesta ke složce obsahující podsložky s xml fakturamy za jednotlivá období");
            Console.WriteLine("OUT_CSV   - Cesta k csv souboru (nemusí existovat) do kterého mají být data uložena");
            Console.WriteLine("DATA_LIMIT - Maximální počet obdoní která se mají zpracovat (od nejaktuálnějšího) [0 = neomezené]\n");
        }

        private void generate_config()
        {
            using (var sw = new StreamWriter("config.cfg"))
            {
                sw.WriteLine("################################################################");
                sw.WriteLine("# Konfigurační soubor pro konzolové využítí xmlInvoice parseru #");
                sw.WriteLine("################################################################");
                sw.WriteLine("");
                sw.WriteLine("# IN_FOLDER - Cesta ke složce obsahující podsložky s xml fakturamy za jednotlivá období");
                sw.WriteLine("IN_FOLDER = \"\"");
                sw.WriteLine("");
                sw.WriteLine("# OUT_CSV - Cesta k csv souboru (nemusí existovat) do kterého mají být data uložena");
                sw.WriteLine("OUT_CSV = \"data.csv\"");
                sw.WriteLine("");
                sw.WriteLine("# DATA_LIMIT - Maximální počet obdoní která se mají zpracovat (od nejaktuálnějšího) [0 = neomezené]");
                sw.WriteLine("DATA_LIMIT = \"12\"");

                Console.WriteLine("Soubor config.cfg úspěšně vytvořen.\n"
                    +"Nyní v něm upravte potřebná nastavení a spusťte program znovu.");
            }            
        }

        private void parse_config(StreamReader sr)
        {
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("#")) continue;
                else if (line.Contains("IN_FOLDER"))
                {
                    try
                    {
                        in_folder = line.Split('\"')[1];
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Cesta musí být zadána v uvozovkách!\n");
                        in_folder = "";
                    }
                }
                else if (line.Contains("OUT_CSV"))
                {
                    try
                    {
                        out_csv = line.Split('\"')[1];
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Cesta musí být zadána v uvozovkách!\n");
                        out_csv = "";
                    }
                }
                else if (line.Contains("DATA_LIMIT"))
                {
                    try
                    {
                        data_limit = int.Parse(line.Split('\"')[1]);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("DATA_LIMIT není validní číslo, nebo není zapsáno v uvozovkách! Ignoruji.\n");
                        data_limit = 0;
                    }
                }
                else continue;
            }

        }
    }
}
