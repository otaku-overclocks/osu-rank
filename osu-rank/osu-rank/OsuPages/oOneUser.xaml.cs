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
using System.Windows.Threading;
using System.Diagnostics;
using System.Net.NetworkInformation;
using osu_rank.Properties;

namespace osurank.osuPages                                                                                                                                       
{
    /// <summary>                                                                                                                                        
    /// Logique d'interaction pour MainWindow.xaml                                                                                                       
    /// </summary>                                                                                                                                       
    public partial class OneUser : Page                                                                                                                
    {
        #region variables
        DispatcherTimer autoRefresh = new DispatcherTimer();
        DispatcherTimer lastUpdatedRefresher = new DispatcherTimer();
        TimeSpan lastRefreshedAgo = new TimeSpan(0, 0, 0);
        NumberFormatInfo NFInoDecimal = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        NumberFormatInfo NFIperformance = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        string username = Settings.Default.DefaultPlayer;
        int gamemode = Settings.Default.DefaultGamemode;
        dynamic userdata;
        // variables for variation labels
        string previousUsername;
        dynamic previousRefresh;
        int previousGamemode;
        #endregion

        private async void autoRefresh_Tick(object sender, EventArgs e)
        {
            await fetchUserData(username, gamemode, false);
        }
        
        public OneUser()
        {
            InitializeComponent();
        }
        
