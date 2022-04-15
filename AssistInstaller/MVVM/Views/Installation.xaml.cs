using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using AssistInstaller.Services;

namespace AssistInstaller.MVVM.Views
{
    /// <summary>
    /// Interaction logic for Installation.xaml
    /// </summary>
    public partial class Installation : Page
    {
        public Installation()
        {
            DataContext = MainWindow.AppInstance;
            InitializeComponent();
        }

        private async void Installation_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.DisableBtns();
            MainWindow.AppInstance.InstallProgress = 0;

            await Installer.InstallAssist();

            // Once install completed 

            MainWindow.GotoComplete();
        }
    }
}
