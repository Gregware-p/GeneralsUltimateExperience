using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Installer
{
    public partial class FormUpdate : Form
    {
        #region Constantes
        private const string UPDATE_SERVICE_URL = "http://gregware.internet-box.ch/GeneralsUltimateExperience";
        private const long SIZE_MARGE = 1048576000; // 1 Go
        #endregion

        #region Enum, structs
        private enum Etat { PasDemarre, EnCours, Suivant, EnErreur, Termine }

        private struct Version
        {
            public int Major;
            public int Minor;
        }

        private struct UpdateItem
        {
            public Version Version;
            public long Size;
            public string Filename;
            public string ChangeLog;
            public UpdateDelete Delete;
        }

        private struct UpdateDelete
        {
            public List<string> Files;
            public List<string> Folders;
        }
        #endregion

        #region variables
        private Version _localVersion = new Version();
        private List<UpdateItem> _remoteUpdateList = new List<UpdateItem>();
        private Etat _etat = Etat.PasDemarre;
        private readonly TaskScheduler _uiScheduler;
        private Process _process = null;
        private string _installPath;
        private string _executingPath;
        private List<UpdateItem> _updateListeUtile;
        private int _nbUpdates;
        private int _updateiterator = 0;
        private UpdateItem _currentUpdate;
        private long _sizeBefore;
        #endregion

        public FormUpdate(RegistryKey keyUge)
        {
            // Initialisations
            _executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _installPath = (string)keyUge.GetValue("InstallPath");
            if (_installPath == null) throw new Exception("L'installation est corrompue");
            InitializeComponent();
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            progressBar.Maximum = 1000;
            GetRemoteVersion();
            _localVersion.Major = (int)keyUge.GetValue("MajorVersion");
            _localVersion.Minor = (int)keyUge.GetValue("MinorVersion");
        }

        private void FormUpdate_Shown(object sender, EventArgs e)
        {
            // Récupérer que les éléments utile de la liste
            _updateListeUtile = _remoteUpdateList.Where(
                u => u.Version.Major > _localVersion.Major || (u.Version.Major == _localVersion.Major && u.Version.Minor > _localVersion.Minor)).OrderBy(
                u => u.Version.Major).ThenBy(
                u => u.Version.Minor).ToList();
            _nbUpdates = _updateListeUtile.Count();
            if (_nbUpdates <= 0)
            {
                MessageBox.Show("Tu as déjà la version la plus récente", "Mise à jour annulée", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            // Obtenir l'espace requis
            long totalsize = _updateListeUtile.Sum(u => u.Size);

            // Vérifier l'espace disponible
            long freeSpace = new DriveInfo(new FileInfo(_installPath).Directory.Root.FullName).AvailableFreeSpace;
            if (freeSpace < totalsize + SIZE_MARGE)
            {
                MessageBox.Show(string.Format("Pas assez d'espace disponible. Un minimum de {0} Go est requis.", (totalsize + SIZE_MARGE) >> 30),
                    "Mise à jour annulée", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            // Appliquer le(s) mise(s) à jour
            _etat = Etat.EnCours;
            RunNextUpdate();
        }

        private void RunNextUpdate()
        {
            // Vider le dossier update (même si ça aurait déjà dû être fait, permet d'éviter les problèmes en cas de récupération après crash)
            try
            {
                DirectoryInfo di = new DirectoryInfo(_installPath + "\\Update");
                FormInstall.SetAttributesNormal(di);
                di.Delete(true);
                Directory.CreateDirectory(_installPath + "\\Update");
            }
            catch (Exception)
            {
                _etat = Etat.EnErreur;
                MessageBox.Show("Problème d'accès au dossier de mise à jour :-(",
                    "Serious error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            // Terminer si nécessaire
            if (_updateListeUtile.Count() <= 0)
            {
                Terminate();
                return;
            }

            // Prendre le prochain
            labelSubtitle.Visible = false;
            labelPoint1.Visible = false;
            labelPoint2.Visible = false;
            labelPoint3.Visible = false;
            labelPoint4.Visible = false;
            labelPoint5.Visible = false;
            _currentUpdate = _updateListeUtile.First();
            _updateListeUtile.Remove(_currentUpdate);
            labelSubtitle.Text = string.Format("Mise à jour version {0}.{1} => {2}.{3}", _localVersion.Major, _localVersion.Minor, _currentUpdate.Version.Major, _currentUpdate.Version.Minor);
            labelSubtitle.Visible = true;
            _updateiterator++;

            // Télécharger la mise à jour
            labelPoint1.Visible = true;
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new System.Uri(UPDATE_SERVICE_URL + "/Updates/" + _currentUpdate.Filename), _installPath + "\\Update\\" + _currentUpdate.Filename);
            }
        }

        private void Terminate(bool error = false)
        {
            // Désctiver l'indexation pour éviter les serious error (http://forums.swr-productions.com/index.php?showtopic=5964)
            foreach (string path in Directory.GetFiles(_installPath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(path, FileAttributes.NotContentIndexed);
            }
            foreach (string path in Directory.GetDirectories(_installPath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(path, FileAttributes.NotContentIndexed);
            }

            // Fin
            _etat = Etat.Termine;
            if (error)
            {
                MessageBox.Show(string.Format("Echec du téléchargement de la mise à jour : vérifie ta connexion internet, essaie plus tard ou contacte Gregware.", Environment.NewLine), "Mise à jour échouée", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                labelPoint5.Visible = true;
                progressBar.Value = 1000;
                MessageBox.Show(string.Format("La mise à jour est un succès :-){0}{0}Merci d'avoir choisi Gregware.", Environment.NewLine), "Mise à jour terminée", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            // Relancer le prog
            Process process = new Process();
            process.StartInfo.FileName = _installPath + "\\GeneralsUltimateExperience.exe";
            process.Start();

            // Fermer celui là
            Close();
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // Erreur
            if(e.Error != null)
            {
                Terminate(true);
                return;
            }

            // Suppression des fichiers obsolètes
            labelPoint2.Visible = true;
            foreach (string folderPath in _currentUpdate.Delete.Folders)
            {
                string fullPath = _installPath + "\\" + folderPath;
                if (Directory.Exists(fullPath))
                {
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(fullPath);
                        FormInstall.SetAttributesNormal(di);
                        di.Delete(true);
                    }
                    catch (Exception)
                    {
                        _etat = Etat.EnErreur;
                        MessageBox.Show("Impossible de supprimer le dossier. L'installation est corrompue, veuillez réinstaller :-(", "Serious error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;
                    }
                }
            }
            foreach (string filePath in _currentUpdate.Delete.Files)
            {
                string fullPath = _installPath + "\\" + filePath;
                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch (Exception)
                    {
                        _etat = Etat.EnErreur;
                        MessageBox.Show("Impossible de supprimer le fichier. L'installation est corrompue, veuillez réinstaller :-(", "Serious error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                        return;
                    }
                }
            }

            // Mettre à jour la progress bar
            // Créer un thread pour éviter les problèmes UI
            DirectoryInfo di2 = new DirectoryInfo(_installPath);
            _sizeBefore = FormInstall.DirSize(di2);
            Task.Factory.StartNew(() =>
            {
                // On attend la fin de la décompression
                while (_etat != Etat.Suivant && _etat != Etat.EnErreur)
                {
                    long actualSize = FormInstall.DirSize(di2) - _sizeBefore;
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            int partAcquise = (1000 / _nbUpdates) * (_updateiterator - 1);
                            partAcquise += 1000 / _nbUpdates / 2;
                            progressBar.Value = partAcquise + (Convert.ToInt32(Decimal.Divide(actualSize, _currentUpdate.Size) * 1000) / 2 / _nbUpdates);
                        }
                        catch (Exception)
                        {
                            // Semble imporbable mais en cas de problème ne pas planter le programme pour une question de progressBar
                        }
                    }, CancellationToken.None, TaskCreationOptions.None, _uiScheduler);
                    Thread.Sleep(200);
                }
            }).ContinueWith(antecedent =>
            { 
                // Gestion des erreurs
                if (_etat == Etat.EnErreur)
                {
                    MessageBox.Show("Erreur lors de la décompression des fichiers. L'installation est corrompue, veuillez réinstaller :-(", "Serious error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                }
                else
                {
                    // On passe à la suite
                    _etat = Etat.EnCours;

                    // Mise à jour du registre
                    labelPoint4.Visible = true;
                    RegistryKey keyUge = Registry.LocalMachine.OpenSubKey("Software\\GeneralsUltimateExperience", true);
                    keyUge.SetValue("MajorVersion", _currentUpdate.Version.Major);
                    keyUge.SetValue("MinorVersion", _currentUpdate.Version.Minor);

                    // Vider le dossier update
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(_installPath + "\\Update");
                        FormInstall.SetAttributesNormal(di);
                        di.Delete(true);
                        Directory.CreateDirectory(_installPath + "\\Update");
                    }
                    catch (Exception)
                    {
                        _etat = Etat.EnErreur;
                        MessageBox.Show("Problème d'accès au dossier de mise à jour. L'installation est corrompue, veuillez réinstaller :-(",
                                "Serious error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                    }

                    if (_etat != Etat.EnErreur)
                    {
                        // Revenir au répertoire courant de l'exécutable (pour pouvoir permettre suppression si nécessaire)
                        Directory.SetCurrentDirectory(_executingPath);

                        // Lancer la prochaine update
                        RunNextUpdate();
                    }
                }
            }, _uiScheduler);

            // Décompression des nouveaux fichiers
            labelPoint3.Visible = true;
            _process = new Process();
            _process.StartInfo.FileName = _installPath + "\\Update\\" + _currentUpdate.Filename;
            _process.StartInfo.Verb = "runas";
            Directory.SetCurrentDirectory(_installPath);
            _process.EnableRaisingEvents = true;
            _process.Exited += Process_Exited;
            _process.Start();
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                int partAcquise = (1000 / _nbUpdates) * (_updateiterator - 1);
                progressBar.Value = partAcquise + ((e.ProgressPercentage * 10) / 2 / _nbUpdates);
            }
            catch (Exception)
            {
                // Semble imporbable mais en cas de problème ne pas planter le programme pour une question de progressBar
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            if (_process.ExitCode != 0) _etat = Etat.EnErreur;
            else _etat = Etat.Suivant;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Interdire (silencieusement) pendant la mise à jour
            if (_etat == Etat.EnCours || _etat == Etat.Suivant)
            {
                e.Cancel = true;
                return;
            }

            // Dans les autre cas autoriser la sortie
            e.Cancel = false;
        }

        private void GetRemoteVersion()
        {
            // Obtenir le fichier xml de la part du server
            string content;
            using (WebClient webClient = new WebClient())
            {
                content = webClient.DownloadString(UPDATE_SERVICE_URL + "/Updates.xml");
            }

            // Décoder le fichier xml
            List<UpdateItem> updateList = new List<UpdateItem>();
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(new StringReader(content), readerSettings))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(reader);
                foreach (XmlNode updateNode in xml.DocumentElement.ChildNodes[0].ChildNodes)
                {
                    UpdateItem update = new UpdateItem();
                    string versionString = updateNode.Attributes["Version"].Value;
                    update.Version = new Version { Major = Int32.Parse(versionString.Split('.')[0]), Minor = Int32.Parse(versionString.Split('.')[1]) };
                    update.Filename = versionString + ".exe";
                    update.Size = long.Parse(updateNode.Attributes["Size"].Value);
                    foreach (XmlNode updateDetailNode in updateNode.ChildNodes)
                    {
                        if (updateDetailNode.Name.Equals("ChangeLog"))
                        {
                            update.ChangeLog = updateDetailNode.InnerText;
                        }
                        else if (updateDetailNode.Name.Equals("Delete"))
                        {
                            update.Delete = new UpdateDelete();
                            update.Delete.Folders = new List<string>();
                            update.Delete.Files = new List<string>();
                            foreach (XmlNode deleteNode in updateDetailNode.ChildNodes)
                            {
                                if (deleteNode.Name.Equals("Folders"))
                                {
                                    foreach (XmlNode folderNode in deleteNode.ChildNodes)
                                    {
                                        update.Delete.Folders.Add(folderNode.InnerText);
                                    }
                                }
                                else if (deleteNode.Name.Equals("Files"))
                                {
                                    foreach (XmlNode fileNode in deleteNode.ChildNodes)
                                    {
                                        update.Delete.Files.Add(fileNode.InnerText);
                                    }
                                }
                                else
                                {
                                    throw new Exception("Unsupported node in update xml");
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Unsupported node in update xml");
                        }
                    }
                    updateList.Add(update);
                }
            }
            _remoteUpdateList = updateList;
        }
    }
}