using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using Unclassified.TxLib;

namespace osurank
{
    public class osu
    {
        static int[] maxIntgr = {int.MaxValue}; 
        static public dynamic GetUser(string player, string apikey, int gamemode = 0, bool showErrors = true)
        {
            if (player != "")
            {
                try {
                    dynamic userdata = JsonConvert.DeserializeObject(new System.Net.WebClient().DownloadString("http://osu.ppy.sh/api/get_user?type=string&u=" + player + "&m=" + gamemode + "&k=" + apikey));
                    if (Convert.ToString(userdata) == "[]" || userdata == null)
                    {
                        if (showErrors == true) MessageBox.Show(Tx.T("errors.Does not exist", "name", player), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                        return maxIntgr;
                    }
                    else if (userdata[0].pp_rank == null)
                    {
                        if (showErrors == true) MessageBox.Show(Tx.T("errors.Did not play yet", "name", player), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return maxIntgr;
                    }
                    else return userdata;
                }
                catch (Exception)
                {
                    MessageBox.Show(Tx.T("osu rank.Servers unavailable"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                    return maxIntgr;
                }
            }
            else
            {
                if (showErrors == true) MessageBox.Show(Tx.T("errors.No name entered"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return maxIntgr; 
            }
        }
    }
}
