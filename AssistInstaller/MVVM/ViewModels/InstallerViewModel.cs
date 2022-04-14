using System;
using System.Collections.Generic;
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
