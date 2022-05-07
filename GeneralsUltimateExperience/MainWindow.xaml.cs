using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using Microsoft.Win32;
using MahApps.Metro.Controls;
using GeneralsUltimateExperience.Fullscreen;

namespace GeneralsUltimateExperience
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region Constants, structs enums
        public const string UPDATE_SERVICE_URL = "http://gregware.internet-box.ch/GeneralsUltimateExperience";
        public const double CHANGE_TAB_ANIMATION_DURATION_IN_SECONDS = 0.1;
        public const double CHANGE_MOD_ANIMATION_DURATION_IN_SECONDS = 0.25;
        public const double BUTTON_FADE_DURATION_IN_SECONDS = 0.1;
        public const double MOD_BUTTON_ACTIVATE_DURATION_IN_SECONDS = 0.5;
        public const double MOD_BUTTON_BLACK_RECTANGLE_OPACTIY = 0.6;
        public const double INITIAL_MOUSEMOVE_FADIN_DURATION_IN_SECONDS = 1;
        private const int STARTUP_ANIMATION_MOUSE_RADIUS = 20;

        public enum TabStateEnum { First, Normal, Change1, Change2 }

        public struct Version
        {
            public int Major;
            public int Minor;
        }

        public struct Update
        {
            public Version Version;
            public string Filename;
            public string ChangeLog;
        }
        #endregion

        #region Variables
        private string _pathToExe;
        private string _pathToUserFolderGenerals;
        private string _pathToUserFolderHeureH;
        private string _pathToMapsGenerals;
        private string _pathToMapsZeroHour;
        private string _pathToOptionIniGenerals;
        private string _pathToOptionIniHeureH;
        private readonly TaskScheduler _uiScheduler;
        private Version _localVersion = new Version();
        private Version _remoteLatestVersion = new Version();
        private List<Update> _remoteUpdateList = new List<Update>();
        private string _currentGameName;
        private Point _originalMousePosition;
        private TabStateEnum _tabState = TabStateEnum.First;
        private bool _imageDeFond1 = true;
        private GregwareCustomizations _fullScreenGregware = null;
        private static int _gameProcessId = -1;
        public bool IsStarted = false;
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            // Vérifier si un changement en cours a été interrompu
            if (Properties.Settings.Default.ChangingGeneralsMod || Properties.Settings.Default.ChangingHeureHMod)
            {
                MessageBoxResult messageBoxResult = CustomMessageBox.Show(string.Format("L'opération a été interrompue au mauvais moment, l'état des fichiers a été perdu :-({0}{0}Le programme va désormais considérer que c'est le jeu original (sans mod) qui est installé.{0}{0}En cas de problème, réinstaller tout.",
                    Environment.NewLine), "Erreur fatale", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                if (messageBoxResult == MessageBoxResult.OK)
                {
                    if (Properties.Settings.Default.ChangingGeneralsMod)
                    {
                        Properties.Settings.Default.CurrentGeneralsMod = ModFactory.ORIGINAL_MOD_ID;
                        Properties.Settings.Default.ChangingGeneralsMod = false;
                        Properties.Settings.Default.Save();
                    }
                    if (Properties.Settings.Default.ChangingHeureHMod)
                    {
                        Properties.Settings.Default.CurrentHeureHMod = ModFactory.ORIGINAL_MOD_ID;
                        Properties.Settings.Default.ChangingHeureHMod = false;
                        Properties.Settings.Default.Save();
                    }
                }
                else
                {
                    Environment.Exit(0);
                }
            }

            // Lire la version dans le registre
            try
            {
                RegistryKey keyUge = Registry.LocalMachine.OpenSubKey("Software\\GeneralsUltimateExperience");
                _localVersion.Major = (int)keyUge.GetValue("MajorVersion");
                _localVersion.Minor = (int)keyUge.GetValue("MinorVersion");
            }
            catch (Exception)
            {
                CustomMessageBox.Show(string.Format("La base de registre est corrompue :-({0}{0}Veuillez réinstaller Generals Ultimate Experience.",
                        Environment.NewLine), "Serious error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Lire la version distante
            try
            {
                RefreshRemoteVersion();
            }
            catch
            {
                // La version du server n'a pas pu être déterminée, ne pas planter pour cela
            }

            // Set task scheduler
            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            // Path to application exe
            _pathToExe = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // Path to maps
            string zeroHourFolderName = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour", "UserDataLeafName", null);
            if (zeroHourFolderName == null) throw new Exception("Installation de HeureH non trouvée :-(");
            string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _pathToUserFolderGenerals = string.Format("{0}\\Command and Conquer Generals Data", myDocumentPath);
            _pathToUserFolderHeureH = string.Format("{0}\\{1}", myDocumentPath, zeroHourFolderName);
            _pathToMapsGenerals = string.Format("{0}\\Maps", _pathToUserFolderGenerals);
            _pathToMapsZeroHour = string.Format("{0}\\Maps", _pathToUserFolderHeureH);

            // Path to option.ini
            _pathToOptionIniGenerals = string.Format("{0}\\Options.ini", _pathToUserFolderGenerals);
            _pathToOptionIniHeureH = string.Format("{0}\\Options.ini", _pathToUserFolderHeureH);
        }
        #endregion

        #region Properties
        private bool IsVersionObsolete
        {
            get
            {
                if (_remoteLatestVersion.Major > _localVersion.Major) return true;
                if (_remoteLatestVersion.Major == _localVersion.Major && _remoteLatestVersion.Minor > _localVersion.Minor) return true;
                return false;
            }
        }

        public Image ImageDeFondCourante
        {
            get
            {
                if (_imageDeFond1) return imageDeFond1;
                return imageDeFond2;
            }
        }

        public Image ImageDeFondAutre
        {
            get
            {
                if (!_imageDeFond1) return imageDeFond1;
                return imageDeFond2;
            }
        }

        public List<TabItem> TabItems { get; private set; }

        public ListCollectionView TabItemsCollectionView { get; private set; }
        #endregion

        #region Methods
        public void ToggleImageDeFond()
        {
            _imageDeFond1 = !_imageDeFond1;
        }
        #endregion

        #region Event handlers
        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            buttonSettings.ContextMenu.IsEnabled = true;
            buttonSettings.ContextMenu.PlacementTarget = (sender as Button);
            buttonSettings.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            buttonSettings.ContextMenu.IsOpen = true;
        }

        private void settingsUpdate_Click(object sender, RoutedEventArgs e)
        {
            // Version distante
            try
            {
                RefreshRemoteVersion();
            }
            catch (Exception)
            {
                CustomMessageBox.Show(this, string.Format("Le service n'est pas disponible. Essaye ultérieurement.",
                        Environment.NewLine), "Mise à jour", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Comparaison
            if (!IsVersionObsolete)
            {
                CustomMessageBox.Show(this, string.Format("Tu disposes déjà de la version la plus récente !",
                        Environment.NewLine), "Mise à jour", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Mettre à jour
            UpdateGue();
        }

        private void settingsGameDat_Click(object sender, RoutedEventArgs e)
        {
            if (IsGameRunning())
            {
                CustomMessageBox.Show(this, string.Format("Nettoyage de game.dat impossible : le jeu est en cours d'exécution.{0}{0}Quitte d'abord le jeu (si nécessaire via Gestionnaire des tâches => fin de tâche sur game.dat)",
                        Environment.NewLine), "Nettoyage game.dat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                File.Copy(
                    string.Format("{0}\\Repair\\Generals\\game.dat", _pathToExe),
                    string.Format("{0}\\Games\\Generals\\game.dat", _pathToExe),
                    true);
                File.Copy(
                    string.Format("{0}\\Repair\\HeureH\\game.dat", _pathToExe),
                    string.Format("{0}\\Games\\HeureH\\game.dat", _pathToExe),
                    true);

                Properties.Settings.Default["Current4g"] = false;
                Properties.Settings.Default.Save();

                CustomMessageBox.Show(this, "Game.dat nettoyés avec succès :-)", "Nettoyage game.dat", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception)
            {
                CustomMessageBox.Show(this, string.Format("Le nettoyage de game.dat a échoué :-({0}{0}Vérifier que le jeu n'est pas en cours d'exécution et essayer ultérieurement.",
                        Environment.NewLine), "Nettoyage game.dat", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void settingsCleanMaps_Click(object sender, RoutedEventArgs e)
        {
            if (CustomMessageBox.Show(this, string.Format("Tu es sur le point de supprimer toutes les maps ajoutées manuellement ou téléchargées depuis le jeu. Continuer ?",
                        Environment.NewLine), "Nettoyage des maps", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            if (IsGameRunning())
            {
                CustomMessageBox.Show(this, string.Format("Nettoyage des maps impossible : le jeu est en cours d'exécution.{0}{0}Quitte d'abord le jeu (si nécessaire via Gestionnaire des tâches => fin de tâche sur generals.exe et/ou game.dat)",
                        Environment.NewLine), "Nettoyage des maps", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Se placer sur aucun MapPack
            ModFactory.AllGamesNoMapPack();

            // Supprimer les maps restantes
            try
            {
                foreach (string path in Directory.GetDirectories(_pathToMapsGenerals))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    ModFactory.SetAttributesNormal(di);
                    di.Delete(true);
                }

                foreach (string path in Directory.GetDirectories(_pathToMapsZeroHour))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    ModFactory.SetAttributesNormal(di);
                    di.Delete(true);
                }

                CustomMessageBox.Show(this, "Maps nettoyées avec succès :-)", "Nettoyage des maps", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(this, string.Format("Erreur lors de la suppression des maps.{0}{0}{1}",
                        Environment.NewLine, ex.Message), "Nettoyage des maps", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void settingsChangeSerial_Click(object sender, RoutedEventArgs e)
        {
            new SerialNumbers(_pathToExe, _uiScheduler) { Owner = this }.ShowDialog();
        }

        private void settingsResolution_Click(object sender, RoutedEventArgs e)
        {
            ModFactory.AllGamesBackToOriginal();
            new Settings(_pathToOptionIniGenerals, _pathToOptionIniHeureH) { Owner = this }.ShowDialog();
        }

        private void settingsCleanIni_Click(object sender, RoutedEventArgs e)
        {
            if (CustomMessageBox.Show(this, string.Format("Tu es sur le point de rétablir les options par défaut, on continue ?",
                        Environment.NewLine), "Nettoyer Options.ini", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            string pathToIniGenerals = string.Format("{0}\\Options.ini", _pathToUserFolderGenerals);
            if (File.Exists(pathToIniGenerals)) File.Delete(pathToIniGenerals);
            WriteIniFile(File.CreateText(pathToIniGenerals));

            string pathToIniHeureH = string.Format("{0}\\Options.ini", _pathToUserFolderHeureH);
            if (File.Exists(pathToIniHeureH)) File.Delete(pathToIniHeureH);
            WriteIniFile(File.CreateText(pathToIniHeureH));
        }

        private void settingsAbout_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBox.Show(this, string.Format("Version {1}.{2}{0}{0}Copyright © 2016-2022 Gregware",
                        Environment.NewLine, _localVersion.Major, _localVersion.Minor), "À propos", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void buttonLaunchGame_Click(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.ChangingGeneralsMod || Properties.Settings.Default.ChangingHeureHMod || canvasLoading.Visibility == Visibility.Visible) return;
            if (IsGameRunning()) return;

            // Désactiver la form
            DesactiverWindow();

            // Créer un thread pour éviter les problèmes UI
            Task.Factory.StartNew(() =>
            {
                // Execute game
                string gameExePath = ModFactory.GetGameExecutable(_currentGameName);
                Process gameProcess = Process.Start(gameExePath);
                _gameProcessId = gameProcess.Id;
            });
        }

        private void Items_CurrentChanging(object sender, System.ComponentModel.CurrentChangingEventArgs e)
        {
            // Annuler l'événement si nécessaire
            if (!this.IsLoaded || tabControl.SelectedIndex < 0 || _tabState == TabStateEnum.Change1)
            {
                if (e.IsCancelable) e.Cancel = true;
                return;
            }

            // 2ème étape du changement
            if (_tabState == TabStateEnum.Change2)
            {
                _tabState = TabStateEnum.Normal;
                return;
            }

            // Se souvenir de l'onglet sélectionner et annuler l'action (elle sera refaite manuellement après animation)
            int previousIndex = tabControl.SelectedIndex;
            if (e.IsCancelable) e.Cancel = true;

            // Démarrer l'animation de sortie
            ModFactory.ChangeTabAnimation(_currentGameName, 0);

            // Mettre à jour le statut
            _tabState = TabStateEnum.Change1;

            // Attendre la fin de l'animation
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(Convert.ToInt32(CHANGE_TAB_ANIMATION_DURATION_IN_SECONDS * 1000));
            }).ContinueWith(antecedent =>
            {
                // Stuff pour que l'animation fonctionne
                tabControl.SelectedIndex = -1;
                _tabState = TabStateEnum.Change2;
                tabControl.SelectedIndex = previousIndex;

                // Prendre note du changement de jeu
                _currentGameName = ((TabItem)tabControl.Items.GetItemAt(tabControl.SelectedIndex)).Name;

                // Afficher et lancer l'animation
                ModFactory.Refresh(_currentGameName, true);
                ModFactory.ChangeTabAnimation(_currentGameName, 1);

                // Adapter le bouton launch
                bool isActivated = (bool)Properties.Settings.Default[string.Format("CurrentLaunchActivated{0}", _currentGameName)];
                ModFactory.ButtonAnimation(buttonLaunchGame, isActivated ? 1 : 0, CHANGE_TAB_ANIMATION_DURATION_IN_SECONDS);
                buttonLaunchGame.IsEnabled = isActivated;

            }, _uiScheduler);
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(e.Source is TabControl)) return;
            if (!this.IsLoaded) return;
            if (_tabState != TabStateEnum.First) return;

            // Evenements sur changement de tab
            tabControl.IsSynchronizedWithCurrentItem = true;
            tabControl.Items.CurrentChanging += Items_CurrentChanging;
            _tabState = TabStateEnum.Normal;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;

            // Faire la collection pour le tab
            TabItem tabItemGenerals = new TabItem { Header = "GENERALS", Name = "Generals" };
            ScrollViewer svGenerals = new ScrollViewer { Name = "svGenerals", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 0), VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            tabItemGenerals.Content = svGenerals;
            Grid gridGenerals = new Grid { Name = "gridButtonsGenerals" };
            svGenerals.Content = gridGenerals;

            TabItem tabItemHeureH = new TabItem { Header = "HEURE H", Name = "HeureH" };
            ScrollViewer svHeureH = new ScrollViewer { Name = "svHeureH", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 0), VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            tabItemHeureH.Content = svHeureH;
            Grid gridHeureH = new Grid { Name = "gridButtonsHeureH" };
            svHeureH.Content = gridHeureH;

            TabItems = new List<TabItem>();
            TabItems.Add(tabItemGenerals);
            TabItems.Add(tabItemHeureH);

            TabItemsCollectionView = new ListCollectionView(TabItems);
            buttonLaunchGame.Effect = new GrayscaleEffect.GrayscaleEffect();
            buttonLaunchGame.IsEnabledChanged += ButtonLaunchGame_IsEnabledChanged;
            bool isActivated = (bool)Properties.Settings.Default["CurrentLaunchActivatedHeureH"];
            ModFactory.ButtonAnimation(buttonLaunchGame, isActivated ? 1 : 0, CHANGE_TAB_ANIMATION_DURATION_IN_SECONDS);
            buttonLaunchGame.IsEnabled = isActivated;

            // Load mods
            _currentGameName = "HeureH";
            ModFactory.Init(this, new List<string> { "Generals", "HeureH" }, new List<string> { _pathToMapsGenerals, _pathToMapsZeroHour }, _pathToExe, _uiScheduler,
                new List<Grid> { gridGenerals, gridHeureH },
                new List<double> { Convert.ToInt32(_currentGameName.Equals("Generals")), Convert.ToInt32(_currentGameName.Equals("HeureH")) },
                new List<ComboBox> { comboBoxMapsGenerals, comboBoxMapsHeureH },
                (int)Properties.Settings.Default["FullscreenMode"] == 2,
                (bool)Properties.Settings.Default["Current4g"], 
                buttonLaunchGame);

            // Evenements sur changement de tab
            tabControl.SelectionChanged += tabControl_SelectionChanged;

            // Sélectionner le bon jeu
            tabControl.SelectedIndex = 1;

            // Réappliquer les sélection après update
            if((bool)Properties.Settings.Default["JustUpdated"])
            {
                ModFactory.AllGamesRefreshCameraSettings();
                ModFactory.AllGamesRefreshFullscreenMode();
                ModFactory.AllGamesRefreshGentool();
                Properties.Settings.Default["JustUpdated"] = false;
                Properties.Settings.Default.Save();
            }

            // Démarrer l'affichage
            ModFactory.Refresh(_currentGameName);

            // Démarrer le détecteur de lancement
            MonitorGameRunning();
        }

        private void ButtonLaunchGame_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            double target;
            if ((bool)e.NewValue) target = 1;
            else target = 0;

            ModFactory.ButtonAnimation(buttonLaunchGame, target, CHANGE_MOD_ANIMATION_DURATION_IN_SECONDS);
        }

        private void MetroWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentMousePosition = PointToScreen(Mouse.GetPosition(this));
            if (_originalMousePosition.X == 0 && _originalMousePosition.Y == 0)
            {
                _originalMousePosition = currentMousePosition;
                return;
            }
            if (currentMousePosition.X > _originalMousePosition.X - STARTUP_ANIMATION_MOUSE_RADIUS &&
                currentMousePosition.X < _originalMousePosition.X + STARTUP_ANIMATION_MOUSE_RADIUS &&
                currentMousePosition.Y > _originalMousePosition.Y - STARTUP_ANIMATION_MOUSE_RADIUS &&
                currentMousePosition.Y < _originalMousePosition.Y + STARTUP_ANIMATION_MOUSE_RADIUS)
                return;

            this.MouseMove -= MetroWindow_MouseMove;

            // Animation
            DoubleAnimation animation = new DoubleAnimation
            {
                To = 1,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = TimeSpan.FromSeconds(INITIAL_MOUSEMOVE_FADIN_DURATION_IN_SECONDS),
                FillBehavior = FillBehavior.Stop
            };
            animation.Completed += (s, a) =>
            {
                imageDeFond1.Opacity = 1;
                buttonLaunchGame.Opacity = 1;
                buttonSettings.Opacity = 1;
                labelTitle.Opacity = 1;
                richTextbox.Opacity = 1;
                IsStarted = true;
                imageDeFond0.Opacity = 1;
                labelTitleMaps.Opacity = 1;
                comboBoxMapsHeureH.Opacity = 1;
            };
            imageDeFond1.BeginAnimation(UIElement.OpacityProperty, animation);
            buttonLaunchGame.BeginAnimation(UIElement.OpacityProperty, animation);
            buttonSettings.BeginAnimation(UIElement.OpacityProperty, animation);
            labelTitle.BeginAnimation(UIElement.OpacityProperty, animation);
            richTextbox.BeginAnimation(UIElement.OpacityProperty, animation);
            labelTitleMaps.BeginAnimation(UIElement.OpacityProperty, animation);
            comboBoxMapsHeureH.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            if (IsVersionObsolete)
            {
                Keyboard.Focus(this);
                if(CustomMessageBox.Show(this, string.Format("Une mise à jour est disponible.{0}{0}Mettre à jour maintenant ?",
                   Environment.NewLine), "Mise à jour disponible", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    UpdateGue();
                }
            }
        }

        //protected override void OnKeyDown(KeyEventArgs e)
        //{
        //    if (Properties.Settings.Default.ChangingHeureHMod || canvasLoading.Visibility == Visibility.Visible) return;

        //    List<string> keys = HeureHOrderedDefinitions.Keys.ToList();
        //    int index = keys.IndexOf(CurrentHeureHMod);

        //    switch (e.Key)
        //    {
        //        case Key.Up:
        //            // Get previous enum
        //            index--;
        //            if (index < 0) index = 0;
        //            string previousModId = keys[index];

        //            // Activate corresponding button
        //            ButtonHeureHChangeMod_Click(GetButton(previousModId), new RoutedEventArgs());

        //            return;

        //        case Key.Down:
        //            // Get next enum
        //            index++;
        //            if (index > keys.Count - 1) index = keys.Count - 1;
        //            string nextModId = keys[index];

        //            // Activate corresponding button
        //            ButtonHeureHChangeMod_Click(GetButton(nextModId), new RoutedEventArgs());

        //            return;

        //        case Key.Enter:
        //            buttonLaunchGame_Click(buttonLaunchGame, new RoutedEventArgs());
        //            return;
        //    }

        //    base.OnKeyDown(e);
        //}
        #endregion

        #region Helpers
        private void DesactiverWindow()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            canvasLoading.Visibility = Visibility.Visible;
            IsEnabled = false;
        }

        private void ActiverWindow()
        {
            IsEnabled = true;
            canvasLoading.Visibility = Visibility.Hidden;
            Mouse.OverrideCursor = null;
            Keyboard.Focus(this);
        }

        private void MonitorGameRunning()
        {
            Task.Factory.StartNew(() =>
            {
                while (!IsGameRunning()) Thread.Sleep(1000);
            }).ContinueWith(antecedent =>
            {
                DesactiverWindow();
            }, _uiScheduler).ContinueWith(antecedent2 =>
            {
                ActiverGregwareCustomizations();
                while (IsGameRunning()) Thread.Sleep(1000);
            }).ContinueWith(antecedent3 =>
            {
                ActiverWindow();
            }, _uiScheduler).ContinueWith(antecedent4 =>
            {
                _fullScreenGregware.Desactiver();
                _fullScreenGregware = null;
                MonitorGameRunning();
            });
        }

        private void ActiverGregwareCustomizations()
        {
            // Initialisation
            bool isFullscreenGregware = (int)Properties.Settings.Default["FullscreenMode"] == 1;
            bool isScrollGregware = (bool)Properties.Settings.Default["ScrollGregware"];
            bool isGentool = (int)Properties.Settings.Default["FullscreenMode"] == 2;
            int resolutionX = 0;
            int resolutionY = 0;

            // Validité
            if (!isFullscreenGregware && !isScrollGregware) return;

            // Trouver la résolution actuelle
            if (isFullscreenGregware)
            {
                string pathToOptionIni;
                if (_currentGameName.Equals("Generals", StringComparison.OrdinalIgnoreCase)) pathToOptionIni = _pathToOptionIniGenerals;
                else if (_currentGameName.Equals("HeureH", StringComparison.OrdinalIgnoreCase)) pathToOptionIni = _pathToOptionIniHeureH;
                else throw new Exception(string.Format("Unknown game name {0}", _currentGameName));
                IniHelper.OptionIni optionIniResolution = IniHelper.GetOptionIni(pathToOptionIni);
                resolutionX = optionIniResolution.ResolutionX;
                resolutionY = optionIniResolution.ResolutionY;
            }

            // Activer
            _fullScreenGregware = new GregwareCustomizations(isFullscreenGregware, isScrollGregware, isGentool, resolutionX, resolutionY, _uiScheduler);
            _fullScreenGregware.Activer(_gameProcessId);
        }

        private void BlockIfGameRunning()
        {
            if (IsGameRunning())
            {
                Task.Factory.StartNew(() => { DesactiverWindow(); }, CancellationToken.None, TaskCreationOptions.None, _uiScheduler);
                Task.Factory.StartNew(() =>
                {
                    while (IsGameRunning()) Thread.Sleep(1000);
                }).ContinueWith(antecedent =>
                {
                    ActiverWindow();
                }, _uiScheduler);
            }
        }

        public static bool IsGameRunning()
        {
            //return Process.GetProcessesByName("game.dat").FirstOrDefault() != default(Process);
            return Process.GetProcesses().Any(p => p.Id == _gameProcessId);
        }

        private void RefreshRemoteVersion()
        {
            // Obtenir le fichier xml de la part du server
            string content;
            using (WebClient webClient = new WebClient())
            {
                content = webClient.DownloadString(UPDATE_SERVICE_URL + "/Updates.xml");
            }

            // Décoder le fichier xml
            List<Update> updateList = new List<Update>();
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(new StringReader(content), readerSettings))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(reader);
                foreach (XmlNode updateNode in xml.DocumentElement.ChildNodes[0].ChildNodes)
                {
                    Update update = new Update();
                    string versionString = updateNode.Attributes["Version"].Value;
                    update.Version = new Version { Major = Int32.Parse(versionString.Split('.')[0]), Minor = Int32.Parse(versionString.Split('.')[1]) };
                    update.Filename = versionString + ".exe";
                    foreach (XmlNode updateDetailNode in updateNode.ChildNodes)
                    {
                        if (updateDetailNode.Name.Equals("ChangeLog"))
                        {
                            update.ChangeLog = updateDetailNode.InnerText;
                        }
                    }
                    updateList.Add(update);
                }
            }
            _remoteUpdateList = updateList;

            // Si aucune entrée on considère qu'on est en 1.0
            if (_remoteUpdateList.Count <= 0)
            {
                _remoteLatestVersion.Major = 1;
                _remoteLatestVersion.Minor = 0;
                return;
            }

            // Sinon on trouve la dernière version
            Update latestUpdate = _remoteUpdateList.OrderByDescending(u => u.Version.Major).ThenByDescending(u => u.Version.Minor).First();
            _remoteLatestVersion.Major = latestUpdate.Version.Major;
            _remoteLatestVersion.Minor = latestUpdate.Version.Minor;
        }

        private void UpdateGue()
        {
            // Confirmation
            UpdateWindow updateWindow = new UpdateWindow(_localVersion, _remoteLatestVersion, _remoteUpdateList) { Owner = this };
            updateWindow.ShowDialog();
            if (!updateWindow.IsOK) return;

            if(IsGameRunning())
            {
                CustomMessageBox.Show(this, string.Format("Mise à jour impossible : le jeu est en cours d'exécution.{0}{0}Quitte d'abord le jeu (si nécessaire via Gestionnaire des tâches => fin de tâche sur generals.exe et/ou game.dat)",
                        Environment.NewLine), "Mise à jour", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Basculement sur le mod Original + pas de map pack
            ModFactory.AllGamesBackToOriginal();
            ModFactory.AllGamesNoMapPack();

            // On "se souvient"
            Properties.Settings.Default["JustUpdated"] = true;
            Properties.Settings.Default.Save();

            // On copie l'exécutable dans un dossier temporaire pour l'exécuter depuis là (sinon on ne pourrait pas mettre à jour l'installeur)
            string tempFolderPath = Path.GetTempPath();
            string executablePath = _pathToExe + "\\Setup.exe";
            string tempPath = Path.GetTempPath() + Path.GetFileName(executablePath);
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    CustomMessageBox.Show(this, "Impossible de générer le fichier temporaire", "Mise à jour annulée", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            File.Copy(executablePath, tempPath);

            Process process = new Process();
            process.StartInfo.FileName = tempPath;
            process.StartInfo.Arguments = "-maj " + Process.GetCurrentProcess().Id.ToString();
            process.Start();

            // Quitter
            Close();
        }

        // Attention en cas de modif : il y a son frère jumeau dans l'autre projet !!!
        private void WriteIniFile(StreamWriter sw)
        {
            // Trouver la résolution optimale (native si <= 1440p, 1080p sinon)
            System.Drawing.Rectangle resolution = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
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
        #endregion
    }
}