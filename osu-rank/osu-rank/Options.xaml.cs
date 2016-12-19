using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using System.Globalization;
using Unclassified.TxLib;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Net;
using osu_rank.Properties;
using System.IO;

namespace osurank
{
    /// <summary>
    /// Logique d'interaction pour Options.xaml
    /// </summary>
    public partial class Options : Page
    {
        public Options()
        {

            InitializeComponent();
        }

        
        private void delaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AutorefreshStatus.Content = Tx.T("options.autorefresh.Delay", Convert.ToInt32(delay.Value));
        }

        private void autoRefresh_Enabled(object sender, RoutedEventArgs e)
        {
            AutorefreshStatus.Content = Tx.T("options.autorefresh.Delay", Convert.ToInt32(delay.Value));
            delay.IsEnabled = true;
        }

        private void autoRefresh_Disabled(object sender, RoutedEventArgs e)
        {
            AutorefreshStatus.Content = Tx.T("options.state.Disabled");
            delay.IsEnabled = false;
        }
        private void LanguageDropdown_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (CultureInfo culture in Tx.AvailableCultures)
            {
                LanguageDropdown.Items.Add(culture.NativeName);
            }
        }
        private void loadSettings()
        {
            #region language
            int languageIndex = 0;
            foreach (string culture in Tx.AvailableCultureNames)
            {
                languageIndex += 1;
                if (culture == Settings.Default.LanguageCode)
                {
                    LanguageDropdown.SelectedIndex = languageIndex;
                    break;
                }
            }
            #endregion
            AutorefreshEnable.IsChecked = Settings.Default.RefreshEnable;
            if (Settings.Default.RefreshEnable == false)
                AutorefreshStatus.Content = Tx.T("options.state.Disabled");
            else
                delay.Value = Settings.Default.RefreshDelay;
            p1Input.Text = Settings.Default.DefaultPlayer;
            p2Input.Text = Settings.Default.DefaultPlayer2;
            gamemodeDropdown.SelectedIndex = Settings.Default.DefaultGamemode;
            startupCheck.IsChecked = Settings.Default.StartupCheck;
        }
        private void saveSettings()
        {
            if (LanguageDropdown.SelectedIndex == 0)
            {
                Settings.Default.LanguageCode = "";
            }
            else {
                Settings.Default.LanguageCode = Tx.AvailableCultures[LanguageDropdown.SelectedIndex - 1].Name;
            }
            Settings.Default.RefreshEnable = (bool)AutorefreshEnable.IsChecked;            
            Settings.Default.RefreshDelay = (int)delay.Value;
            Settings.Default.DefaultPlayer = p1Input.Text;
            Settings.Default.DefaultPlayer2 = p2Input.Text;
            Settings.Default.DefaultGamemode = gamemodeDropdown.SelectedIndex;
            Settings.Default.StartupCheck = (bool)startupCheck.IsChecked;
            Settings.Default.Save();
        }
        string osuRankAppdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\osu-rank";
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            saveSettings();
            #region language
            if (Settings.Default.LanguageCode != "")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.LanguageCode);
            }
            else if (Settings.Default.LanguageCode == "")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = App.systemCulture;
            }

            if (Convert.ToInt32(File.ReadAllText(osuRankAppdata + @"\txdVersion.txt")) > App.minTxdVersionRequired)
                Tx.LoadFromXmlFile(osuRankAppdata + @"\osu_rank.txd");
            else Tx.LoadFromEmbeddedResource("osu_rank.Translation.osu_rank.txd");

            #endregion
            if (Settings.Default.RefreshEnable == false)
                AutorefreshStatus.Content = Tx.T("options.state.Disabled");
            else
                AutorefreshStatus.Content = Tx.T("options.autorefresh.Delay", Convert.ToInt32(delay.Value));
            Storyboard animation = TryFindResource("settingsSaved") as Storyboard;
            animation.Begin();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            OKButton.IsEnabled = false;
            testKey.IsEnabled = true;
            keyBox.Text = Settings.Default.apikey;
            loadSettings();
        }

        private void page_loaded(object sender, RoutedEventArgs e)
        {
            loadSettings();
        }

        private void keyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (txtBox.Text != Settings.Default.apikey)
            {
                OKButton.IsEnabled = false;
                testKey.IsEnabled = true;
            }
        }

        private void testKey_Click(object sender, RoutedEventArgs e)
        {
            if (keyBox.Text == "")
            {
                MessageBox.Show(Tx.T("errors.No key entered"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                string test;
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        test = wc.DownloadString("https://osu.ppy.sh/api/get_user?k=" + keyBox.Text + "&u=Cookiezi&m=3");
                        Settings.Default.apikey = keyBox.Text;
                        OKButton.IsEnabled = true;
                        testKey.IsEnabled = false;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show(Tx.T("errors.Invalid key"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
