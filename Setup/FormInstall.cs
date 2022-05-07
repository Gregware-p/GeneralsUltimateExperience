using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Generic;
using System.Security.Principal;
using System.Security.AccessControl;
using Microsoft.Win32;

namespace Installer
{
    public partial class FormInstall : Form
    {
        private const long TOTAL_SIZE = 47285244121;
        private const long TOTAL_SIZE_COMPRESSE = 20555309319;
        private const long SIZE_MARGE = 1048576000; // 1 Go
        private const string DOWNLOADFILE_URL = "http://gregware.internet-box.ch/GeneralsUltimateExperience/";
        private readonly List<string> FILES = new List<string>
        {
            "GueBackgroundInstall.part01.exe",
            "GueBackgroundInstall.part02.rar",
            "GueBackgroundInstall.part03.rar",
            "GueBackgroundInstall.part04.rar",
            "GueBackgroundInstall.part05.rar"
        };

        public const string UNINSTALL_REG_KEY_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        private enum Etat { PasDemarre, Download, EchecDownload, Copie, EchecCopie, Annulation, Finalisation, Termine }

        private Etat _etat = Etat.PasDemarre;
        private readonly TaskScheduler _uiScheduler;
        private Process _process = null;
        private string _installPath;
        private string _executingPath;
        private object _lockAnnulation = new object();
        private List<string> _filesToDownload;
        private string _currentDownload;
        private int _nbDownloads;
        private int _nbDownloadsCompleted;

