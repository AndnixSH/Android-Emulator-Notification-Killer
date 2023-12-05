using Microsoft.Win32;
using Ookii.Dialogs.WinForms;
using SaveToGameWpf.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Emulator_Notification_Killer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            GetInstalledApps();
        }
        public static string NoxBackup = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NoxBak");
        public static string LD3Backup = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LD3Bak");
        public static string LD4Backup = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LD5Bak");
        public static string LD64Backup = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LD564Bak");
        public static string LD9Backup = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LD9Bak");
        public void GetInstalledApps()
        {
            string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    Debug.WriteLine(skName);
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            string path = (string)sk.GetValue("UninstallString");
                            if (path != null)
                            {
                                string dirName = Path.GetDirectoryName(path.Replace("\"", ""));
                                if (skName == "Nox")
                                    noxPlayerPath.Text = dirName;
                                if (skName == "LDPlayer")
                                    ld3Path.Text = dirName;
                                if (skName == "LDPlayer4")
                                    ld4Path.Text = dirName;
                                if (skName == "LDPlayer64")
                                    ld64Path.Text = dirName;
                                if (skName == "LDPlayer9")
                                    ld9Path.Text = dirName;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void selNoxPath_Click(object sender, EventArgs e)
        {
            using (VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = Path.Combine(fbd.SelectedPath, "Nox.exe");
                    if (File.Exists(path))
                    {
                        Log("Location selected: " + fbd.SelectedPath);
                        noxPlayerPath.Text = fbd.SelectedPath;
                    }
                    else
                        Log("The location you have selected is not Nox Player");
                }
            }
        }

        private void selLd3Path_Click(object sender, EventArgs e)
        {
            using (VistaFolderBrowserDialog fbd = new VistaFolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = Path.Combine(fbd.SelectedPath, "dnplayer.exe");
                    if (File.Exists(path))
                    {
                        Log("Location selected: " + fbd.SelectedPath);
                        noxPlayerPath.Text = fbd.SelectedPath;
                    }
                    else
                        Log("The location you have selected is not LDPlayer 3");
                }
            }
        }

        private void patchNoxPlayer_Click(object sender, EventArgs e)
        {
            try
            {
                if (String.IsNullOrEmpty(noxPlayerPath.Text))
                {
                    Log("The Nox Player location is not set");
                    return;
                }

                Directory.CreateDirectory(NoxBackup);

                string noxQt5Path = Path.Combine(noxPlayerPath.Text, "Qt5Widgets.dll");
                string noxPath = Path.Combine(noxPlayerPath.Text, "Nox.exe");

                string noxQt5BakPath = Path.Combine(NoxBackup, "Qt5Widgets.dll");
                string noxBakPath = Path.Combine(NoxBackup, "Nox.exe");

                bool p1 = Patch.PatternExists(noxQt5Path, "55 57 56 53 89 CE 83 EC 1C 8B 41 04 8B 58 08 8B");
                bool p2 = Patch.PatternExists(noxPath, "89 34 24 89 D9 E8 58 ?? FE FF 83 EC 04");
                if (p1 && p2)
                {
                    Log("Created backup on " + noxQt5BakPath);
                    File.Copy(noxQt5Path, noxQt5BakPath, true);
                    File.Copy(noxPath, noxBakPath, true);
                }
                else
                {
                    Log("Failed to patch Nox Player. It might be already patched or the patterns could not be found");
                    return;
                }

                if (Patch.PatchFile(noxQt5Path, "55 57 56 53 89 CE 83 EC 1C 8B 41 04 8B 58 08 8B", "B8 01 00 00 00 C3"))
                    Log("Patched Qt5Widgets.dll");
                else
                    Log("Failed to patch Qt5Widgets.dll");

                if (Patch.PatchFile(noxPath, "89 34 24 89 D9 E8 58 ?? FE FF 83 EC 04", "90 90 90 90 90 90 90 90 90 90 90 90 90"))
                    Log("Patched Nox.exe");
                else
                    Log("Failed to patch Nox.exe");
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                    Log("Failed to patch. Make sure both Nox.exe and MultiPlayerManager.exe are not running");
                else
                    Log(ex.ToString());
            }
        }

        private void patchLd3_Click(object sender, EventArgs e)
        {
            LDPatch(ld3Path.Text, LD3Backup);
        }

        private void patchLd4_Click(object sender, EventArgs e)
        {
            LDPatch(ld4Path.Text, LD4Backup);
        }

        private void ld564Patch_Click(object sender, EventArgs e)
        {
            LDPatch(ld64Path.Text, LD64Backup);
        }

        private void ld9Patch_Click(object sender, EventArgs e)
        {
            LDPatch(ld9Path.Text, LD9Backup);
        }

        private void LDPatch(string ldPath, string bakPath)
        {
            try
            {
                if (String.IsNullOrEmpty(ldPath))
                {
                    Log("The LDPlayer location is not set");
                    return;
                }

                Directory.CreateDirectory(bakPath);

                string bakExePath = Path.Combine(bakPath, "dnplayer.exe");

                string dnExePath = Path.Combine(ldPath, "dnplayer.exe");
                bool p1 = Patch.PatternExists(dnExePath, "51 8B 8B ?? ?? 00 00 50 56 E8 ?? ?? 01 00");
                bool p2 = Patch.PatternExists(dnExePath, "50 8B ?? E8 ?? ?? 00 00 83 7C 24");
                bool p3 = Patch.PatternExists(dnExePath, "8B ?? 50 E8 ?? ?? 00 00 83 7C 24 38 08");
                if (p1 && p2 && p3)
                {
                    Log("Created backup on " + bakPath);
                    File.Copy(dnExePath, bakExePath, true);
                }
                else
                {
                    Log("Failed to patch LDPlayer. It might be already patched or the patterns could not be found");
                    return;
                }

                Patch.PatchFile(dnExePath, "51 8B 8B ?? ?? 00 00 50 56 E8 ?? ?? 01 00", "90 90 90 90 90 90 90 90 90 90 90 90 90 90");
                Patch.PatchFile(dnExePath, "50 8B ?? E8 ?? ?? 00 00 83 7C 24", "90 90 90 90 90 90 90 90");
                Patch.PatchFile(dnExePath, "8B ?? 50 E8 ?? ?? 00 00 83 7C 24 38 08", "90 90 90 90 90 90 90 90"); // 2 occurrences

                Log("Patched dnplayer.exe");
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                    Log("Failed to patch. Make sure dnplayer.exe is not running");
                else
                    Log(ex.ToString());
            }
        }

        private void Log(string msg)
        {
            richTextBox1.AppendText(msg + "\n");
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }
    }
}
