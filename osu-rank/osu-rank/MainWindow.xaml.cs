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
using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using osu_rank.Properties;
using System.Net.Http;
using System.IO;

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

        bool hasApiKey = false;
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
            goRippleOnePlayer.FontWeight = FontWeights.Regular;
            goRippleComparator.FontWeight = FontWeights.Regular;
            goSettings.FontWeight = FontWeights.Regular;
            goAbout.FontWeight = FontWeights.Regular;
        }

        
        // Navigation code
        private void goOneUser_Click(object sender, MouseButtonEventArgs e)
        {
            drawerUnbold();
            Label lbl = sender as Label;
            lbl.FontWeight = FontWeights.SemiBold;
            actionBar_Text.Content = "osu!rank - " + Tx.T("osu rank.One player");
            WindowContent.Navigate(new osuPages.OneUser());
            closeDrawer();
        }

        private void goCompare_Click(object sender, MouseButtonEventArgs e)
        {
            drawerUnbold();
            Label lbl = sender as Label;
            lbl.FontWeight = FontWeights.SemiBold;
            actionBar_Text.Content = "osu!rank - " + Tx.T("osu rank.Compare");            
            WindowContent.Navigate(new osuPages.Compare());
            closeDrawer();
        }
        private void goSettings_Click(object sender, MouseButtonEventArgs e)
        {
            drawerUnbold();
            Label lbl = sender as Label;
            lbl.FontWeight = FontWeights.SemiBold;
            actionBar_Text.Content = "osu!rank - " + Tx.T("osu rank.Settings");
            WindowContent.Navigate(new Options());
            closeDrawer();
        }

        private async void apiDialog_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            if ((Int16)eventArgs.Parameter == 1)
            {
                if (keyBox.Text == "")
                {
                    MessageBox.Show(Tx.T("errors.No key entered"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    eventArgs.Cancel();
                }
                else
                {
                    string test;
                    try
                    {
                        using (HttpClient wc = new HttpClient())
                        {
                            test = await wc.GetStringAsync("https://osu.ppy.sh/api/get_user?k=" + keyBox.Text + "&u=Cookiezi&m=3");
                            Settings.Default.apikey = keyBox.Text;
                        }
                        hasApiKey = true;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(Tx.T("errors.Invalid key"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        eventArgs.Cancel();
                    }
                }
            }
            else if ((Int16)eventArgs.Parameter == 0)
            {
                MessageBox.Show(Tx.T("errors.You need a key"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            else if ((Int16)eventArgs.Parameter == -1)
            {
                osuExpander.IsEnabled = false;
                hasApiKey = false;
                Settings.Default.RippleOnly = true;
                Settings.Default.Save();
            }
        }
        string osuRankAppdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\osu-rank";
        private async Task checkForDictUpdate()
        {

            if (!Directory.Exists(osuRankAppdata))
            {
                Directory.CreateDirectory(osuRankAppdata);
            }
            if (!File.Exists(osuRankAppdata + @"\txdVersion.txt"))
            {
                await WebUtils.DownloadAsync(@"https://raw.githubusercontent.com/otaku-overclocks/osu-rank/master/osu-rank/osu-rank/Translation/txdVersion.txt", osuRankAppdata + @"\txdVersion.txt");
                await WebUtils.DownloadAsync(@"https://raw.githubusercontent.com/otaku-overclocks/osu-rank/master/osu-rank/osu-rank/Translation/osu_rank.txd", osuRankAppdata + @"\osu_rank.txd");

            }
            else
            {
                int onlineTxdVersion = Convert.ToInt32(await new HttpClient().GetStringAsync(@"https://raw.githubusercontent.com/otaku-overclocks/osu-rank/master/osu-rank/osu-rank/Translation/txdVersion.txt"));
                int localTxdVersion = Convert.ToInt32(File.ReadAllText(osuRankAppdata + @"\txdVersion.txt"));
                if (onlineTxdVersion <= localTxdVersion)
                {
                    await WebUtils.DownloadAsync(@"https://raw.githubusercontent.com/otaku-overclocks/osu-rank/master/osu-rank/osu-rank/Translation/txdVersion.txt", osuRankAppdata + @"\txdVersion.txt");
                    await WebUtils.DownloadAsync(@"https://raw.githubusercontent.com/otaku-overclocks/osu-rank/master/osu-rank/osu-rank/Translation/osu_rank.txd", osuRankAppdata + @"\osu_rank.txd");
                }
            }
        }

        private async void windowLoaded(object sender, RoutedEventArgs e)
        {
            Tx.LoadFromEmbeddedResource("osu_rank.Translation.osu_rank.txd");
            if (Settings.Default.apikey == "" && Settings.Default.RippleOnly == false)
            {
                osuExpander.IsEnabled = true;
                hasApiKey = false;
                apiDialog.IsOpen = true;
            }
            else if (Settings.Default.RippleOnly == true)
            {
                osuExpander.IsEnabled = false;
                hasApiKey = false;
                drawerUnbold();
                goRippleOnePlayer.FontWeight = FontWeights.SemiBold;
                actionBar_Text.Content = "Ripple!rank - " + Tx.T("osu rank.One player");
                WindowContent.Navigate(new RipplePages.OneUser());
            }
            else if (Settings.Default.apikey != "" && Settings.Default.RippleOnly == false)
            {
                osuExpander.IsEnabled = true;
                hasApiKey = true;
            }
            await checkForDictUpdate();
            if (Convert.ToInt32(File.ReadAllText(osuRankAppdata + @"\txdVersion.txt"))>App.minTxdVersionRequired)
                Tx.LoadFromXmlFile(osuRankAppdata + @"\osu_rank.txd");
            if (App.HasCheckedForUpdates == false)
            {
                App.HasCheckedForUpdates = true;
                try
                {
                    string webVersion = await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Jeremiidesu/osu-rank/master/osu-rank/osu-rank/version.txt");
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
        }

        private void keyPrompt_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://osu.ppy.sh/p/api");
        }

        private void goAbout_MouseDown(object sender, MouseButtonEventArgs e)
        {
            drawerUnbold();
            Label lbl = sender as Label;
            lbl.FontWeight = FontWeights.SemiBold;
            actionBar_Text.Content = "osu!rank - " + Tx.T("osu rank.About");
            WindowContent.Navigate(new About());
            closeDrawer();
        }

        private void goRippleOneUser_Click(object sender, MouseButtonEventArgs e)
        {
            drawerUnbold();
            Label lbl = sender as Label;
            lbl.FontWeight = FontWeights.SemiBold;
            actionBar_Text.Content = "Ripple!rank - " + Tx.T("osu rank.One player");
            WindowContent.Navigate(new RipplePages.OneUser());
            closeDrawer();
        }

        private void goRippleCompare_Click(object sender, MouseButtonEventArgs e)
        {
            drawerUnbold();
            Label lbl = sender as Label;
            lbl.FontWeight = FontWeights.SemiBold;
            actionBar_Text.Content = "Ripple!rank - " + Tx.T("osu rank.Compare");
            WindowContent.Navigate(new RipplePages.Compare());
            closeDrawer();
        }

        private void osuExpander_mouseUp(object sender, MouseButtonEventArgs e)
        {
            if (hasApiKey==false)
            {
                apiDialog.IsOpen = true;
            }
        }
    }
}
