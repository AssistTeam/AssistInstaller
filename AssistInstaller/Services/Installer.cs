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
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace AssistInstaller.Services
{
    internal class Installer
    {
        const string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private string appName = "Assist";
        private const string zipName = "assist.zip";
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static string tempFolderName = "temp";
        private static InstallSettings settings => MainWindow.AppInstance.InstallSettings;

        public async static Task InstallAssist()
        {
            MainWindow.AppInstance.InstallStatus = $"Getting Installer Data";

            var installedPrograms = GetInstalledPrograms();

            var rand = new Random();
            tempFolderName =
                new string(Enumerable.Repeat(chars, 9).Select(s => s[rand.Next(s.Length)]).ToArray());


            var installPath = Path.Combine(MainWindow.AppInstance.InstalLoc, "Assist");

            Directory.CreateDirectory(installPath); // Prevents Errors

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



            MainWindow.AppInstance.InstallStatus = $"Extracting Assist";
            // Extract File
            ZipFile.ExtractToDirectory(Path.Combine(installPath, zipName), installPath, Encoding.UTF8);

            MainWindow.AppInstance.InstallProgress = 20;

            MainWindow.AppInstance.InstallStatus = $"Deleting Download File";
            // Delete Init File
            File.Delete(Path.Combine(installPath, zipName));
            MainWindow.AppInstance.InstallProgress = 30;

            MainWindow.AppInstance.InstallStatus = $"Checking for Latest .Net";

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

            MainWindow.AppInstance.InstallStatus = $"Checking for WebView2 Framework";
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
            await appShortcutToDesktop(installPath);
            await CreateInstallerFileAsync();
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

        
        private static async Task AddShortcut(string installLoc)
        {
            string commonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            string appStartMenuPath = Path.Combine(commonStartMenuPath, "Programs", "Assist");

            if (!Directory.Exists(appStartMenuPath))
                Directory.CreateDirectory(appStartMenuPath);

            string shortcutLocation = Path.Combine(appStartMenuPath, "Assist" + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

            //shortcut.IconLocation = @"C:\Program Files (x86)\TestApp\TestApp.ico"; //uncomment to set the icon of the shortcut
            shortcut.TargetPath = Path.Combine(installLoc, "Assist.exe");
            shortcut.Save();
        }

        private static async Task appShortcutToDesktop(string installLoc)
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Assist.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.TargetPath = Path.Combine(installLoc, "Assist.exe");
            shortcut.WorkingDirectory = Path.Combine(installLoc, "Assist.exe");
            shortcut.Save();
            await AddShortcut(installLoc);
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


        private static async Task CreateInstallerFileAsync()
        {
            var s = new
            {
                installDate = DateTime.UtcNow.ToString(),
                versionInstalled = MainWindow.AppInstance.InstallSettings.VersionNumber,
                installLoction = MainWindow.AppInstance.InstalLoc,
                executablePath = Path.Combine(MainWindow.AppInstance.InstalLoc, "Assist.exe")
            };

            var progData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData);
            var jData = JsonConvert.SerializeObject(s, Formatting.Indented);

            var di = Directory.CreateDirectory(Path.Combine(progData,"Assist"));

            File.WriteAllText(Path.Combine(di.FullName, "AssistInstallSettings.json"), jData);
        }
    }
}
