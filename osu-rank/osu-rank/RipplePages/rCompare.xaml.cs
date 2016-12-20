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
using Unclassified.TxLib;
using System.Globalization;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using osu_rank.Properties;

namespace osurank.RipplePages
{
    /// <summary>
    /// Logique d'interaction pour Compare.xaml
    /// </summary>
    public partial class Compare : Page
    {
        // ----- VARIABLES START -----
        FontWeight regular = FontWeights.Normal;
        FontWeight bold = FontWeights.Bold;
        DispatcherTimer autoRefresh = new DispatcherTimer();
        string username1 = Settings.Default.DefaultPlayer;
        string username2 = Settings.Default.DefaultPlayer2;
        int gamemode = Settings.Default.DefaultGamemode;
        NumberFormatInfo NFInoDecimal = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        NumberFormatInfo NFIperformance = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
        // ------ VARIABLES END ------
        public Compare()
        {
            InitializeComponent();
        }

        private async void autoRefresh_Tick(object sender, EventArgs e)
        {
            await fetchUserData(username1,username2, gamemode, false);
        }

        private async void goCompare_Click(object sender, RoutedEventArgs e)
        {
            username1 = player1Input.Text;
            username2 = player2Input.Text;
            gamemode = gamemodeDropdown.SelectedIndex;
            await fetchUserData(username1, username2, gamemode);
            timerReset();
        }        

        private void resetDifference()
        {
            diff_ranking.Content = Tx.T("comparator.n ranks", 0);
            diff_performance.Content = "0 pp";
            diff_acc.Content = "0.00%";
            diff_playcount.Content = "0";
            diff_totalscore.Content = "0";
            diff_rankedscore.Content = "0";
        }
        private async Task fetchUserData(string player_name1, string player_name2, int gamemode_index, bool show_errors = true)
        {
            // retrieve userdata
            dynamic userdata1 = await ripple.GetUserAsync(player: player_name1, gamemode: gamemode_index, showErrors:show_errors);
            dynamic userdata2 = await ripple.GetUserAsync(player: player_name2, gamemode: gamemode_index, showErrors:show_errors);
            dynamic player1 = null;
            dynamic player2 = null;

            // do nothing if player did not play or player is invalid
            if (Convert.ToString(userdata1[0]) == Convert.ToString(int.MaxValue)) { return; }
            if (Convert.ToString(userdata2[0]) == Convert.ToString(int.MaxValue)) { return; }
            if (userdata1[0].pp_rank == null | userdata1 == null) { return; }
            if (userdata2[0].pp_rank == null | userdata2 == null) { return; }
            

            if (userdata1[0] != null)
            {
                player1 = userdata1[0];
                p1_avatar.Source = new ImageSourceConverter().ConvertFromString("http://a.ripple.moe/" + player1.user_id) as ImageSource;
                p1_name.Content = Convert.ToString(player1.username);
                p1_ranking.Content = "#" + player1.pp_rank;
                p1_performance.Content = Convert.ToDouble(player1.pp_raw).ToString("n", NFIperformance) + " pp";
                p1_acc.Content = Math.Round(Convert.ToDouble(player1.accuracy), 2) + "%";
                p1_playcount.Content = Convert.ToInt32(player1.playcount).ToString("n", NFInoDecimal);
                p1_totalscore.Content = Convert.ToInt64(player1.total_score).ToString("n", NFInoDecimal);
                p1_rankedscore.Content = Convert.ToInt64(player1.ranked_score).ToString("n", NFInoDecimal);
            }
            
           
            if (userdata2[0] != null)
            {
                player2 = userdata2[0];
                p2_avatar.Source = new ImageSourceConverter().ConvertFromString("http://a.ripple.moe/" + player2.user_id) as ImageSource;
                p2_name.Content = Convert.ToString(player2.username);
                p2_ranking.Content = "#" + player2.pp_rank;
                p2_performance.Content = Convert.ToDouble(player2.pp_raw).ToString("n", NFIperformance) + " pp";
                p2_acc.Content = Math.Round(Convert.ToDouble(player2.accuracy), 2) + "%";
                p2_playcount.Content = Convert.ToInt32(player2.playcount).ToString("n", NFInoDecimal);
                p2_totalscore.Content = Convert.ToInt64(player2.total_score).ToString("n", NFInoDecimal);
                p2_rankedscore.Content = Convert.ToInt64(player2.ranked_score).ToString("n", NFInoDecimal);
            }
            playerAllregular(1); playerAllregular(2);
            if (player1 == player2 && player1 != null)
            {
                playerAllbold(1);
                playerAllbold(2);
                resetDifference();
            }
            else if (player1 == null && player2 != null)
            {
                playerAllbold(2);
                resetPlayer(1);
                resetDifference();
            }
            else if (player1 != null && player1 == null)
            {
                playerAllbold(1);
                resetPlayer(2);
                resetDifference();
            }
            else if (player1 == null && player2 == null)
            {
                resetPlayer(1);
                resetPlayer(2);
                resetDifference();
            }
            else compareStats(player1, player2);

        }
        private void compareStats(dynamic player1, dynamic player2)
        {
            #region comparison
                // variables to simplify comparison
                // player 1
                int p1ranking = Convert.ToInt32(player1.pp_rank);
                double p1pp = Convert.ToDouble(player1.pp_raw);
                double p1acc = Convert.ToDouble(player1.accuracy);
                int p1playcount = Convert.ToInt32(player1.playcount);
                long p1totalscore = Convert.ToInt64(player1.total_score);
                long p1rankedscore = Convert.ToInt64(player1.ranked_score);
                // player 2
                int p2ranking = Convert.ToInt32(player2.pp_rank);
                double p2pp = Convert.ToDouble(player2.pp_raw);
                double p2acc = Convert.ToDouble(player2.accuracy);
                int p2playcount = Convert.ToInt32(player2.playcount);
                long p2totalscore = Convert.ToInt64(player2.total_score);
                long p2rankedscore = Convert.ToInt64(player2.ranked_score);
                // difference
                long drankedscore, dtotalscore; int dranking, dplaycount; double dpp, dacc;

                // actual comparison display
                // pp and ranking
                if (p1ranking < p2ranking)
                {
                    dranking = p2ranking - p1ranking;
                    dpp = p1pp - p2pp;
                    p1_ranking.FontWeight = bold;
                    p1_performance.FontWeight = bold;
                }
                else
                {
                    dranking = p1ranking - p2ranking;
                    dpp = p2pp - p1pp;
                    p2_ranking.FontWeight = bold;
                    p2_performance.FontWeight = bold;
                }
                diff_ranking.Content = Tx.T("comparator.n ranks", dranking);
                diff_performance.Content = dpp.ToString("n", NFIperformance) + " pp";
                // acc
                if (p1acc > p2acc)
                {
                    dacc = p1acc - p2acc;
                    p1_acc.FontWeight = bold;
                }
                else
                {
                    dacc = p2acc - p1acc;
                    p2_acc.FontWeight = bold;
                }
                diff_acc.Content = dacc.ToString("n", NFIperformance) + "%";
                // playcount
                if (p1playcount > p2playcount)
                {
                    dplaycount = p1playcount - p2playcount;
                    p1_playcount.FontWeight = bold;
                }
                else
                {
                    dplaycount = p2playcount - p1playcount;
                    p2_playcount.FontWeight = bold;
                }
                diff_playcount.Content = dplaycount.ToString("n", NFInoDecimal);
                // totalscore
                if (p1totalscore > p2totalscore)
                {
                    dtotalscore = p1totalscore - p2totalscore;
                    p1_totalscore.FontWeight = bold;
                }
                else
                {
                    dtotalscore = p2totalscore - p1totalscore;
                    p2_totalscore.FontWeight = bold;
                }
                diff_totalscore.Content = dtotalscore.ToString("n", NFInoDecimal);
                // rankedscore
                if (p1rankedscore > p2rankedscore)
                {
                    drankedscore = p1rankedscore - p2rankedscore;
                    p1_rankedscore.FontWeight = bold;
                }
                else
                {
                    drankedscore = p2rankedscore - p1rankedscore;
                    p2_rankedscore.FontWeight = bold;
                }
                diff_rankedscore.Content = drankedscore.ToString("n", NFInoDecimal);
                #endregion
        }

