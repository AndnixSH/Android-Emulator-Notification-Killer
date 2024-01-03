using Microsoft.Win32;
using Ookii.Dialogs.WinForms;
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

namespace EmuPatcher
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
                string noxMultiPath = Path.Combine(noxPlayerPath.Text, "MultiPlayerManager.exe");

                string noxQt5BakPath = Path.Combine(NoxBackup, "Qt5Widgets.dll");
                string noxBakPath = Path.Combine(NoxBackup, "Nox.exe");
                string noxMultiBakPath = Path.Combine(NoxBackup, "MultiPlayerManager.exe");

                string patch1 = "55 57 56 53 89 CE 83 EC 1C 8B 41 04 8B 58 08 8B";
                string patch2 = "89 34 24 89 D9 E8 58 ?? FE FF 83 EC 04";

                bool p1 = Patch.PatternExists(noxQt5Path, patch1);
                bool p2 = Patch.PatternExists(noxPath, patch2);
                if (p1 && p2)
                {
                    Log("Created backup on " + noxQt5BakPath);
                    File.Copy(noxQt5Path, noxQt5BakPath, true);
                    File.Copy(noxPath, noxBakPath, true);

                    if (Patch.PatchFile(noxQt5Path, patch1, "B8 01 00 00 00 C3"))
                        Log("Patched Qt5Widgets.dll");

                    if (Patch.PatchFile(noxPath, patch2, "90 90 90 90 90 90 90 90 90 90 90 90 90"))
                        Log("Patched Nox.exe");
                }
                else
                {
                    Log("Nothing to patch Nox Player. It might be already patched or the patterns could not be found");
                }

                //https://appcenter-api.bignox.com
                string patchads1 = "68 74 74 70 73 3A 2F 2F 61 70 70 63 65 6E 74 65 72 2D 61 70 69 2E 62 69 67 6E 6F 78 2E 63 6F 6D";
                //https://appcenter-api.yeshen.com
                string patchads2 = "68 74 74 70 73 3A 2F 2F 61 70 70 63 65 6E 74 65 72 2D 61 70 69 2E 79 65 73 68 65 6E 2E 63 6F 6D";
                //https://bi.noxgroup.com
                string patchads3 = "68 74 74 70 73 3A 2F 2F 62 69 2E 6E 6F 78 67 72 6F 75 70 2E 63 6F 6D";
                //https://api-new.bignox.com
                string patchads4 = "68 74 74 70 73 3A 2F 2F 61 70 69 2D 6E 65 77 2E 62 69 67 6E 6F 78 2E 63 6F 6D";

                bool pa1 = Patch.PatternExists(noxMultiPath, patchads1);
                bool pa2 = Patch.PatternExists(noxMultiPath, patchads2);
                bool pa3 = Patch.PatternExists(noxMultiPath, patchads3);
                bool pa4 = Patch.PatternExists(noxMultiPath, patchads4);
                if (pa1 || pa2 || pa3 || pa4)
                {
                    Log("Created backup on " + noxMultiBakPath);
                    File.Copy(noxMultiPath, noxMultiBakPath, true);

                    Patch.PatchFile(noxMultiPath, patchads1, "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                    Patch.PatchFile(noxMultiPath, patchads2, "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                    Patch.PatchFile(noxMultiPath, patchads3, "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                    Patch.PatchFile(noxMultiPath, patchads4, "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

                    Log("Patched MultiPlayerManager.exe");
                }
                else
                {
                    Log("Failed to patch MultiPlayerManager. It might be already patched or the patterns could not be found");
                    return;
                }
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
