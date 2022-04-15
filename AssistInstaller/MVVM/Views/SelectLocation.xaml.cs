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
using AssistInstaller.MVVM.ViewModels;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace AssistInstaller.MVVM.Views
{
    /// <summary>
    /// Interaction logic for SelectLocation.xaml
    /// </summary>
    public partial class SelectLocation : Page
    {
        public SelectLocation()
        {
            DataContext =  MainWindow.AppInstance;
            MainWindow.AppInstance.ChangePageState(Models.InstallerPageState.LocationSelect);
            InitializeComponent();
        }

        private void FileD_Btn_Click(object sender, RoutedEventArgs e)
        {

            using (CommonOpenFileDialog dia = new CommonOpenFileDialog())
            {
                dia.IsFolderPicker = true;
                dia.InitialDirectory = "C:/";
                if (dia.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    MainWindow.AppInstance.InstalLoc = dia.FileName;
                }
            }
        }
    }
}
