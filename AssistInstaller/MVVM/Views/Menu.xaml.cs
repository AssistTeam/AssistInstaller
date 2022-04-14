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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AssistInstaller.MVVM.Views
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Page
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void Menu_Loaded(object sender, RoutedEventArgs e)
        {
            InstallBtn.Checked += InstallBtn_Checked;
            UninstallBtn.Checked += UninstallBtn_Checked;
            InstallBtn.IsChecked = true;
        }

        private void UninstallBtn_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.AppInstance.CurrentInstallerMode = Models.InstallerMode.Uninstall;
        }

        private void InstallBtn_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.AppInstance.CurrentInstallerMode = Models.InstallerMode.Install;
        }
    }
}
