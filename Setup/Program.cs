using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Installer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Récupérer les clés de registre
            RegistryKey keyUge = Registry.LocalMachine.OpenSubKey("Software\\GeneralsUltimateExperience");
            RegistryKey keyEaGames = Registry.LocalMachine.OpenSubKey("Software\\EA Games\\Command and Conquer Generals Zero Hour");
            RegistryKey keyGenerals = Registry.LocalMachine.OpenSubKey("Software\\Electronic Arts\\EA GAMES\\Generals");
            RegistryKey keyZeroHour = Registry.LocalMachine.OpenSubKey("Software\\Electronic Arts\\EA GAMES\\Command and Conquer Generals Zero Hour");

            // Traitement
            if (keyUge == null)
            {
                if (keyEaGames != null || keyGenerals != null || keyZeroHour != null)
                {
                    if (MessageBox.Show(string.Format("Generals et/ou HeureH sont déjà installés de manière conventionnelle !{0}{0}Merci de les désinstaller proprement via ajout/suppression de programme avant avant de procéder à l'installation de Generals Ultimate Experience.",
                        Environment.NewLine), "Jeu original déjà installé", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes) return;
                    if (MessageBox.Show(string.Format("Comment ça non ?!{0}{0}Vraiment tu devrais d'abord faire une déinstallation via l'outil proposé par Electronic Arts.",
                        Environment.NewLine), "Jeu original déjà installé", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) return;
                    if (MessageBox.Show(string.Format("OK j'en déduis que tu veux tenter l'installation malgré tout.{0}{0}Après ça ce sera le boxon dans ta base de registre et le désinstalleur Electronic Arts ne fonctionnera plus correctement. Generals Ultimate Experience devrait toutefois fonctionner normalement.{0}{0}Choisis OK pour installer, je t'aurai prévenu !",
                        Environment.NewLine), "Jeu original déjà installé", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel) return;
                }

                // Installation
                Application.Run(new FormInstall());
            }
            else
            {
                // Désinstallation
                if (args.Length <= 0)
                {
                    // On copie l'exécutable dans un dossier temporaire pour l'exécuter depuis là (car si on exécute celui du dossier d'installation la suppression échoue)
                    string tempFolderPath = Path.GetTempPath();
                    string executablePath = Process.GetCurrentProcess().MainModule.FileName;
                    string tempPath = Path.GetTempPath() + Path.GetFileName(executablePath);
                    if (File.Exists(tempPath))
                    {
                        try
                        {
                            File.Delete(tempPath);
                        }
                        catch
                        {
                            MessageBox.Show("Impossible de générer le fichier temporaire", "Désinstallation annulée", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    File.Copy(executablePath, tempPath);

                    Process process = new Process();
                    process.StartInfo.FileName = tempPath;
                    process.StartInfo.Verb = "runas";
                    process.StartInfo.Arguments = "-u " + Process.GetCurrentProcess().Id.ToString();
                    process.Start();
                }
                else
                {
                    if (args.Length != 2) throw new Exception("Mauvais appel, arguments inconnus");
                    if (args[0].Equals("-u"))
                    {
                        // Attendre la fin du process précédent
                        WaitForProcessExit(Int32.Parse(args[1]));

                        // Lancer le désinstalleur
                        Application.Run(new FormUninstall());

                        // Suppression du setup
                        DeleteSetup();
                    }
                    else if (args[0].Equals("-maj"))
                    {
                        // Attendre la fin du process précédent
                        WaitForProcessExit(Int32.Parse(args[1]));

                        // Lancer la mise à jour
                        Application.Run(new FormUpdate(keyUge));

                        // Suppression du setup
                        DeleteSetup();
                    }
                    else if (args[0].Equals("-serial"))
                    {
                        string serialGenerals = args[1].Split(';')[0];
                        string serialHeureH = args[1].Split(';')[1];

                        RegistryKey keySerialGenerals = Registry.LocalMachine.OpenSubKey("Software\\Electronic Arts\\EA GAMES\\Generals\\ergc", true);
                        RegistryKey keySerialZeroHour = Registry.LocalMachine.OpenSubKey("Software\\Electronic Arts\\EA GAMES\\Command and Conquer Generals Zero Hour\\ergc", true);

                        keySerialGenerals.SetValue("", serialGenerals);
                        keySerialZeroHour.SetValue("", serialHeureH);
                    }
                    else
                    {
                        throw new Exception("Mauvais appel, arguments inconnus");
                    }
                }
            }
        }

        private static void WaitForProcessExit(int processId)
        {
            Process previousProcess = GetProcessByID(processId);
            if (previousProcess != null) previousProcess.WaitForExit();
        }

        private static Process GetProcessByID(int id)
        {
            Process[] processlist = Process.GetProcesses();
            return processlist.FirstOrDefault(pr => pr.Id == id);
        }

        private static void DeleteSetup()
        {
            // Suppression du setup (petit hack car c'est l'exécutable en cours d'exécution).
            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = "/C choice /C Y /N /D Y /T 3 & Del " + Application.ExecutablePath;
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
        }
    }
}