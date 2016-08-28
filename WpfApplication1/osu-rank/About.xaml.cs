using System;
using System.Collections.Generic;
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

namespace osu_rank
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
    }
}