        public FormInstall()
        {
            InitializeComponent();

            // Set task scheduler
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            textBoxInstallpath.Text = GetProgramFilesx86Path() + "\\GeneralsUltimateExperience";
            progressBar.Maximum = 1000;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonInstall_Click(object sender, EventArgs e)
        {
            // Initialisations
            _executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Désactiver les contrôles
            SetActivationDesControles(false);

            // Vérifications
            _installPath = textBoxInstallpath.Text;
            if (!Path.IsPathRooted(_installPath))
            {
                MessageBox.Show("Le dossier d'installation n'est pas valide.", "Dossier d'installation invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetActivationDesControles(true);
                textBoxInstallpath.Focus();
                return;
            }

            textBoxSerialGenerals.Text = textBoxSerialGenerals.Text.Trim();
            textBoxSerialHeureH.Text = textBoxSerialHeureH.Text.Trim();

            string serialGenerals = textBoxSerialGenerals.Text;
            string[] splittedSerial = serialGenerals.Split('-');
            if (splittedSerial.Length != 4 ||
                splittedSerial[0].Length != 4 ||
                splittedSerial[1].Length != 7 ||
                splittedSerial[2].Length != 7 ||
                splittedSerial[3].Length != 4)
            {
                MessageBox.Show("Le n° de série Generals doit être sous la forme XXXX-XXXXXXX-XXXXXXX-XXXX", "N° de série Generals invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetActivationDesControles(true);
                textBoxSerialGenerals.Focus();
                return;
            }
            if (!IsDigitsOnly(splittedSerial))
            {
                MessageBox.Show("Le n° de série Generals n'est pas valable", "N° de série Generals invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetActivationDesControles(true);
                textBoxSerialGenerals.Focus();
                return;
            }

            string serialHeureH = textBoxSerialHeureH.Text;
            splittedSerial = serialHeureH.Split('-');
            if (splittedSerial.Length != 5 ||
                splittedSerial[0].Length != 4 ||
                splittedSerial[1].Length != 4 ||
                splittedSerial[2].Length != 4 ||
                splittedSerial[3].Length != 4 ||
                splittedSerial[4].Length != 4)
            {
                MessageBox.Show("Le n° de série HeureH doit être sous la forme XXXX-XXXX-XXXX-XXXX-XXXX", "N° de série HeureH invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetActivationDesControles(true);
                textBoxSerialHeureH.Focus();
                return;
            }

            // Créer le répertoire s'il n'existe pas
            try
            {
                Directory.CreateDirectory(_installPath);
            }
            catch (Exception)
            {
                MessageBox.Show("Le dossier d'installation n'est pas valide ou vous n'avez pas les droits pour créer le dossier.", "Dossier d'installation invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetActivationDesControles(true);
                textBoxInstallpath.Focus();
                return;
            }

            // Vérifier que le répertoire d'installation est vide - sinon l'annulation de l'installation serait impossible
            if (Directory.EnumerateFileSystemEntries(_installPath).Any())
            {
                MessageBox.Show("Le dossier d'installation n'est pas vide.", "Dossier d'installation invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetActivationDesControles(true);
                textBoxInstallpath.Focus();
                return;
            }

            // Vérifier l'espace disponible
            string tempFolderPath = Path.GetTempPath();
            if (Path.GetPathRoot(tempFolderPath).Equals(Path.GetPathRoot(_installPath)))
            {
                // Installation sur le même drive que le dossier temporaire, il faut cumuler les besoins
                long freeSpace = new DriveInfo(new FileInfo(_installPath).Directory.Root.FullName).AvailableFreeSpace;
                if (freeSpace < TOTAL_SIZE + TOTAL_SIZE_COMPRESSE + SIZE_MARGE)
                {
                    MessageBox.Show(string.Format("Le lecteur désigné n'a pas suffisamment d'espace disponible. Un minimum de {0} Go est requis pour l'installation. Certains fichiers sont effacés à l'issue de l'installation et au final le programme prendra {1} Go sur le disque.", (TOTAL_SIZE + TOTAL_SIZE_COMPRESSE + SIZE_MARGE) >> 30, (TOTAL_SIZE + SIZE_MARGE) >> 30),
                        "Dossier d'installation invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetActivationDesControles(true);
                    textBoxInstallpath.Focus();
                    return;
                }
            }
            else
            {
                // installation sur drive séparés, besoins séparés

                // espace sur drive d'installation
                long freeSpace = new DriveInfo(new FileInfo(_installPath).Directory.Root.FullName).AvailableFreeSpace;
                if (freeSpace < TOTAL_SIZE + SIZE_MARGE)
                {
                    MessageBox.Show(string.Format("Le lecteur désigné n'a pas suffisamment d'espace disponible. Un minimum de {0} Go est requis.", (TOTAL_SIZE + SIZE_MARGE) >> 30),
                        "Dossier d'installation invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetActivationDesControles(true);
                    textBoxInstallpath.Focus();
                    return;
                }

                // espace sur dossier temporaire
                freeSpace = new DriveInfo(new FileInfo(tempFolderPath).Directory.Root.FullName).AvailableFreeSpace;
                if (freeSpace < TOTAL_SIZE_COMPRESSE + SIZE_MARGE)
                {
                    MessageBox.Show(string.Format("Le lecteur {0} n'a pas suffisamment d'espace disponible. L'installation a besoin d'espace sur ce disque pour les exctractions temporaires. Ces données sont supprimées à la fin de l'installation. Un minimum de {1} Go est requis.",
                        Path.GetFileName(new FileInfo(tempFolderPath).FullName),
                        (TOTAL_SIZE_COMPRESSE + SIZE_MARGE) >> 30),
                        "Dossier d'installation invalide", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetActivationDesControles(true);
                    textBoxInstallpath.Focus();
                    return;
                }
            }

            // Télécharger les fichiers
            labelAvancement.Visible = true;
            _etat = Etat.Download;
            _filesToDownload = new List<string>();
            foreach (string s in FILES) _filesToDownload.Add(s);
            _nbDownloads = _filesToDownload.Count;
            _nbDownloadsCompleted = 0;
            RunNextDownload();
        }

        private void RunNextDownload()
        {
            // Terminer si nécessaire
            if (_filesToDownload.Count() <= 0)
            {
                Decompress();
                return;
            }

            // Prendre le prochain
            _currentDownload = _filesToDownload.First();
            _filesToDownload.Remove(_currentDownload);

            // Télécharger la mise à jour
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new System.Uri(DOWNLOADFILE_URL + _currentDownload), _executingPath + "\\" + _currentDownload);
            }
        }

        private void Decompress()
        {
            if (_etat != Etat.Download) return;

            // Décompresser les fichiers
            labelAvancement.Text = "Installation (étape 2/2)";
            progressBar.Value = 0;
            _etat = Etat.Copie;
            _process = new Process();
            _process.StartInfo.FileName = _executingPath + "\\GueBackgroundInstall.part01.exe";
            _process.StartInfo.Verb = "runas";
            Directory.SetCurrentDirectory(_installPath);
            _process.EnableRaisingEvents = true;
            _process.Exited += Process_Exited;
            _process.Start();

            // Mettre à jour la progress bar
            // Créer un thread pour éviter les problèmes UI
            Task.Factory.StartNew(() =>
            {
                DirectoryInfo di = new DirectoryInfo(_installPath);
                while (_etat != Etat.Finalisation && _etat != Etat.Annulation && _etat != Etat.EchecCopie)
                {
                    long actualSize = DirSize(di);
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            progressBar.Value = Convert.ToInt32(Decimal.Divide(actualSize, TOTAL_SIZE) * 1000);
                        }
                        catch (Exception)
                        {
                            // Semble improbable mais en cas de problème ne pas planter le programme pour une question de progressBar
                        }
                    }, CancellationToken.None, TaskCreationOptions.None, _uiScheduler);
                    Thread.Sleep(1000);
                }
            }).ContinueWith(antecedent =>
            {
                lock (_lockAnnulation)
                {
                    if (_etat == Etat.EchecCopie)
                    {
                        // Le processus a terminé en échec, qqch ne joue pas on annule tout
                        MessageBox.Show("Échec de la décompression de fichier. Le programme va maintenant supprimer les fichiers déjà copiés puis se terminer.", "Serious error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        // Supprimer le dossier d'installation
                        DirectoryInfo di = new DirectoryInfo(_installPath);
                        SetAttributesNormal(di);
                        di.Delete(true);

                        // Quitter le programme
                        Close();
                    }
                    else if (_etat != Etat.Annulation)
                    {
                        // L'installation ne peut plus être annulée
                        buttonCancel.Enabled = false;

                        // Donner suffisamment de droits au fichiers copiés
                        SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                        DirectorySecurity secUserFolderInstall = Directory.GetAccessControl(_installPath);
                        secUserFolderInstall.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                        Directory.SetAccessControl(_installPath, secUserFolderInstall);

                        // Désctiver l'indexation pour éviter les serious error (http://forums.swr-productions.com/index.php?showtopic=5964)
                        foreach (string path in Directory.GetFiles(_installPath, "*", SearchOption.AllDirectories))
                        {
                            File.SetAttributes(path, FileAttributes.NotContentIndexed);
                        }
                        foreach (string path in Directory.GetDirectories(_installPath, "*", SearchOption.AllDirectories))
                        {
                            File.SetAttributes(path, FileAttributes.NotContentIndexed);
                        }

                        // Registre : Generals Ultimate Experience
                        RegistryKey keySoftware = Registry.LocalMachine.OpenSubKey("Software", true);
                        keySoftware.CreateSubKey("GeneralsUltimateExperience");
                        RegistryKey keyUge = keySoftware.OpenSubKey("GeneralsUltimateExperience", true);
                        keyUge.SetValue("InstallPath", _installPath);
                        keyUge.SetValue("MajorVersion", 1);
                        keyUge.SetValue("MinorVersion", 0);

                        // Registre : Electronic Arts
                        RegistryKey keyEaGames = keySoftware.OpenSubKey("EA Games", true);
                        if (keyEaGames == null)
                        {
                            keySoftware.CreateSubKey("EA Games");
                            keyEaGames = keySoftware.OpenSubKey("EA Games", true);
                        }
                        if (keyEaGames.OpenSubKey("Command and Conquer Generals Zero Hour") != null) keyEaGames.DeleteSubKeyTree("Command and Conquer Generals Zero Hour");
                        keyEaGames.CreateSubKey("Command and Conquer Generals Zero Hour");
                        RegistryKey keyEaGamesZeroHour = keyEaGames.OpenSubKey("Command and Conquer Generals Zero Hour", true);
                        keyEaGamesZeroHour.SetValue("DisplayName", "Command and Conquer Generals - Heure H");

                        RegistryKey keyElectronicArts = keySoftware.OpenSubKey("Electronic Arts", true);
                        if (keyElectronicArts == null)
                        {
                            keySoftware.CreateSubKey("Electronic Arts");
                            keyElectronicArts = keySoftware.OpenSubKey("Electronic Arts", true);
                        }
                        RegistryKey keyElectronicArtsEaGames = keyElectronicArts.OpenSubKey("EA GAMES", true);
                        if (keyElectronicArtsEaGames == null)
                        {
                            keyElectronicArts.CreateSubKey("EA GAMES");
                            keyElectronicArtsEaGames = keyElectronicArts.OpenSubKey("EA GAMES", true);
                        }

                        if (keyElectronicArtsEaGames.OpenSubKey("Command and Conquer Generals Zero Hour") != null) keyEaGames.DeleteSubKeyTree("Command and Conquer Generals Zero Hour");
                        keyElectronicArtsEaGames.CreateSubKey("Command and Conquer Generals Zero Hour");
                        RegistryKey keyElectronicArtsEaGamesZeroHour = keyElectronicArtsEaGames.OpenSubKey("Command and Conquer Generals Zero Hour", true);
                        keyElectronicArtsEaGamesZeroHour.SetValue("InstallPath", _installPath + "\\Games\\HeureH");
                        keyElectronicArtsEaGamesZeroHour.SetValue("Language", "french");
                        keyElectronicArtsEaGamesZeroHour.SetValue("MapPackVersion", 65536);
                        keyElectronicArtsEaGamesZeroHour.SetValue("UserDataLeafName", "Command & Conquer Generals - Heure H Data");
                        keyElectronicArtsEaGamesZeroHour.SetValue("Version", 65536);
                        keyElectronicArtsEaGamesZeroHour.CreateSubKey("ergc");
                        RegistryKey keyElectronicArtsEaGamesZeroHourErgc = keyElectronicArtsEaGamesZeroHour.OpenSubKey("ergc", true);
                        keyElectronicArtsEaGamesZeroHourErgc.SetValue("", textBoxSerialHeureH.Text.Replace("-", string.Empty));

                        if (keyElectronicArtsEaGames.OpenSubKey("Generals") != null) keyEaGames.DeleteSubKeyTree("Generals");
                        keyElectronicArtsEaGames.CreateSubKey("Generals");
                        RegistryKey keyElectronicArtsEaGamesGenerals = keyElectronicArtsEaGames.OpenSubKey("Generals", true);
                        keyElectronicArtsEaGamesGenerals.SetValue("InstallPath", _installPath + "\\Games\\Generals\\");
                        keyElectronicArtsEaGamesGenerals.SetValue("Language", "french");
                        keyElectronicArtsEaGamesGenerals.SetValue("MapPackVersion", 65536);
                        keyElectronicArtsEaGamesGenerals.SetValue("Version", 65536);
                        keyElectronicArtsEaGamesGenerals.CreateSubKey("ergc");
                        RegistryKey keyElectronicArtsEaGamesGeneralsErgc = keyElectronicArtsEaGamesGenerals.OpenSubKey("ergc", true);
                        keyElectronicArtsEaGamesGeneralsErgc.SetValue("", textBoxSerialGenerals.Text.Replace("-", string.Empty));

                        // Ajout de la police
                        Setup.FontTool.RegisterFont("PLCC____.TTF");

                        // Registre : Ajout/Suppression programmes Windows
                        CreateUninstaller();

                        // Ajouter les dossiers dans documents et donner suffisamment de droits
                        string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        string generalsUserFoldePath = string.Format("{0}\\{1}", myDocumentPath, "Command and Conquer Generals Data");
                        string zeroHoursUserFoldePath = string.Format("{0}\\{1}", myDocumentPath, "Command & Conquer Generals - Heure H Data");
                        CreateFolderAllRight(generalsUserFoldePath);
                        CreateFolderAllRight(zeroHoursUserFoldePath);
                        CreateFolderAllRight(generalsUserFoldePath + "\\MapPreviews");
                        CreateFolderAllRight(zeroHoursUserFoldePath + "\\MapPreviews");
                        CreateFolderAllRight(generalsUserFoldePath + "\\Maps");
                        CreateFolderAllRight(zeroHoursUserFoldePath + "\\Maps");
                        CreateFolderAllRight(generalsUserFoldePath + "\\Replays");
                        CreateFolderAllRight(zeroHoursUserFoldePath + "\\Replays");
                        CreateFolderAllRight(generalsUserFoldePath + "\\Save");
                        CreateFolderAllRight(zeroHoursUserFoldePath + "\\Save");

                        // Ajouter l'option.ini
                        WriteIniFile(File.CreateText(generalsUserFoldePath + "\\Options.ini"));
                        WriteIniFile(File.CreateText(zeroHoursUserFoldePath + "\\Options.ini"));

                        // Ajouter le fichier replay en readonly pour éviter les problèmes de lag (http://www.cnclabs.com/forums/cnc_postst15317_Horrible-Lag-on-Network-play.aspx)
                        File.Create(generalsUserFoldePath + "\\Replays\\00000000.rep").Dispose();
                        File.SetAttributes(generalsUserFoldePath + "\\Replays\\00000000.rep", FileAttributes.ReadOnly);
                        File.Create(zeroHoursUserFoldePath + "\\Replays\\00000000.rep").Dispose();
                        File.SetAttributes(zeroHoursUserFoldePath + "\\Replays\\00000000.rep", FileAttributes.ReadOnly);

                        // Ajouter un raccourci sur le bureau
                        string link = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + Path.DirectorySeparatorChar + "Generals Ultimate Experience.lnk";
                        var shell = new IWshRuntimeLibrary.WshShell();
                        var shortcut = shell.CreateShortcut(link) as IWshRuntimeLibrary.IWshShortcut;
                        shortcut.TargetPath = _installPath + "\\GeneralsUltimateExperience.exe";
                        shortcut.WorkingDirectory = _installPath;
                        shortcut.Save();

                        // S'assurer que la progress bar est au max
                        progressBar.Value = 1000;

                        // Fin
                        _etat = Etat.Termine;
                        MessageBox.Show(string.Format("L'installation est un succès :-){0}{0}Un raccourci a été placé sur le bureau. Merci d'avoir choisi Gregware.", Environment.NewLine), "Installation terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Close();
                    }
                }
            }, _uiScheduler);
        }

        private void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // Erreur
            if(e.Error != null)
            {
                _etat = Etat.EchecDownload;
                MessageBox.Show("Échec du téléchargement de fichier : vérifie ta connexion internet, essaie plus tard ou contacte Gregware.", "Serious error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Supprimer le dossier d'installation
                DirectoryInfo di = new DirectoryInfo(_installPath);
                SetAttributesNormal(di);
                di.Delete(true);

                // Quitter le programme
                Close();
                return;
            }

            // Succès
            _nbDownloadsCompleted++;
            RunNextDownload();
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = _nbDownloadsCompleted * (1000 / _nbDownloads) + Convert.ToInt32((e.ProgressPercentage / 100.0 * (1000.0 / _nbDownloads)));
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            // Revenir au répertoire courant de l'exécutable (pour pouvoir permettre suppression si nécessaire)
            Directory.SetCurrentDirectory(_executingPath);

            // Se placer en finalisation
            lock (_lockAnnulation)
            {
                if (_etat != Etat.Annulation)
                {
                    if (_process.ExitCode == 0) _etat = Etat.Finalisation;
                    else _etat = Etat.EchecCopie;
                }
            }

        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK) textBoxInstallpath.Text = fbd.SelectedPath;
        }

        private static string GetProgramFilesx86Path()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        private bool IsDigitsOnly(string[] strTab)
        {
            foreach (string str in strTab)
            {
                foreach (char c in str)
                {
                    if (c < '0' || c > '9') return false;
                }
            }
            return true;
        }

        private void SetActivationDesControles(bool enabled)
        {
            textBoxSerialGenerals.Enabled = enabled;
            textBoxSerialHeureH.Enabled = enabled;
            textBoxInstallpath.Enabled = enabled;
            buttonBrowse.Enabled = enabled;
            buttonInstall.Enabled = enabled;
        }

        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (_lockAnnulation)
            {
                if (_etat == Etat.Finalisation)
                {
                    // Interdire (silencieusement) pendant la finalisation
                    e.Cancel = true;
                    return;
                }
                else if (_etat == Etat.Download)
                {
                    if (MessageBox.Show("Es-tu sûr de vouloir annuler l'installation ?", "Annuler l'installation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _etat = Etat.Annulation;
                        buttonCancel.Enabled = false;
                    }
                    else
                    {
                        // Commande de sortie annulée par l'utilisateur
                        e.Cancel = true;
                        return;
                    }
                }
                else if (_etat == Etat.Copie)
                {
                    // Considérer comme annulation lors de la copie
                    if (MessageBox.Show("Es-tu sûr de vouloir annuler l'installation ? Les fichiers déjà copiés seront supprimés.", "Annuler l'installation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        _etat = Etat.Annulation;
                        buttonCancel.Enabled = false;
                        if (_process != null)
                        {
                            // Tuer le process de copie si nécessaire
                            if (!_process.HasExited)
                            {
                                _process.Kill();
                                _process.WaitForExit();
                            }

                            // Supprimer le dossier d'installation
                            DirectoryInfo di = new DirectoryInfo(_installPath);
                            SetAttributesNormal(di);
                            di.Delete(true);
                        }
                        else
                        {
                            MessageBox.Show("La copie en cours n'a pas pu être interrompue correctement. Supprime manuellement le dossier d'installation s'il existe.", "Échec de l'annulation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        // Commande de sortie annulée par l'utilisateur
                        e.Cancel = true;
                        return;
                    }
                }
            }

            // Dans les autre cas autoriser la sortie
            e.Cancel = false;
        }

        public static void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                SetAttributesNormal(subDir);
            }
            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }

        // Attention en cas de modif : il y a son frère jumeau dans l'autre projet !!!
        private void WriteIniFile(StreamWriter sw)
        {
            // Trouver la résolution optimale (native si <= 1440p, 1080p sinon)
            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            int width = resolution.Width;
            int height = resolution.Height;
            if (width > 2560)
            {
                width = 1920;
                height = 1080;
            }

            // écrire le fichier INI
            sw.WriteLine("AntiAliasing = 1");
            sw.WriteLine("BuildingOcclusion = yes");
            sw.WriteLine("DynamicLOD = no");
            sw.WriteLine("ExtraAnimations = yes");
            sw.WriteLine("GameSpyIPAddress = 0.0.0.0");
            sw.WriteLine("Gamma = 50");
            sw.WriteLine("HeatEffects = yes");
            sw.WriteLine("IPAddress = 0.0.0.0");
            sw.WriteLine("IdealStaticGameLOD = High");
            sw.WriteLine("LanguageFilter = true");
            sw.WriteLine("MaxParticleCount = 5000");
            sw.WriteLine("MusicVolume = 50");
            sw.WriteLine(string.Format("Resolution = {0} {1}", width, height));
            sw.WriteLine("Retaliation = yes");
            sw.WriteLine("SFX3DVolume = 66");
            sw.WriteLine("SFXVolume = 60");
            sw.WriteLine("SawTOS = yes");
            sw.WriteLine("ScrollFactor = 30");
            sw.WriteLine("SendDelay = no");
            sw.WriteLine("ShowSoftWaterEdge = yes");
            sw.WriteLine("ShowTrees = yes");
            sw.WriteLine("StaticGameLOD = Custom");
            sw.WriteLine("TextureReduction = 0");
            sw.WriteLine("UseAlternateMouse = no");
            sw.WriteLine("UseCloudMap = yes");
            sw.WriteLine("UseDoubleClickAttackMove = no");
            sw.WriteLine("UseLightMap = yes");
            sw.WriteLine("UseShadowDecals = yes");
            sw.WriteLine("UseShadowVolumes = no");
            sw.WriteLine("VoiceVolume = 60");
            sw.Close();
        }

        private void CreateUninstaller()
        {
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(UNINSTALL_REG_KEY_PATH, true))
            {
                if (parent == null)
                {
                    // Ne pas embrouiller l'utilisateur avec ça, la seule conséquence c'est que la désinstallation ne sera pas disponible depuis le panneau de configuration
                    return;
                }
                try
                {
                    RegistryKey key = null;

                    try
                    {
                        string guidText = new Guid("f416dd93-d270-4bff-aa66-c1ce6bbf95a8").ToString("B");
                        key = parent.OpenSubKey(guidText, true) ?? parent.CreateSubKey(guidText);

                        if (key == null)
                        {
                            throw new Exception(String.Format("Unable to create uninstaller '{0}\\{1}'", UNINSTALL_REG_KEY_PATH, guidText));
                        }

                        Assembly asm = GetType().Assembly;
                        Version v = asm.GetName().Version;
                        string exe = "\"" + _installPath + "\\setup.exe" + "\"";

                        key.SetValue("DisplayName", "Generals Ultimate Experience");
                        key.SetValue("Publisher", "Gregware");
                        key.SetValue("DisplayIcon", exe);
                        key.SetValue("DisplayVersion", v.ToString(2));
                        key.SetValue("InstallDate", DateTime.Now.ToString("dd.MM.yyyy"));
                        key.SetValue("UninstallString", exe);
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                }
                catch (Exception)
                {
                    // Ne pas embrouiller l'utilisateur avec ça, la seule conséquence c'est que la désinstallation ne sera pas disponible depuis le panneau de configuration
                }
            }
        }

        private void CreateFolderAllRight(string path)
        {
            SecurityIdentifier everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            Directory.CreateDirectory(path);
            DirectorySecurity secUserFolder = Directory.GetAccessControl(path);
            secUserFolder.AddAccessRule(new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            Directory.SetAccessControl(path, secUserFolder);
        }
    }
}