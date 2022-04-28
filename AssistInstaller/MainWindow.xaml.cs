using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Newtonsoft.Json;

namespace AssistInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static internal InstallerViewModel AppInstance;
        private static MainWindow window;

        public MainWindow()
        {
            window = this;
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
                case InstallerPageState.LocationSelect:
                    await LocationLogic();
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

        public static void DisableBtns()
        {
            if (window.BackBtn.IsEnabled == false && window.NextBtn.IsEnabled == false)
            {
                window.BackBtn.Visibility = Visibility.Collapsed;
                window.NextBtn.Visibility = Visibility.Collapsed;
            }

            window.BackBtn.IsEnabled = false;
            window.NextBtn.IsEnabled = false;


        }

        public static void GotoComplete()
        {
            window.MainContentFrame.Navigate(new Uri("MVVM/Views/Complete.xaml", UriKind.RelativeOrAbsolute));
        }

        #endregion

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var s = await GetInstllerDataAsync();

            if(s == null)
            {
                MessageBox.Show("Failed to get installer data.");
                Environment.Exit(0);
            }
                

            AppInstance.InstallSettings = s;

            MainContentFrame.Content = new MVVM.Views.Menu();
            
        }

        private async Task<InstallSettings> GetInstllerDataAsync()
        {
            HttpClient client = new HttpClient();

            var resp = await client.GetAsync("https://api.assistapp.dev/data/installer");

            try
            {
                InstallSettings settings =
                JsonConvert.DeserializeObject<InstallSettings>(await resp.Content.ReadAsStringAsync());

                return settings;
            }
            catch(Exception e)
            {
                
                return null;
            }
            

            
        }


        #region Navigation Logic

        async Task MenuLogic()
        {
            if (AppInstance.CurrentInstallerMode == InstallerMode.Install)
                MainContentFrame.Navigate(new Uri("MVVM/Views/SelectLocation.xaml", UriKind.RelativeOrAbsolute));
            else if (AppInstance.CurrentInstallerMode == InstallerMode.Uninstall)
                MainContentFrame.Navigate(new Uri("MVVM/Views/Uninstall.xaml", UriKind.RelativeOrAbsolute));
        }

        async Task LocationLogic()
        {
            if (AppInstance.CurrentInstallerMode == InstallerMode.Install)
                MainContentFrame.Navigate(new Uri("MVVM/Views/Installation.xaml", UriKind.RelativeOrAbsolute));
            
        }

        #endregion
    }
}
