using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;

namespace xmlParser
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    /// 
    /// Celý tento kód je velký bordel. Vpodstatě by to chtělo refaktor a přesunout většinu threadovací logiky buď do
    /// Xml_manipulator.cs a nebo vytvořit ještě jednu vrstvu abstrakce pro poskytnutí jednoduchého interface pro interakci,
    /// jak už s gui nebo bez něj.
    public partial class MainWindow : Window
    {
        private Xml_Manipulator xml_manage;
        private string[] filelders;                                         // files or folders
        private BackgroundWorker back_thread;
        private string last_folder = "";
        private int last_month = 0;
        private bool cbb_last_folder = false;

        public MainWindow()
        {
            InitializeComponent();
            xml_manage = new Xml_Manipulator();
            l_progress_text.Content = "---";
            cbb_last_folder = (bool)cb_from_last_inv.IsChecked;
            l_last_invoice.IsEnabled = false;
            l_info_lns.IsEnabled = false;

            try
            {
                using (var sr = new StreamReader("./lastFolder.xmlip", Encoding.UTF8)) // Pokusí se načíst název poslední složky ze souboru
                {
                    last_folder = sr.ReadLine();
                    try
                    {
                        last_month = int.Parse(last_folder.Split('\\').Last<string>().Split('-')[0]); // pokusí se z něj dostat údaj o období
                    }
                    catch (System.FormatException)
                    {
                        last_folder = "";
                    }
                    
                    l_last_invoice.Content = last_folder;
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                // do nothing
            }
            
        }

        private void bw_load_from_multi_dirs(object sender, DoWorkEventArgs e) // Funkce pozdeji delegovaná na thread
        {                                                                      // Slouží k nahrání souborů z několika podsložek
            int i = 0;
            foreach (string folder in filelders)
            {
                if (cbb_last_folder == true)
                {
                    int tmp = 0;
                    try
                    {
                        tmp = int.Parse(folder.Split('\\').Last<string>().Split('-')[0]);
                    }
                    catch (System.FormatException)
                    {
                        break;
                    }
                    
                    if (tmp <= last_month)
                    {
                        i++;
                        continue;
                    };
                }

                string[] files = Directory.GetFiles(folder);
                List<string> m_files = new List<string>(files);

                if (xml_manage.load_files(m_files) == error.FAIL)               // Samotná funkce pro načtení
                    System.Windows.MessageBox.Show("Něco se pokazilo při načítaání dat",
                                            "Načtení se nepovedlo", MessageBoxButton.OK,
                                            MessageBoxImage.Warning);

                i++;
                int progress = (int)((float)i / (float)filelders.Count<string>() * 100);
                last_folder = folder.Split('\\').Last<string>();
                last_month = int.Parse(last_folder.Split('-')[0]);
                back_thread.ReportProgress(progress);                          // report progressu pro progress bar
            }
        }

        private void change_ui_state(bool enable)                              // funkce pro hromadné zapnutí/vypnutí interakce s UI komponentama
        {
            btn_choose_xml.IsEnabled = enable;
            btn_delete_data.IsEnabled = enable;
            btn_load_data.IsEnabled = enable;
            cb_multi_month.IsEnabled = enable;
            if(cb_multi_month.IsChecked == true && enable == true)
                cb_from_last_inv.IsEnabled = enable;
        }

        private void bw_load_from_one_dir(object sender, DoWorkEventArgs e)     // funkce pozdeji delegovana na thread
        {                                                                       // funkce k načtení faktur z jedné složky
            List<string> m_files = new List<string>(filelders);

            if (xml_manage.load_files(m_files) == error.FAIL)
                System.Windows.MessageBox.Show("Něco se pokazilo při načítaání dat",
                                        "Načtení se nepovedlo", MessageBoxButton.OK,
                                        MessageBoxImage.Warning);

            back_thread.ReportProgress(100);
        }

        private void bw_work_completed(object sender, RunWorkerCompletedEventArgs e) // ošetření události pro dokončení práce threadu
        {
            if (xml_manage.Customers_count != 0)
            {
                System.Windows.MessageBox.Show("Data úspěšně nahrána",
                                           "Hotovo",
                                           MessageBoxButton.OK,
                                           MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("Napovedlo se najít žádné validní data ke zpracování.",
                                           "Žádné data",
                                           MessageBoxButton.OK,
                                           MessageBoxImage.Warning);
            }
            
            l_subs_count.Content = xml_manage.Customers_count;
            pb_progress.Value = 0;
            l_last_invoice.Content = last_folder;
            l_progress_text.Content = "Hotovo!";

            using (var sw = new StreamWriter("./lastFolder.xmlip", false, Encoding.UTF8))
            {
                sw.WriteLine(last_folder);
            }

            change_ui_state(true);
        }

        private void bw_progress_changed(object sender, ProgressChangedEventArgs e) // ošetření události pro změny stavu nahrávání na vedlejším threadu
        {
            pb_progress.Value = e.ProgressPercentage;
        }

        private void Btn_choose_xml_Click(object sender, RoutedEventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();


                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (cb_multi_month.IsChecked == true)
                    {
                        filelders = Directory.GetDirectories(fbd.SelectedPath);
                        back_thread = new BackgroundWorker();
                        back_thread.DoWork += bw_load_from_multi_dirs;
                        back_thread.RunWorkerCompleted += bw_work_completed;
                        back_thread.ProgressChanged += bw_progress_changed;
                        back_thread.WorkerReportsProgress = true;
                        back_thread.RunWorkerAsync();
                        change_ui_state(false);
                        l_progress_text.Content = "Pracuji...";
                    }
                    else
                    {
                        filelders = Directory.GetFiles(fbd.SelectedPath);
                        last_folder = fbd.SelectedPath.Split('\\').Last<string>();
                        try
                        {
                            last_month = int.Parse(last_folder.Split('-')[0]);
                        }
                        catch (System.FormatException) { }
                        back_thread= new BackgroundWorker();
                        back_thread.DoWork += bw_load_from_one_dir;
                        back_thread.RunWorkerCompleted += bw_work_completed;
                        back_thread.ProgressChanged += bw_progress_changed;
                        back_thread.WorkerReportsProgress = true;
                        back_thread.RunWorkerAsync();
                        change_ui_state(false);
                        l_progress_text.Content = "Pracuji...";

                    }
                }
            }            
            
        }

        private void Btn_load_data_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog svDialog = new SaveFileDialog();
            svDialog.Filter = "soubory csv (*.csv)| *.csv";

            if (svDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                error err = xml_manage.save_to_csv_var1(svDialog.FileName);
                switch (err)
                {
                    case error.SUCCESS:
                        l_file_path.Content = svDialog.FileName;
                        System.Windows.MessageBox.Show("Soubor uložen úspěšně.",
                                                       "Povedlo se",
                                                       MessageBoxButton.OK,
                                                       MessageBoxImage.Information);
                        break;

                    case error.FILE_OPEN:
                        System.Windows.MessageBox.Show("Soubor je nejspíše otevřen v jiném programu.", 
                                                       "Nepovedlo se otevřít soubor",
                                                       MessageBoxButton.OK,
                                                       MessageBoxImage.Warning);
                        break;

                    case error.NO_DATA:
                        System.Windows.MessageBox.Show("V programu nejsou nahrána žádná data pro zapsání.",
                                                       "Chybí data",
                                                       MessageBoxButton.OK,
                                                       MessageBoxImage.Warning);
                        break;

                    default:
                        System.Windows.MessageBox.Show("Bum bác tohle by se stát nemělo.",
                                                        "Něco se pokazilo",
                                                        MessageBoxButton.OK,
                                                        MessageBoxImage.Warning);
                        break;
                }
            } 

        }

        private void Btn_delete_data_Click(object sender, RoutedEventArgs e)
        {
            xml_manage.delete_all();
            System.GC.Collect();
            l_file_path.Content = "";
            l_subs_count.Content = xml_manage.Customers_count;
        }

        private void Cb_multi_month_Click(object sender, RoutedEventArgs e)
        {
            cb_from_last_inv.IsEnabled = (bool)cb_multi_month.IsChecked;
            if (cb_from_last_inv.IsChecked == true)
            {
                l_last_invoice.IsEnabled = (bool)cb_multi_month.IsChecked;
                l_info_lns.IsEnabled = (bool)cb_multi_month.IsChecked;
            }
        }

        private void Cb_from_last_inv_Click(object sender, RoutedEventArgs e)
        {
            cbb_last_folder = (bool)cb_from_last_inv.IsChecked;
            l_last_invoice.IsEnabled = (bool)cb_from_last_inv.IsChecked;
            l_info_lns.IsEnabled = (bool)cb_from_last_inv.IsChecked;
        }
    }
}
