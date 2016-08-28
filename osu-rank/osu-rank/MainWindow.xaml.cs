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
using osu_rank.Properties;
using System.Diagnostics;

namespace osu_rank
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
            if (Settings.Default.apikey=="")
            {
                apiDialog.IsOpen = true;
            }
        }

        private void keyPrompt_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://osu.ppy.sh/p/api");
        }
    }
}
