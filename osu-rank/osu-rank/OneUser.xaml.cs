using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unclassified.TxLib;
using osurank.Properties;
using System.Windows.Threading;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace osurank                                                                                                                                       
{
    /// <summary>                                                                                                                                        
    /// Logique d'interaction pour MainWindow.xaml                                                                                                       
    /// </summary>                                                                                                                                       
    public partial class OneUser : Page                                                                                                                
    {
        // ----- START VARIABLES -----
        DispatcherTimer autoRefresh = new DispatcherTimer();
        NumberFormatInfo NFInoDecimal = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        NumberFormatInfo NFIperformance = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        string username = Settings.Default.DefaultPlayer;
        int gamemode = Settings.Default.DefaultGamemode;
        dynamic userdata;
        // variables for variation labels
        string previousUsername;
        dynamic previousRefresh;
        // ------ END VARIABLES ------

        private void autoRefresh_Tick(object sender, EventArgs e)
        {
            fetchUserData(username, gamemode, false);
        }
        
        public OneUser()
        {
            InitializeComponent();
        }
        
        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            timerReset();
            username = nameInput.Text;
            gamemode = gamemodeDropdown.SelectedIndex;
            fetchUserData(nameInput.Text, gamemodeDropdown.SelectedIndex);
        }
        private void fetchUserData(string player_name, int gamemode_index, bool show_errors = true)
        {
            bool samename = false;
            if (player_name==previousUsername)
            {
                previousRefresh = userdata;
                samename = true;
            }
            userdata = osu.GetUser(player: player_name, gamemode: gamemode_index, apikey: Settings.Default.apikey, showErrors:show_errors)[0];
            previousUsername = player_name;
            // do nothing if player did not play or player is invalid
            if (Convert.ToString(userdata) == Convert.ToString(int.MaxValue) ) { return; }
            if (userdata.pp_rank == null) {return;}
            if (userdata == null) { return; }

            playername.Content = Convert.ToString(userdata.username);
            Avatar.Source = new ImageSourceConverter().ConvertFromString("http://a.ppy.sh/" + userdata.user_id) as ImageSource;
            globalrank.Content = "#" + userdata.pp_rank;
            countryRank.Content = "#" + userdata.pp_country_rank + " (" + userdata.country + ")";

            rankedScore.Content = Tx.TC("player.Ranked score") + " " + Convert.ToInt64(userdata.ranked_score).ToString("n", NFInoDecimal);
            playCount.Content = Tx.TC("player.Play count") + " " + Convert.ToInt32(userdata.playcount).ToString("n", NFInoDecimal);
            acc.Content = Math.Round(Convert.ToDouble(userdata.accuracy), 2) + "%";
            pp.Content = Convert.ToDouble(userdata.pp_raw).ToString("n", NFIperformance) + " pp";
            totalScore.Content = Tx.TC("player.Total score") + " " + Convert.ToInt64(userdata.total_score).ToString("n", NFInoDecimal);
            level.Content = Tx.TC("player.Level") + " " + Convert.ToDouble(userdata.level).ToString("n", NFInoDecimal) + " (" + ((Convert.ToDouble(userdata.level) - Math.Truncate(Convert.ToDouble(userdata.level))) * 100).ToString("n", NFInoDecimal) + "%)";
            /* grades.Content = "SS / S / A: " + Convert.ToInt32(userdata.count_rank_ss).ToString("n", NFInoDecimal) + " / "
                                                + Convert.ToInt32(userdata.count_rank_s).ToString("n", NFInoDecimal) + " / "
                                                + Convert.ToInt32(userdata.count_rank_a).ToString("n", NFInoDecimal); */
            SS.Content = Convert.ToInt32(userdata.count_rank_ss).ToString("n", NFInoDecimal);
            S.Content = Convert.ToInt32(userdata.count_rank_s).ToString("n", NFInoDecimal);
            A.Content = Convert.ToInt32(userdata.count_rank_a).ToString("n", NFInoDecimal);
            userID.Content = Tx.TC("player.User ID") + " " + userdata.user_id;
            levelProgress.Value = ((Convert.ToDouble(userdata.level) - Math.Truncate(Convert.ToDouble(userdata.level))) * 100);
            if (samename==true)
            {
                updateVariation(previousRefresh,userdata);
            }
            else
            {
                //insert reset code here
            }
        }

        private void timerReset()
        {
            autoRefresh.Stop();
            autoRefresh.Start();
        }

        private void updateVariation(dynamic before, dynamic after)
        {
            // so many needed variables
            long RscoreDiff, Scorediff;
            int PCdiff,rankdiff,countryrankdiff,Sdiff,SSdiff,Adiff;
            double ppDiff, accDiff;

            // difference calculation 
            RscoreDiff = (long)after.ranked_score - (long)before.ranked_score;
            Scorediff = (long)after.total_score - (long)before.total_score;
            PCdiff = (int)after.playcount - (int)before.playcount;
            rankdiff = (int)before.pp_rank - (int)after.pp_rank;
            countryrankdiff = (int)before.pp_country_rank - (int)after.pp_country_rank;
            Sdiff = (int)after.count_rank_s - (int)before.count_rank_s;
            SSdiff = (int)after.count_rank_ss - (int)before.count_rank_ss;
            Adiff = (int)after.count_rank_a - (int)before.count_rank_a;
            ppDiff = (double)after.pp_raw - (double)before.pp_raw;
            accDiff = (double)after.accuracy - (double)before.accuracy;

            // display these values
            if (RscoreDiff > 0)
            {
                rankedScore_diff.Content = "+" + RscoreDiff.ToString("n", NFInoDecimal);
            }
        }

        private void page_loaded(object sender, RoutedEventArgs e)
        {
            
            RenderOptions.SetBitmapScalingMode(Avatar, BitmapScalingMode.HighQuality);
            Settings.Default.LastWindow = "OneUser";

            // NumberFormatInfo for Ranked/total score and play count
            NFInoDecimal.NumberGroupSeparator = ",";
            NFInoDecimal.NumberDecimalSeparator = ".";
            NFInoDecimal.NumberDecimalDigits = 0;
            // NumberFormatInfo for PPs
            NFIperformance.NumberGroupSeparator = ",";
            NFIperformance.NumberDecimalSeparator = ".";

            // Timer
            autoRefresh.Tick += new EventHandler(autoRefresh_Tick);
            autoRefresh.Interval = new TimeSpan(0, Settings.Default.RefreshDelay, 0);

            nameInput.Text = Settings.Default.DefaultPlayer;
            gamemodeDropdown.SelectedIndex = Settings.Default.DefaultGamemode;
            if (Settings.Default.StartupCheck == true)
            {
                fetchUserData(username, gamemode);
                if (Settings.Default.RefreshEnable == true)
                {
                    autoRefresh.IsEnabled = true;
                }
                else
                    autoRefresh.IsEnabled = false;
            }
        }
    }
}
                                                                                                                                                         