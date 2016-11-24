using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace osurank
{
    /// <summary>
    /// Logique d'interaction pour About.xaml
    /// </summary>
    public partial class About : Page
    {
        public About()
        {
            InitializeComponent();
            about.Content = "osu!rank " + Assembly.GetExecutingAssembly().GetName().Version.Major
                                        + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor
                                        + "." + Assembly.GetExecutingAssembly().GetName().Version.Build;
        }

        private void page_loaded(object sender, RoutedEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(osuSmall, BitmapScalingMode.HighQuality);
            RenderOptions.SetBitmapScalingMode(stdSmall, BitmapScalingMode.HighQuality);
        }

        private void goTopic_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://osu.ppy.sh/forum/t/478865");
        }

        private void goGeetHub_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Jeremiidesu/osu-rank");
        }

        private void reportBug_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Jeremiidesu/osu-rank/issues");
        }

        private void goContribute_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Jeremiidesu/osu-rank/tree/master/osu-rank/osu-rank/Stringlists");
        }

        private void goXial_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://osu.ppy.sh/u/Xial");
        }
    }
}
