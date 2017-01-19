using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows;
using Unclassified.TxLib;
using System.Net.Http;
using System.IO;

// TODO: Look closely at all possible cases for the get_scores API method on both osu! and Ripple servers
// and return an "universal" format (aka if i accidentally use osu!'s method into a ripple page osu!rank
// doesn't insult me)
// example: get_scores when there's no result: 
//   - osu! returns `[]` (empty json array)
//   - ripple returns `null` (null json value)
// mark's fix with maxIntgr might be of some help...

namespace osurank
{
    // osu!rank related
    public class osu
    {
        static int[] maxIntgr = {int.MaxValue}; 
        // sync
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
                    MessageBox.Show(Tx.T("osu rank.Servers unavailable", "server", "osu!"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
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
        static public dynamic GetScores(string apikey, int beatmap, int gamemode = 0, string player = "")
        {
            return JsonConvert.DeserializeObject(new System.Net.WebClient().DownloadString("http://osu.ppy.sh/api/get_scroes?type=string&u=" + player + "&m=" + gamemode + "&k=" + apikey + "&b=" + beatmap));
        }

        //async
        static public async Task<dynamic> GetUserAsync(string player, string apikey, int gamemode = 0, bool showErrors = true)
        {
            if (player != "")
            {
                try
                {
                    dynamic userdata = null;
                    using (HttpClient apiCall = new HttpClient())
                    {
                        userdata = JsonConvert.DeserializeObject(await apiCall.GetStringAsync("http://osu.ppy.sh/api/get_user?type=string&u=" + player + "&m=" + gamemode + "&k=" + apikey));
                    }
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
                    MessageBox.Show(Tx.T("osu rank.Servers unavailable", "server", "osu!"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
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
        static public async Task<dynamic> GetScoresAsync(string apikey, int beatmap, int gamemode = 0, string player = "")
        {
            return JsonConvert.DeserializeObject(await new HttpClient().GetStringAsync("http://osu.ppy.sh/api/get_scroes?type=string&u=" + player + "&m=" + gamemode + "&k=" + apikey + "&b=" + beatmap));            
        }
    }

    public class ripple
    {
        static int[] maxIntgr = { int.MaxValue };
        // sync
        static public dynamic GetUser(string player, int gamemode = 0, bool showErrors = true)
        {
            if (player != "")
            {
                try
                {
                    dynamic userdata = JsonConvert.DeserializeObject(new System.Net.WebClient().DownloadString("http://ripple.moe/api/get_user?type=string&u=" + player + "&m=" + gamemode));
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
                    MessageBox.Show(Tx.T("osu rank.Servers unavailable", "server", "Ripple"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
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
        static public dynamic GetScores(int beatmap, int gamemode = 0, string player = "")
        {
            return JsonConvert.DeserializeObject(new System.Net.WebClient().DownloadString("http://ripple.moe/api/get_scroes?type=string&u=" + player + "&m=" + gamemode + "&b=" + beatmap));
        }

        //async
        static public async Task<dynamic> GetUserAsync(string player, int gamemode = 0, bool showErrors = true)
        {
            if (player != "")
            {
                try
                {
                    dynamic userdata = null;
                    using (HttpClient apiCall = new HttpClient())
                    {
                        userdata = JsonConvert.DeserializeObject(await apiCall.GetStringAsync("http://ripple.moe/api/get_user?type=string&u=" + player + "&m=" + gamemode));
                    }
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
                    MessageBox.Show(Tx.T("osu rank.Servers unavailable","server","Ripple"), Tx.T("errors.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
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
        static public async Task<dynamic> GetScoresAsync(int beatmap, int gamemode = 0, string player = "")
        {
            return JsonConvert.DeserializeObject(await new HttpClient().GetStringAsync("http://ripple.moe/api/get_scroes?type=string&u=" + player + "&m=" + gamemode + "&b=" + beatmap));
        }
    }

    // helper
    public static class WebUtils
    {
        public static Task DownloadAsync(string requestUri, string filename)
        {
            if (requestUri == null)
                throw new ArgumentNullException("requestUri");

            return DownloadAsync(new Uri(requestUri), filename);
        }

        public static async Task DownloadAsync(Uri requestUri, string filename)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    using (
                        Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await contentStream.CopyToAsync(stream);
                    }
                }
            }
        }
    }
}
