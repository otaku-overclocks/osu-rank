using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unclassified.TxLib;
using osurank.Properties;
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;

namespace osurank
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool drawerOpened = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        // Navigation drawer
        private void menuMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (drawerOpened==false)
            {
                Storyboard animation = TryFindResource("MenuOpen") as Storyboard;
                animation.Begin();
                Storyboard openDrawer = TryFindResource("DrawerOpen") as Storyboard;
                openDrawer.Begin();
                drawerOpened = true;
            }
            else
            {
                closeDrawer();
            }
        }
        private void closeDrawer()
        {
            Storyboard animation = TryFindResource("MenuClose") as Storyboard;
            animation.Begin();
            Storyboard closeDrawer = TryFindResource("DrawerClose") as Storyboard;
            closeDrawer.Begin();
            drawerOpened = false;
        }
        private void drawerClose(object sender, MouseButtonEventArgs e)
        {
            closeDrawer();
        }

        // Needed for navigation
        private void drawerUnbold()
        {
            goOnePlayer.FontWeight = FontWeights.Regular;
            goComparator.FontWeight = FontWeights.Regular;
            goSettings.FontWeight = FontWeights.Regular;
        }

        
        // Navigation code
        private void goOneUser_Click(object sender, MouseButtonEventArgs e)
        {
            drawerUnbold();
            Label lbl = sender as Label;
            lbl.FontWeight = FontWeights.Bold;
            actionBar_Text.Content = "osu!rank - " + Tx.T("osu rank.One player");
            WindowContent.Navigate(new OneUser());
            closeDrawer();
        }

        private void goCompare_Click(object sender, MouseButtonEventArgs e)
        {
            drawerUnbold();
            Label lbl = sender as Label;
            lbl.FontWeight = FontWeights.Bold;
            actionBar_Text.Content = "osu!rank - " + Tx.T("osu rank.Compare");            
            WindowContent.Navigate(new Compare());
            closeDrawer();
        }
        private void goSettings_Click(object sender, MouseButtonEventArgs e)
        {
            drawerUnbold();
            Label lbl = sender as Label;
            lbl.FontWeight = FontWeights.Bold;
            actionBar_Text.Content = "osu!rank - " + Tx.T("osu rank.Settings");
            WindowContent.Navigate(new Options());
            closeDrawer();
        }

        private void apiDialog_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if ((bool)eventArgs.Parameter==true)
            {
                if (keyBox.Text == "")
                {
                    MessageBox.Show(Tx.T("errors.No key entered"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    eventArgs.Cancel();
                }
                else
                {
                    string test;
                    try {
                        using (WebClient wc = new WebClient())
                        {
                            test = wc.DownloadString("https://osu.ppy.sh/api/get_user?k=" + keyBox.Text + "&u=Cookiezi&m=3");
                            Settings.Default.apikey = keyBox.Text;
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(Tx.T("errors.Invalid key"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        eventArgs.Cancel();
                    }
                }
            }
            else
            {
                MessageBox.Show(Tx.T("errors.You need a key"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            if (App.HasCheckedForUpdates == false)
            {
                App.HasCheckedForUpdates = true;
                try
                {
                    string webVersion = new System.Net.WebClient().DownloadString("https://raw.githubusercontent.com/Jeremiidesu/osu-rank/master/osu-rank/osu-rank/version.txt");
                    if (Convert.ToInt32(webVersion) > App.version)
                    { // New release available
                        var updateYesNo = MessageBox.Show(Tx.T("update.text"), Tx.T("update.Title"), MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (updateYesNo == MessageBoxResult.Yes)
                        {
                            Process.Start("https://github.com/Jeremiidesu/osu-rank/releases");
                        }
                    }
                }
                catch (Exception) { }
            }
            Ping myPing = new Ping();
            string host = "osu.ppy.sh";
            byte[] buffer = new byte[32];
            int timeout = 5000;
            PingOptions pingOptions = new PingOptions();
            PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
            if (reply.Status != IPStatus.Success)
            {
                MessageBox.Show(Tx.T("osu rank.Servers unavailable"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
            if (Settings.Default.apikey=="")
            {
                apiDialog.IsOpen = true;
            }
            #region language
            if (Settings.Default.LanguageCode != "")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.LanguageCode);
            }
            else if (Settings.Default.LanguageCode == "")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = App.systemCulture;
            }
            string[] resourceNames = this.GetType().Assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                Console.WriteLine(resourceName);
            }
            Tx.LoadFromEmbeddedResource("osu_rank.osu_rank.txd");
            #endregion
        }

        private void keyPrompt_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://osu.ppy.sh/p/api");
        }
    }
}