        private async void searchButton_Click(object sender, RoutedEventArgs e)
        {
            timerReset();
            username = nameInput.Text;
            gamemode = gamemodeDropdown.SelectedIndex;
            await fetchUserData(nameInput.Text, gamemodeDropdown.SelectedIndex);
        }
        private async Task fetchUserData(string player_name, int gamemode_index, bool show_errors = true)
        {
            bool samename = false;
            bool samemode = false;
            if (player_name == previousUsername) samename = true;
            if (gamemode_index == previousGamemode) samemode = true;
            if (samemode==true && samename==true) previousRefresh = userdata;
            dynamic rawreturns = await osu.GetUserAsync(player: player_name, gamemode: gamemode_index, apikey: Settings.Default.apikey, showErrors: show_errors);
            userdata = rawreturns[0];
            previousUsername = player_name;
            previousGamemode = gamemode_index;
            // do nothing if player did not play or player is invalid
            if (Convert.ToString(userdata) == Convert.ToString(int.MaxValue) ) { return; }
            if (userdata.pp_rank == null) {return;}
            if (userdata == null) { return; }

            playername.Content = Convert.ToString(userdata.username);
            Avatar.Source = new ImageSourceConverter().ConvertFromString("http://a.ppy.sh/" + userdata.user_id) as ImageSource;
            globalrank.Content = "#" + userdata.pp_rank;
            countryRank.Content = "#" + userdata.pp_country_rank;

            rankedScore.Content = Tx.TC("player.Ranked score") + " " + Convert.ToInt64(userdata.ranked_score).ToString("n", NFInoDecimal);
            playCount.Content = Tx.TC("player.Play count") + " " + Convert.ToInt32(userdata.playcount).ToString("n", NFInoDecimal);
            acc.Content = Math.Round(Convert.ToDouble(userdata.accuracy), 2) + "%";
            pp.Content = Convert.ToDouble(userdata.pp_raw).ToString("n", NFIperformance) + " pp";
            totalScore.Content = Tx.TC("player.Total score") + " " + Convert.ToInt64(userdata.total_score).ToString("n", NFInoDecimal);
            level.Content = Tx.TC("player.Level") + " " + Math.Truncate(Convert.ToDouble(userdata.level)) + " (" + ((Convert.ToDouble(userdata.level) - Math.Truncate(Convert.ToDouble(userdata.level))) * 100).ToString("n", NFInoDecimal) + "%)";
            /* grades.Content = "SS / S / A: " + Convert.ToInt32(userdata.count_rank_ss).ToString("n", NFInoDecimal) + " / "
                                                + Convert.ToInt32(userdata.count_rank_s).ToString("n", NFInoDecimal) + " / "
                                                + Convert.ToInt32(userdata.count_rank_a).ToString("n", NFInoDecimal); */
            SS.Content = Convert.ToInt32(userdata.count_rank_ss).ToString("n", NFInoDecimal);
            S.Content = Convert.ToInt32(userdata.count_rank_s).ToString("n", NFInoDecimal);
            A.Content = Convert.ToInt32(userdata.count_rank_a).ToString("n", NFInoDecimal);
            userID.Content = Tx.TC("player.User ID") + " " + userdata.user_id;
            flag.Source = new BitmapImage(new Uri("/osu!rank;component/Drapeaux/" + userdata.country+".png", UriKind.Relative));
            // {pack://application:,,,/osu!rank;component/Drapeaux/unknown.png}
            levelProgress.Value = ((Convert.ToDouble(userdata.level) - Math.Truncate(Convert.ToDouble(userdata.level))) * 100);
            if (samename==true && samemode==true)
            {
                updateVariation(previousRefresh,userdata);
            }
            else
            {
                resetVariation();
            }
            lastUpdatedRefresher.Stop();
            lastRefreshedAgo = new TimeSpan(0);
            lastRefresh.Visibility = Visibility.Visible;
            string currentTime = lastRefreshedAgo.ToString();
            lastRefresh.Content = Tx.T("osu rank.auto refresh.updated ago", "time", currentTime);
            lastUpdatedRefresher.Start();
            
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
            int PCdiff, rankdiff, countryrankdiff, Sdiff, SSdiff, Adiff;
            double ppDiff, accDiff;
            SolidColorBrush decreaseColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#DDDA0000"));
            SolidColorBrush increaseColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#DD00A80F"));
            SolidColorBrush noChangeColor = Brushes.DimGray;

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

            // show back again the hidden labels
            rankedScore_diff.Visibility = Visibility.Visible;
            totalScore_diff.Visibility = Visibility.Visible;
            playcount_diff.Visibility = Visibility.Visible;
            globalrank_diff.Visibility = Visibility.Visible;
            countryRank_diff.Visibility = Visibility.Visible;
            S_diff.Visibility = Visibility.Visible;
            SS_diff.Visibility = Visibility.Visible;
            A_diff.Visibility = Visibility.Visible;
            pp_diff.Visibility = Visibility.Visible;
            acc_diff.Visibility = Visibility.Visible;

            // display these values
            #region rankedscore
            if (RscoreDiff > 0)
            {
                rankedScore_diff.Content = "+" + RscoreDiff.ToString("n", NFInoDecimal);
                rankedScore_diff.Foreground = increaseColor;
            }
            else if (RscoreDiff == 0)
            {
                rankedScore_diff.Content = "~0";
                rankedScore_diff.Foreground = noChangeColor;
            }
            else
            {
                rankedScore_diff.Content = RscoreDiff.ToString("n", NFInoDecimal);
                rankedScore_diff.Foreground = decreaseColor;
            }
            #endregion
            #region totalscore
            if (Scorediff > 0)
            {
                totalScore_diff.Content = "+" + Scorediff.ToString("n", NFInoDecimal);
                totalScore_diff.Foreground = increaseColor;
            }
            else if (Scorediff == 0)
            {
                totalScore_diff.Content = "~0";
                totalScore_diff.Foreground = noChangeColor;
            }
            else
            {
                totalScore_diff.Content = Scorediff.ToString("n", NFInoDecimal);
                totalScore_diff.Foreground = decreaseColor;
            }
            #endregion
            #region playcount
            if (PCdiff > 0)
            {
                playcount_diff.Content = "+" + PCdiff.ToString("n", NFInoDecimal);
                playcount_diff.Foreground = increaseColor;
            }
            else if (PCdiff == 0)
            {
                playcount_diff.Content = "~0";
                playcount_diff.Foreground = noChangeColor;
            }
            else
            {
                MessageBox.Show("Wow, playcount decreased? That's insane fam!");
                MessageBox.Show("This probably shouldn't happen tbh");
            }
            #endregion
            #region globalrank
            if (rankdiff > 0)
            {
                globalrank_diff.Content = "+" + rankdiff.ToString("n", NFInoDecimal);
                globalrank_diff.Foreground = increaseColor;
            }
            else if (rankdiff == 0)
            {
                globalrank_diff.Content = "~0";
                globalrank_diff.Foreground = noChangeColor;
            }
            else
            {
                globalrank_diff.Content = rankdiff.ToString("n", NFInoDecimal);
                globalrank_diff.Foreground = decreaseColor;
            }
            #endregion
            #region countryrank
            if (countryrankdiff > 0)
            {
                countryRank_diff.Content = "+" + countryrankdiff.ToString("n", NFInoDecimal);
                countryRank_diff.Foreground = increaseColor;
            }
            else if (countryrankdiff == 0)
            {
                countryRank_diff.Content = "~0";
                countryRank_diff.Foreground = noChangeColor;
            }
            else
            {
                countryRank_diff.Content = countryrankdiff.ToString("n", NFInoDecimal);
                countryRank_diff.Foreground = decreaseColor;
            }
            #endregion
            #region s diff
            if (Sdiff > 0)
            {
                S_diff.Content = "+" + Sdiff.ToString("n", NFInoDecimal);
                S_diff.Foreground = increaseColor;
            }
            else if (Sdiff < 0)
            {
                S_diff.Content = Sdiff.ToString("n", NFInoDecimal);
                S_diff.Foreground = decreaseColor;
            }
            else
            {
                S_diff.Content = "~0";
                S_diff.Foreground = noChangeColor;
            }
            #endregion
            #region ss diff
            if (SSdiff > 0)
            {
                SS_diff.Content = "+" + SSdiff.ToString("n", NFInoDecimal);
                SS_diff.Foreground = increaseColor;
            }
            else if (SSdiff < 0)
            {
                SS_diff.Content = SSdiff.ToString("n", NFInoDecimal);
                SS_diff.Foreground = decreaseColor;
            }
            else
            {
                SS_diff.Content = "~0";
                SS_diff.Foreground = noChangeColor;
            }
            #endregion
            #region a diff
            if (Adiff > 0)
            {
                A_diff.Content = "+" + Adiff.ToString("n", NFInoDecimal);
                A_diff.Foreground = increaseColor;
            }
            else if (Adiff < 0)
            {
                A_diff.Content = Adiff.ToString("n", NFInoDecimal);
                A_diff.Foreground = decreaseColor;
            }
            else
            {
                A_diff.Content = "~0";
                A_diff.Foreground = noChangeColor;
            }
            #endregion
            #region pp
            if (ppDiff > 0)
            {
                pp_diff.Content = "+" + ppDiff.ToString("n", NFIperformance);
                pp_diff.Foreground = increaseColor;
            }
            else if (ppDiff < 0)
            {
                MessageBox.Show("less pp 4 u, u little shit");
                pp_diff.Content = ppDiff.ToString("n", NFIperformance);
                pp_diff.Foreground = decreaseColor;
            }
            else
            {
                pp_diff.Content = "~0";
                pp_diff.Foreground = noChangeColor;
            }
            #endregion
            #region acc diff
            if (accDiff > 0)
            {
                acc_diff.Content = "+" + accDiff.ToString("n", NFIperformance);
                acc_diff.Foreground = increaseColor;
            }
            else if (accDiff < 0)
            {
                acc_diff.Content = accDiff.ToString("n", NFIperformance);
                acc_diff.Foreground = decreaseColor;
            }
            else
            {
                acc_diff.Content = "~0";
                acc_diff.Foreground = noChangeColor;
            }
            if ((string)acc_diff.Content==String.Concat("0",NFIperformance.NumberDecimalSeparator,"00") || 
                (string)acc_diff.Content == String.Concat("+0", NFIperformance.NumberDecimalSeparator ,"00"))
            {
                acc_diff.Content = "~0.00";
                acc_diff.Foreground = noChangeColor;
            }
            #endregion
        }

