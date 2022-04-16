using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Path = System.IO.Path;

namespace AssistInstaller.MVVM.Views
{
    /// <summary>
    /// Interaction logic for Complete.xaml
    /// </summary>
    public partial class Complete : Page
    {
        public Complete()
        {
            InitializeComponent();
        }

        private void Complete_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.DisableBtns();
        }

        private void Launch_BTN(object sender, RoutedEventArgs e)
        {
            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(MainWindow.AppInstance.InstalLoc, "Assist.exe"),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            p.Start();

            Environment.Exit(0);
        }
    }
}
