using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using osu_rank.Properties;
using Unclassified.TxLib;

namespace osurank
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Needed for default language
        public static readonly CultureInfo systemCulture = CultureInfo.CurrentCulture;
        // Needed for update logic
        public static bool HasCheckedForUpdates = false;
        public static int version = 27;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (Settings.Default.CanUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.CanUpgrade = false;
                Settings.Default.Save();
            }

            Tx.LoadFromEmbeddedResource("osu_rank.osu_rank.txd");
            #region language
            if (Settings.Default.LanguageCode != "")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.LanguageCode);
            }
            else if (Settings.Default.LanguageCode == "")
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = App.systemCulture;
            }
            /* don't use useless debug code in released build
            string[] resourceNames = this.GetType().Assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames)
            {
                Console.WriteLine(resourceName);
            }
            Console.WriteLine(System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName); */
            #endregion
        }
    }
}