        private void resetVariation()
        {
            rankedScore_diff.Visibility = Visibility.Hidden;
            rankedScore_diff.Content = "";
            totalScore_diff.Visibility = Visibility.Hidden;
            totalScore_diff.Content = "";
            playcount_diff.Visibility = Visibility.Hidden;
            playcount_diff.Content = "";
            globalrank_diff.Visibility = Visibility.Hidden;
            globalrank_diff.Content = "";
            countryRank_diff.Visibility = Visibility.Hidden;
            countryRank_diff.Content = "";
            S_diff.Visibility = Visibility.Hidden;
            S_diff.Content = "";
            SS_diff.Visibility = Visibility.Hidden;
            SS_diff.Content = "";
            A_diff.Visibility = Visibility.Hidden;
            A_diff.Content = "";
            pp_diff.Visibility = Visibility.Hidden;
            pp_diff.Content = "";
            acc_diff.Visibility = Visibility.Hidden;
            acc_diff.Content = "";
        }

        private async void page_loaded(object sender, RoutedEventArgs e)
        {
            
            RenderOptions.SetBitmapScalingMode(Avatar, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(flag, BitmapScalingMode.HighQuality);
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

            lastUpdatedRefresher.Tick += new EventHandler(LastUpdated_Tick);
            lastUpdatedRefresher.Interval = new TimeSpan(0, 0, 1);

            nameInput.Text = Settings.Default.DefaultPlayer;
            gamemodeDropdown.SelectedIndex = Settings.Default.DefaultGamemode;
            if (Settings.Default.StartupCheck == true)
            {
                await fetchUserData(username, gamemode);
                if (Settings.Default.RefreshEnable == true)
                {
                    autoRefresh.IsEnabled = true;
                }
                else
                    autoRefresh.IsEnabled = false;
            }
        }

        private void LastUpdated_Tick(object sender, EventArgs e)
        {
            lastRefreshedAgo = lastRefreshedAgo + new TimeSpan(0,0,1);
            string currentTime = lastRefreshedAgo.ToString();
            lastRefresh.Content = Tx.T("osu rank.auto refresh.updated ago", "time", currentTime);
        }
    }
}
                                                                                                                                                         