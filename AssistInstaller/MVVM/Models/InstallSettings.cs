using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssistInstaller.MVVM.Models
{
    internal class InstallSettings
    {

        /*
         * DownloadLocation : "linktoDownload",
         * VersionNumber : "1.0.0.0",
         * Dependencys {
         *    netInstall : "linktoDownload",
         *    wbViewInstall : "linktoDownload",
         *    uninstallerInstall : "linktoDownload",
         * }
         */

        public string DownloadLocation { get; set; }
        public string VersionNumber { get; set; }
        public Dependencys Dependencys { get; set; }
    }
    class Dependencys
    {
        public string netInstall { get; set; }
        public string netVersion { get; set; } 
        public string wbViewInstall { get; set; }
        public string uninstallerInstall { get; set; }
    }
}
