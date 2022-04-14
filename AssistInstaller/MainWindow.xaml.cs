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
using AssistInstaller.MVVM.Models;
using AssistInstaller.MVVM.ViewModels;
using AssistInstaller.MVVM.Views;

namespace AssistInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static internal InstallerViewModel AppInstance;


        public MainWindow()
        {
            DataContext = AppInstance = new InstallerViewModel();
            AppInstance.ChangePageState(InstallerPageState.Menu);
            InitializeComponent();
            
        }

        #region Window Navigations
        private void WindowBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region Installer Navigation

        private async void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (AppInstance.CurrentPageState)
            {
                case InstallerPageState.Menu:
                    await MenuLogic();
                    break;
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if(AppInstance.CurrentPageState != InstallerPageState.Menu)
            {
                AppInstance.ChangeToPreviousState();
                MainContentFrame.GoBack();

            }
                
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Content = new MVVM.Views.Menu();
        }


        #region Navigation Logic

        async Task MenuLogic()
        {
            if (AppInstance.CurrentInstallerMode == InstallerMode.Install)
                MainContentFrame.Navigate(new Uri("MVVM/Views/SelectLocation.xaml", UriKind.RelativeOrAbsolute));
            else if (AppInstance.CurrentInstallerMode == InstallerMode.Uninstall)
                MainContentFrame.Navigate(new Uri("MVVM/Views/Uninstall.xaml", UriKind.RelativeOrAbsolute));
        }

        #endregion
    }
}
