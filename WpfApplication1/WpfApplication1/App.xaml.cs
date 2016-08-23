using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using osu_rank.Properties;

namespace osu_rank
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
        public static int version = 19;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (Settings.Default.CanUpgrade)
            {
                Settings.Default.Upgrade();
                Settings.Default.CanUpgrade = false;
            }
        }
    }
}