        private void playerAllregular(int player)
        {
            if (player == 1)
            {
                p1_ranking.FontWeight = regular;
                p1_performance.FontWeight = regular;
                p1_acc.FontWeight = regular;
                p1_playcount.FontWeight = regular;
                p1_totalscore.FontWeight = regular;
                p1_rankedscore.FontWeight = regular;
            }
            else if (player == 2)
            {
                p2_ranking.FontWeight = regular;
                p2_performance.FontWeight = regular;
                p2_acc.FontWeight = regular;
                p2_playcount.FontWeight = regular;
                p2_totalscore.FontWeight = regular;
                p2_rankedscore.FontWeight = regular;
            }
        }
        private void playerAllbold(int player)
        {
            if (player == 1)
            {
                p1_ranking.FontWeight = bold;
                p1_performance.FontWeight = bold;
                p1_acc.FontWeight = bold;
                p1_playcount.FontWeight = bold;
                p1_totalscore.FontWeight = bold;
                p1_rankedscore.FontWeight = bold;
            }
            else if (player == 2)
            {
                p2_ranking.FontWeight = bold;
                p2_performance.FontWeight = bold;
                p2_acc.FontWeight = bold;
                p2_playcount.FontWeight = bold;
                p2_totalscore.FontWeight = bold;
                p2_rankedscore.FontWeight = bold;
            }
        }
        private void resetPlayer(int player)
        {
            if (player == 1)
            {
                p1_ranking.Content = "#0";
                p1_performance.Content = "0 pp";
                p1_acc.Content = "0.00%";
                p1_playcount.Content = "0";
                p1_totalscore.Content = "0";
                p1_rankedscore.Content = "0";
            }
            else if (player == 2)
            {
                p2_ranking.Content = "#0";
                p2_performance.Content = "0 pp";
                p2_acc.Content = "0.00%";
                p2_playcount.Content = "0";
                p2_totalscore.Content = "0";
                p2_rankedscore.Content = "0";
            }
        }
        private void timerReset()
        {
            autoRefresh.Stop();
            autoRefresh.Start();
        }

        private async void page_loaded(object sender, RoutedEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(p1_avatar, BitmapScalingMode.HighQuality);   // Completes what the designer can't do
            RenderOptions.SetBitmapScalingMode(p2_avatar, BitmapScalingMode.HighQuality);   // 
            this.Title = this.Title + " - osu!rank";                                        // 
            Settings.Default.LastWindow = "Compare";
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
            if (Settings.Default.StartupCheck == true)
            {
                await fetchUserData(username1, username2, gamemode);
                if (Settings.Default.RefreshEnable == true)
                {
                    autoRefresh.Start();
                }
            }
            player1Input.Text = username1;
            player2Input.Text = username2;
            gamemodeDropdown.SelectedIndex = gamemode;
        }

        private void gamemode_changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (gamemodeDropdown.SelectedIndex == 1 || gamemodeDropdown.SelectedIndex == 2)
                {
                    pp_not_implemented.Visibility = Visibility.Visible;
                }
                else
                {
                    pp_not_implemented.Visibility = Visibility.Hidden;
                }
            }
            catch { }
        }
    }
}
