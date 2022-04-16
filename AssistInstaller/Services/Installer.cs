using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AssistInstaller.MVVM.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Reflection;
using System.Windows;

namespace AssistInstaller.Services
{
    internal class Installer
    {
        const string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private string appName = "Assist";
        private const string zipName = "assist.zip";
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static string tempFolderName = "temp";

        public async static Task InstallAssist()
        {

            // Check for Previous Install.

            List<string> installedPrograms = GetInstalledPrograms();

            if (installedPrograms.Count != 0)
            {
                var pro = installedPrograms.Where(p => p == "Assist").SingleOrDefault();

                if (pro != null)
                {
                    MessageBox.Show("We have detected Assist installed on your computer." + pro);
                    Environment.Exit(0);
                }
            }

            var rand = new Random();
            tempFolderName =
                new string(Enumerable.Repeat(chars, 9).Select(s => s[rand.Next(s.Length)]).ToArray());


            var installPath = MainWindow.AppInstance.InstalLoc;

            Directory.CreateDirectory(installPath); // Prevents Errors

            HttpClient client = new HttpClient();

            var resp = await client.GetAsync("https://api.assistapp.dev/data/update");

            InstallSettings settings =
                JsonConvert.DeserializeObject<InstallSettings>(await resp.Content.ReadAsStringAsync());

            //Clear Files in Dir
            foreach (var file in Directory.GetFiles(installPath, "*.*", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }



            // Download file
            using (var wClient = new WebClient())
            {
                wClient.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs args)
                {
                    MainWindow.AppInstance.InstallStatus = $"Downloading Assist {args.ProgressPercentage}";
                };
                await wClient.DownloadFileTaskAsync(settings.DownloadLocation, Path.Combine(installPath, zipName));
            }

            MainWindow.AppInstance.InstallProgress = 10;




            // Extract File
            ZipFile.ExtractToDirectory(Path.Combine(installPath, zipName), installPath, Encoding.UTF8);

            MainWindow.AppInstance.InstallProgress = 20;

            // Delete Init File
            File.Delete(Path.Combine(installPath, zipName));
            MainWindow.AppInstance.InstallProgress = 30;

            // Check for .Net 6
            bool bNetInstalledFlag = false;
            var installedRuntimes = CheckCoreDesktopRuntimes();
            foreach (var runtime in installedRuntimes)
            {
                if (runtime.Contains(settings.Dependencys.netVersion))
                {
                    bNetInstalledFlag = true;
                }
            }

            // After Checked installs
            if (!bNetInstalledFlag)
            {
                // Current Runtime is not installed. Install .Net
                var tempP = Path.Combine(installPath, tempFolderName);
                Directory.CreateDirectory(tempP);
                var installerName = "netInstaller.exe";
                using (WebClient wClient = new WebClient())
                {
                    wClient.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs args) {
                        MainWindow.AppInstance.InstallStatus = $"Downloading Microsoft Net Core {args.ProgressPercentage}";
                    };

                    await wClient.DownloadFileTaskAsync(settings.Dependencys.netInstall,
                        Path.Combine(tempP, installerName));

                }

                MainWindow.AppInstance.InstallProgress = 40;

                Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = "/install /quiet /norestart /passive",
                        FileName = Path.Combine(tempP, installerName),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                p.Start();

                p.WaitForExit();


                MainWindow.AppInstance.InstallProgress = 50;


            }

            MainWindow.AppInstance.InstallProgress = 50;

            bool bWVInstalledFlag = false;
            if (installedPrograms.Count != 0)
            {
                var pro = installedPrograms.Where(p => p.Contains("WebView2")).SingleOrDefault();

                if (pro != null)
                {
                    bWVInstalledFlag = true;
                }
            }

            if (!bWVInstalledFlag)
            {
                // Current Runtime is not installed. Install .Net
                var tempP = Path.Combine(installPath, tempFolderName);
                Directory.CreateDirectory(tempP);
                var installerName = "wbvInstaller.exe";
                using (WebClient wClient = new WebClient())
                {
                    wClient.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs args) {
                        MainWindow.AppInstance.InstallStatus = $"Downloading Microsoft Webview 2 {args.ProgressPercentage}";
                    };

                    await wClient.DownloadFileTaskAsync(settings.Dependencys.wbViewInstall,
                        Path.Combine(tempP, installerName));

                }

                MainWindow.AppInstance.InstallProgress = 60;


                Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = "/install",
                        FileName = Path.Combine(tempP, installerName),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                p.Start();

                p.WaitForExit();


            }
            MainWindow.AppInstance.InstallProgress = 75;

            // Delete Temp Folder
            if(Directory.Exists(Path.Combine(installPath, tempFolderName)))
                Directory.Delete(Path.Combine(installPath, tempFolderName), true);

            MainWindow.AppInstance.InstallProgress = 90;
            // Add Assist to Control Panel :)

            // Creates Short cut on desktop.
            appShortcutToDesktop(installPath);

            MainWindow.AppInstance.InstallProgress = 100;

        }


        #region Windows Registry Installation Dependencys
        // Shoutout Stack Overflow PogU

        public static List<string> GetInstalledPrograms()
        {
            var result = new List<string>();
            result.AddRange(GetInstalledProgramsFromRegistry(RegistryView.Registry32));
            result.AddRange(GetInstalledProgramsFromRegistry(RegistryView.Registry64));
            return result;
        }

        private static IEnumerable<string> GetInstalledProgramsFromRegistry(RegistryView registryView)
        {
            var result = new List<string>();

            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView).OpenSubKey(registry_key))
            {
                foreach (string subkey_name in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        if (IsProgramVisible(subkey))
                        {
                            result.Add((string)subkey.GetValue("DisplayName"));
                        }
                    }
                }
            }

            return result;
        }

        private static bool IsProgramVisible(RegistryKey subkey)
        {
            var name = (string)subkey.GetValue("DisplayName");
            var releaseType = (string)subkey.GetValue("ReleaseType");
            //var unistallString = (string)subkey.GetValue("UninstallString");
            var systemComponent = subkey.GetValue("SystemComponent");
            var parentName = (string)subkey.GetValue("ParentDisplayName");

            return
                !string.IsNullOrEmpty(name)
                && string.IsNullOrEmpty(releaseType)
                && string.IsNullOrEmpty(parentName)
                && (systemComponent == null);
        }

        private static void CreateInstallerDataFile()
        {

        }


        private static void appShortcutToDesktop(string installLoc)
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            using (StreamWriter writer = new StreamWriter(deskDir + "\\" + "Launch Assist" + ".url"))
            {
                string app = Path.Combine(installLoc, "Assist.exe");
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=file:///" + app);
                writer.WriteLine("IconIndex=0");
                string icon = app.Replace('\\', '/');
                writer.WriteLine("IconFile=" + icon);
            }
        }
        #endregion

        #region Check for Deps

        static List<string> CheckCoreDesktopRuntimes()
        {
            List<string> runtimes = new List<string>();

            try
            {
                Process p = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        Arguments = "--list-runtimes",
                        FileName = "dotnet",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                p.Start();
                string output;

                while ((output = p.StandardOutput.ReadLine()) != null)
                {
                    if(output.Contains("Microsoft.WindowsDesktop.App"))
                        runtimes.Add(output.Split('[')[0]);
                }

                p.WaitForExit();
            }
            catch (Exception e)
            {
            }


            return runtimes;

        }

        #endregion
    }
}
