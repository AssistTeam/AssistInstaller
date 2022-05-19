using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using AssistInstaller.Services;
using Path = System.Windows.Shapes.Path;

namespace AssistInstaller.MVVM.Views
{
    /// <summary>
    /// Interaction logic for Uninstaller.xaml
    /// </summary>
    public partial class Uninstaller : Page
    {
        public Uninstaller()
        {
            InitializeComponent();
        }

        private async void Uninstall_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.DisableBtns();
            MainWindow.AppInstance.InstallProgress = 0;

            bool p = await Installer.UninstallAssist();
            if (!p)
                MessageBox.Show("Failed to Uninstall");
            await CleanUp();

            // Once install completed 

            MessageBox.Show("Assist has Successfully Uninstalled, Thank you for using Assist!. Please delete the Uninstaller to officially finish removing assist.");

            try
            {
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        Arguments = AppDomain.CurrentDomain.BaseDirectory,
                        FileName = "explorer.exe"
                    };

                    Process.Start(startInfo);
                }
            }
            catch (Exception xe)
            {

            }
            finally
            {
                Environment.Exit(0);
            }
        }

        private async Task CleanUp()
        {
            var pathExe = Process.GetCurrentProcess().MainModule.FileName;
            
            

            var uninstallerStringName = Process.GetCurrentProcess().ProcessName + ".exe";
            pathExe = pathExe.Replace(uninstallerStringName, "");

            File.WriteAllText("pogPath.txt", pathExe);
            if (Directory.Exists(pathExe))
            {
                var installFiles = Directory.GetFiles(pathExe, "*.*", SearchOption.AllDirectories);

                foreach (string file in installFiles)
                {
                    if (!file.Contains("Uninstaller"))
                        File.Delete(file);
                }

                var appDataSettingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Assist");
                if (Directory.Exists(appDataSettingsPath))
                {
                    var settingsFiles = Directory.GetFiles(appDataSettingsPath, "*.*", SearchOption.AllDirectories);
                    MainWindow.AppInstance.InstallProgress = 50;
                    foreach (var settingsFile in settingsFiles)
                        File.Delete(settingsFile);
                }
            }


            

            await Task.Delay(2000);
        }
    }
}
