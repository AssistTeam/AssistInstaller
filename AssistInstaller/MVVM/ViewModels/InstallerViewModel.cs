using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssistInstaller.MVVM.Models;

namespace AssistInstaller.MVVM.ViewModels
{
    internal class InstallerViewModel : ViewModelBase
    {
        public InstallerMode CurrentInstallerMode;
        public InstallerPageState CurrentPageState { get; private set; }
        public List<InstallerPageState> PreviousPageState = new List<InstallerPageState>();
        public InstallSettings InstallSettings { get; set; }
        private string _installLoc = Path.Combine("C:/", "Assist");
        public string InstalLoc
        {
            get => _installLoc;
            set => SetProperty(ref _installLoc, value);
        }

        private string _installStatus;

        public string InstallStatus
        {
            get => _installStatus;
            set => SetProperty(ref _installStatus, value);
        }

        private double _installProgress;

        public double InstallProgress
        {
            get => _installProgress;
            set => SetProperty(ref _installProgress, value);
        }


        public void ChangePageState(InstallerPageState newState)
        {
            PreviousPageState.Add(CurrentPageState);
            CurrentPageState = newState;
        }

        public void ChangeToPreviousState()
        {
            if (PreviousPageState.Count == 0)
                return;

            CurrentPageState = PreviousPageState[PreviousPageState.Count - 1];
            PreviousPageState.RemoveAt(PreviousPageState.Count - 1);
        }


    }
}
