using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GeneralsUltimateExperience
{
    public class ModFactory
    {
        #region Constants
        public const string ORIGINAL_MOD_ID = "Original";
        private const string XML_FILENAME = "definitions.xml";
        private const string IMAGE_BANNER_RELATIVE_PATH = "Images\\Mods\\{0}\\Banners";
        private const string IMAGE_BANNER_FILE_EXTENSION = "png";
        private const string IMAGE_BACKGROUND_RELATIVE_PATH = "Images\\Mods\\{0}\\Backgrounds";
        private const string IMAGE_BACKGROUND_FILE_EXTENSION = "bmp";
        private const string GAMEDATA_FILE_RELATIVE_PATH = "Data\\INI\\GameData.ini";
        private const string GAMEDATA_CUSTOM_FILE_RELATIVE_PATH = "CustomINI\\GameData.ini";
        private const int NB_OF_IO_RETRY = 10;
        private const int DELAY_BETWEEN_IO_RETRY_MS = 500;
        private const string MESSAGE_ACCESS_PROBLEM_1 = "Problème d'accès lors du déplacement d'un {0}. Réessayer ? Si le jeu est en cours d'exécution quittez-le.{1}{1}Source : {2}{1}Destination : {3}{1}{1}Attention si le programme ne parvient pas à copier/déplacer les fichiers il perdra ses références et ne fonctionnera plus correctement. Une réinstallation complète sera alors nécessaire :-( {1}{1}Détails : {4}";
        private const string MESSAGE_ACCESS_PROBLEM_2 = "Voulez-vous vraiment interrompre le déplacement ?{0}{0}Attention le programme perdra ses références et ne fonctionnera plus correctement !!! Une réinstallation complète sera nécessaire :-(";
        private readonly List<string> GENTOOL_FILES = new List<string> { "d3d8.cfg", "d3d8.dlL", "GenToolUpdater.exe" };
        private readonly List<string> PATCH4G_FILES = new List<string> { "game.dat", "generals.exe" };
        #endregion

        #region structs
        private struct Elements
        {
            public Contenu Contenu;
            public string Description;
            public string ImageName;
            public string Title;
            public string DisplayName;
            public int Order;
        }

        private struct Contenu
        {
            public ContenuMod Mod;
            public ContenuOriginal Original;
        }

        private struct ContenuOriginal
        {
            public List<string> Files;
            public List<string> Folders;
        }

        private struct ContenuMod
        {
            public List<string> Files;
            public List<string> Folders;
            public List<string> Maps;
        }
        #endregion

        #region Variables
        private static Dictionary<string, ModFactory> _modFactories;
        private Dictionary<string, Elements> _definitions = new Dictionary<string, Elements>();
        private Dictionary<string, Button> _modButtons = new Dictionary<string, Button>();
        private Dictionary<string, Label> _modLabels = new Dictionary<string, Label>();
        private Dictionary<string, Rectangle> _modRectangles = new Dictionary<string, Rectangle>();
        private Dictionary<string, List<string>> _mapPacks = new Dictionary<string, List<string>>();
        private MainWindow _window;
        private Grid _panelButtons;
        private string _gameName;
        private string _pathToApplication;
        private string _pathToMaps;
        private string _pathToMod;
        private string _pathToGame;
        private string _pathToMapPacks;
        private string _pathToGentool;
        private string _pathToGameDataIni;
        private string _pathToPatch4g;
        private TaskScheduler _uiScheduler;
        private ComboBox _comboboxMapPack;
        private bool _isGentool;
        private bool _isPatch4g;
        private Button _buttonLaunchGame;
        #endregion

        #region Constructor
        public ModFactory(MainWindow window, string gameName, string pathToApplication, string pathToMaps, TaskScheduler uiScheduler, Grid panelButtons, double buttonOpacity, ComboBox comboboxMapPack, bool isGentool, bool isPatch4g, Button buttonLaunchGame)
        {
            _window = window;
            _gameName = gameName;
            _panelButtons = panelButtons;
            _pathToApplication = pathToApplication;
            _pathToMaps = pathToMaps;
            _pathToMod = string.Format("{0}\\Mods\\{1}", _pathToApplication, _gameName);
            _pathToGame = string.Format("{0}\\Games\\{1}", _pathToApplication, _gameName);
            _pathToMapPacks = string.Format("{0}\\MapPacks\\{1}", _pathToApplication, _gameName);
            _pathToGentool = string.Format("{0}\\Gentool", _pathToApplication);
            _pathToGameDataIni = string.Format("{0}\\Games\\{1}\\{2}", _pathToApplication, _gameName, GAMEDATA_FILE_RELATIVE_PATH);
            _pathToPatch4g = string.Format("{0}\\Patch4g", _pathToApplication);
            _uiScheduler = uiScheduler;
            _comboboxMapPack = comboboxMapPack;
            _isGentool = isGentool;
            _isPatch4g = isPatch4g;
            _buttonLaunchGame = buttonLaunchGame;

            string xmlPath = string.Format("{0}\\{1}", _pathToApplication, XML_FILENAME);
            LoadXml(xmlPath);

            // Créer boutons des mods
            int margin = 0;
            foreach (KeyValuePair<string, Elements> definition in OrderedDefinitions)
            {
                string bannerImagepath = string.Format("{0}\\{1}\\{2}.{3}", _pathToApplication, string.Format(IMAGE_BANNER_RELATIVE_PATH, _gameName), definition.Value.ImageName, IMAGE_BANNER_FILE_EXTENSION);
                ImageBrush brush = new ImageBrush();
                if (File.Exists(bannerImagepath))
                {
                    // Utilisation de l'image du mod (pas d'image par défaut)
                    brush.ImageSource = new BitmapImage(new Uri(bannerImagepath, UriKind.Absolute));
                }

                ControlTemplate template = new ControlTemplate(typeof(Button));
                var image = new FrameworkElementFactory(typeof(Image));
                image.SetValue(Image.SourceProperty, new BitmapImage(new Uri(bannerImagepath, UriKind.Absolute)));
                template.VisualTree = image;

                Button button = new Button
                {
                    Name = "Button" + _gameName + "Mod" + definition.Key.ToString(),
                    Background = brush,
                    Template = template,
                    Width = 197,
                    Height = 31,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, margin, 0, 0),
                };

                button.Effect = new GrayscaleEffect.GrayscaleEffect();
                button.MouseEnter += (s, e) => { if (s != ActiveModButton) ButtonAnimation((Button)s, 1.0, MainWindow.BUTTON_FADE_DURATION_IN_SECONDS); };
                button.MouseLeave += (s, e) => { if (s != ActiveModButton) ButtonAnimation((Button)s, 0, MainWindow.BUTTON_FADE_DURATION_IN_SECONDS); };
                button.Opacity = buttonOpacity;
                button.Click += ButtonChangeMod_Click;

                Rectangle rectangle = new Rectangle()
                {
                    Width = 197,
                    Height = 31,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, margin, 0, 0),
                    Fill = new SolidColorBrush(Colors.Black),
                    Opacity = MainWindow.MOD_BUTTON_BLACK_RECTANGLE_OPACTIY,
                    IsHitTestVisible = false
                };

                Label label = new Label
                {
                    Width = 197,
                    Height = 31,
                    Content = definition.Value.DisplayName,
                    Margin = new Thickness(0, margin + 5, 0, 0),
                    IsHitTestVisible = false,
                    Foreground = new SolidColorBrush(Colors.White),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontFamily = new FontFamily("Verdana"),
                    FontSize = 13
                };

                _panelButtons.Children.Add(button);
                _panelButtons.Children.Add(rectangle);
                _panelButtons.Children.Add(label);
                _modButtons.Add(definition.Key, button);
                _modLabels.Add(definition.Key, label);
                _modRectangles.Add(definition.Key, rectangle);

                margin += 31;
            }

            // MapPacks
            _comboboxMapPack.Items.Add(new ComboBoxItem { Content = "- Aucun -" });
            _comboboxMapPack.Items.Add(new ComboBoxItem { Content = "- Maps du mod -" });
            foreach (string mapPack in _mapPacks.Keys) _comboboxMapPack.Items.Add(new ComboBoxItem { Content = mapPack });
            _comboboxMapPack.SelectedItem = _comboboxMapPack.Items.GetItemAt((int)Properties.Settings.Default[string.Format("Current{0}MapPack", _gameName)]);
            _comboboxMapPack.SelectionChanged += _comboboxMapPack_SelectionChanged;
        }
        #endregion

        #region Properties
        public string GameExecutable
        {
            get { return string.Format("{0}\\generals.exe", _pathToGame); }
        }

        private Dictionary<string, Elements> OrderedDefinitions
        {
            get { return _definitions.OrderBy(x => x.Value.Order).ThenBy(x => x.Key.ToString()).ToDictionary(x => x.Key, x => x.Value); }
        }

        private string CurrentMod
        {
            get { return Properties.Settings.Default[string.Format("Current{0}Mod", _gameName)].ToString(); }
        }

        private Button ActiveModButton
        {
            get { return GetButton(CurrentMod); }
        }

        private bool IsForceZoom
        {
            get { return bool.Parse(Properties.Settings.Default[string.Format("CurrentForceZoom", _gameName)].ToString()); }
        }
        #endregion

        #region Methods
        public static void Init(MainWindow window, List<string> gameNames, List<string> pathToMaps, string pathToApplication, TaskScheduler uiScheduler, List<Grid> panelButtons, List<double> buttonOpacities, List<ComboBox> comboboxMapPacks, bool isGentool, bool isPAtch4g, Button buttonLaunchGame)
        {
            _modFactories = new Dictionary<string, ModFactory>();
            for (int i = 0; i < gameNames.Count; i++)
            {
                _modFactories.Add(gameNames[i], new ModFactory(window, gameNames[i], pathToApplication, pathToMaps[i], uiScheduler, panelButtons[i], buttonOpacities[i], comboboxMapPacks[i], isGentool, isPAtch4g, buttonLaunchGame));
            }
        }

        public static void AllGamesBackToOriginal()
        {
            foreach (ModFactory modFactory in _modFactories.Values)
            {
                modFactory.ChangeMod(ORIGINAL_MOD_ID);
                modFactory.RefreshControls();
            }
        }

        public static void AllGamesNoMapPack()
        {
            foreach (ModFactory modFactory in _modFactories.Values)
            {
                modFactory.SwitchToNoMapPackWithoutErrorMessage();
            }
        }

        public static void AllGamesRefresh4g(bool enabled)
        {
            foreach (ModFactory modFactory in _modFactories.Values)
            {
                modFactory.Activer4g(enabled);
            }
        }

        public static void AllGamesRefreshCameraSettings()
        {
            foreach (ModFactory modFactory in _modFactories.Values)
            {
                modFactory.RefreshGameDataIniCameraSettings();
            }
        }

        public static void AllGamesRefreshZoomLibre(bool enabled)
        {
            foreach (ModFactory modFactory in _modFactories.Values)
            {
                modFactory.RefreshZoomLibre(enabled);
            }
        }

        public static void AllGamesRefreshFullscreenMode()
        {
            bool windowed = (int)Properties.Settings.Default["FullscreenMode"] == 1; // seulement pour mode Gregware
            foreach (ModFactory modFactory in _modFactories.Values)
            {
                modFactory.SetFullscreenModeInGameDataIni(windowed);
            }
        }

        public static void AllGamesRefreshGentool()
        {
            bool enabled = (int)Properties.Settings.Default["FullscreenMode"] == 2;
            foreach (ModFactory modFactory in _modFactories.Values)
            {
                modFactory.ActiverGentool(enabled);
            }
        }

        public static string GetGameExecutable(string gameName)
        {
            return _modFactories[gameName].GameExecutable;
        }

        public static void Refresh(string gameName, bool fromChangetab = false)
        {
            _modFactories[gameName].RefreshControls(fromChangetab);
        }

        public void SwitchToNoMapPackWithoutErrorMessage()
        {
            ChangeMapPack(0, false, true);
            _comboboxMapPack.SelectedIndex = 0;
        }

        public void ChangeMod(string modId)
        {
            // On est déjà dans le bon mod
            if (CurrentMod == modId) return;

            // Désactiver la form et changer le curseur
            Mouse.OverrideCursor = Cursors.Wait;

            // Marquer comme "en changement"
            Properties.Settings.Default[string.Format("Changing{0}Mod", _gameName)] = true;
            Properties.Settings.Default.Save();

            // Maps
            bool noMaps = _comboboxMapPack.SelectedIndex == 0;
            bool modMaps = _comboboxMapPack.SelectedIndex == 1;
            if (modMaps) ChangeMapPack(0, true); // On est dans les cas où les maps du mod étaient sélectionnées, il faut les remettre à leur place

            // Enlever le mod précédent
            CleanPreviousMod();

            // Mettre le nouveau mod
            bool launchActivated = ApplyNextMod(modId);

            // Ne plus marquer comme "en changement"
            Properties.Settings.Default[string.Format("Current{0}Mod", _gameName)] = modId;
            Properties.Settings.Default[string.Format("Changing{0}Mod", _gameName)] = false;
            Properties.Settings.Default.Save();

            // Traiter l'activation du launch
            _buttonLaunchGame.IsEnabled = launchActivated;
            Properties.Settings.Default[string.Format("CurrentLaunchActivated{0}", _gameName)] = launchActivated;
            Properties.Settings.Default.Save();

            // Maps
            bool hasMaps = _definitions[modId].Contenu.Mod.Maps.Count > 0;
            if (modMaps)
            {
                if (hasMaps) ChangeMapPack(1, true); // On reste sur "Maps du mod" mais on remplace les maps par celle du nouveau mod
                else _comboboxMapPack.SelectedIndex = 0; // On est passé sur "Aucun", il faut mettre à jour la combobox
            }
            else if (noMaps)
            {
                if (hasMaps)
                {
                    // On passe automatiquement sur "Maps du mod" dans ce cas
                    ChangeMapPack(1, true);
                    _comboboxMapPack.SelectedIndex = 1;
                }
            }

            // Réactiver la form et changer le curseur
            Mouse.OverrideCursor = null;
        }

        public void ActiverGentool(bool enabled)
        {
            _isGentool = enabled;
            if (_isGentool)
            {
                foreach (string filename in GENTOOL_FILES)
                {
                    File.Copy(string.Format("{0}\\{1}", _pathToGentool, filename), string.Format("{0}\\{1}", _pathToGame, filename), true);
                }
            }
            else
            {
                foreach (string filename in GENTOOL_FILES)
                {
                    string path = string.Format("{0}\\{1}", _pathToGame, filename);
                    if (File.Exists(path)) File.Delete(path);
                }
            }
        }
        
        public void Activer4g(bool enabled)
        {
            _isPatch4g = enabled;
            if (_isPatch4g)
            {
                foreach (string filename in PATCH4G_FILES)
                {
                    File.Copy(string.Format("{0}\\{1}\\patch\\{2}", _pathToPatch4g, _gameName, filename), string.Format("{0}\\{1}", _pathToGame, filename), true);
                }
            }
            else
            {
                foreach (string filename in PATCH4G_FILES)
                {
                    File.Copy(string.Format("{0}\\{1}\\original\\{2}", _pathToPatch4g, _gameName, filename), string.Format("{0}\\{1}", _pathToGame, filename), true);
                }
            }
        }

        public void RefreshZoomLibre(bool enabled)
        {
            RefreshGameDataIniZoomForce(CurrentMod, enabled);
            Properties.Settings.Default["CurrentForceZoom"] = enabled;
            Properties.Settings.Default.Save();
        }

        public void RefreshControls(bool fromChangeTab = false)
        {
            // Désactiver tous les boutons
            foreach (Button button in _modButtons.Values) ButtonAnimation(button, 0, MainWindow.BUTTON_FADE_DURATION_IN_SECONDS);
            foreach (Label label in _modLabels.Values) label.Opacity = 1;
            foreach (Rectangle rectangle in _modRectangles.Values) rectangle.Opacity = MainWindow.MOD_BUTTON_BLACK_RECTANGLE_OPACTIY;

            // Activer le bon
            ButtonAnimation(ActiveModButton, 1.0, 0);

            DoubleAnimation animation = GetAnimationActivation(0);
            animation.Completed += (s, a) =>
            {
                _modLabels[CurrentMod].Opacity = 0;
                _modRectangles[CurrentMod].Opacity = 0;
            };
            _modLabels[CurrentMod].BeginAnimation(UIElement.OpacityProperty, animation);
            _modRectangles[CurrentMod].BeginAnimation(UIElement.OpacityProperty, animation);

            // Rafraîchir les images/textes
            string backgroundImagePath = string.Format("{0}\\{1}\\{2}.{3}", _pathToApplication, string.Format(IMAGE_BACKGROUND_RELATIVE_PATH, _gameName), _definitions[CurrentMod].ImageName, IMAGE_BACKGROUND_FILE_EXTENSION);
            if (!File.Exists(backgroundImagePath))
            {
                // Utilisation de l'image originale par défaut
                backgroundImagePath = string.Format("{0}\\{1}\\{2}.{3}", _pathToApplication, string.Format(IMAGE_BACKGROUND_RELATIVE_PATH, _gameName), _definitions[ORIGINAL_MOD_ID].ImageName, IMAGE_BACKGROUND_FILE_EXTENSION);
            }

            if (_window.IsStarted)
            {
                DoubleAnimation animationFondIn = GetAnimationActivation(1);
                animationFondIn.Completed += (s, a) => { _window.ImageDeFondCourante.Opacity = 1; };
                DoubleAnimation animationFondOut = GetAnimationActivation(0);
                animationFondOut.Completed += (s, a) => { _window.ImageDeFondAutre.Opacity = 0; };

                _window.ImageDeFondAutre.Source = new BitmapImage(new Uri(backgroundImagePath, UriKind.Absolute));
                _window.ImageDeFondCourante.BeginAnimation(UIElement.OpacityProperty, animationFondOut);
                _window.ImageDeFondAutre.BeginAnimation(UIElement.OpacityProperty, animationFondIn);
                _window.ToggleImageDeFond();

                if (!fromChangeTab)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(Convert.ToInt32(MainWindow.CHANGE_MOD_ANIMATION_DURATION_IN_SECONDS * 1000));
                    }).ContinueWith(antecedent =>
                    {
                        _window.labelTitle.Content = _definitions[CurrentMod].DisplayName;
                        _window.labelTitle.ToolTip = _definitions[CurrentMod].Title;
                        _window.richTextbox.Document.Blocks.Clear();
                        _window.richTextbox.Document.Blocks.Add(new Paragraph(new Run(_definitions[CurrentMod].Description)));
                    }, _uiScheduler);
                }
            }
            else
            {
                if (!fromChangeTab)
                {
                    _window.labelTitle.Content = _definitions[CurrentMod].DisplayName;
                    _window.labelTitle.ToolTip = _definitions[CurrentMod].Title;
                    _window.richTextbox.Document.Blocks.Clear();
                    _window.richTextbox.Document.Blocks.Add(new Paragraph(new Run(_definitions[CurrentMod].Description)));
                }
                _window.ImageDeFondCourante.Source = new BitmapImage(new Uri(backgroundImagePath, UriKind.Absolute));
            }

            if (fromChangeTab)
            {
                _window.labelTitle.Content = _definitions[CurrentMod].DisplayName;
                _window.labelTitle.ToolTip = _definitions[CurrentMod].Title;
                _window.richTextbox.Document.Blocks.Clear();
                _window.richTextbox.Document.Blocks.Add(new Paragraph(new Run(_definitions[CurrentMod].Description)));
            }

            // Rafraichir les MapPacks
            bool hasMaps = _definitions[CurrentMod].Contenu.Mod.Maps.Count > 0;
            ComboBoxItem itemModMaps = (ComboBoxItem)_comboboxMapPack.Items[1];
            if (hasMaps) itemModMaps.Visibility = Visibility.Visible;
            else itemModMaps.Visibility = Visibility.Collapsed;
        }

        private void RefreshGameDataIni(string modId)
        {
            // Écraser
            File.Copy(GetModCustomGameDataIni(modId), _pathToGameDataIni, true);

            // Zoom forcé
            RefreshGameDataIniZoomForce(modId, IsForceZoom, true);

            // Mode fullscreen
            RefreshGameDataIniFullscreenMode(modId);
        }

        public void RefreshGameDataIniCameraSettings()
        {
            if ((bool)Properties.Settings.Default["CurrentForceZoom"])
            {
                SetCameraSettingsInGameDataIni();
            }
        }        

        private void RefreshGameDataIniZoomForce(string modId, bool enabled, bool onlyIfEnabled = false)
        {
            if (enabled)
            {
                // * Activer le zoom forcé *
                SetCameraSettingsInGameDataIni();
            }
            else if (!onlyIfEnabled)
            {
                // * Désactiver le zoom forcé *
                IniHelper.SetGameDataIniZoomValuesFromOtherGameDataIni(GetModCustomGameDataIni(modId), _pathToGameDataIni);
            }
        }

        private void RefreshGameDataIniFullscreenMode(string modId)
        {
            bool enabled = (int)Properties.Settings.Default["FullscreenMode"] == 1;
            SetFullscreenModeInGameDataIni(enabled);
        }

        private void SetCameraSettingsInGameDataIni()
        {
            // * Activer le zoom forcé *
            int maxCameraHeight = (int)Properties.Settings.Default["ZoomMaxCameraHeight"];
            double cameraSpeed = (double)Properties.Settings.Default["ZoomCameraAdjustSpeed"];
            bool drawEntireTerrain = (bool)Properties.Settings.Default["ZoomDrawEntireTerrain"];
            IniHelper.SetGameDataIniZoomValues(_pathToGameDataIni, maxCameraHeight, cameraSpeed, drawEntireTerrain);
        }

        public void SetFullscreenModeInGameDataIni(bool windowed)
        {
            IniHelper.SetGameDataIniWindowed(_pathToGameDataIni, windowed);
        }

        private static DoubleAnimation GetAnimationActivation(double target)
        {
            return new DoubleAnimation
            {
                To = target,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = TimeSpan.FromSeconds(MainWindow.MOD_BUTTON_ACTIVATE_DURATION_IN_SECONDS),
                FillBehavior = FillBehavior.Stop
            };
        }

        public static void ChangeTabAnimation(string gameName, double target)
        {
            _modFactories[gameName].ChangeTabAnimation(target);
        }

        public void ChangeTabAnimation(double target)
        {
            if (target == 0)
            {
                Panel.SetZIndex(_comboboxMapPack, 0);
            }
            else
            {
                Panel.SetZIndex(_comboboxMapPack, 1);
            }

            DoubleAnimation animation = new DoubleAnimation
            {
                To = target,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = TimeSpan.FromSeconds(MainWindow.CHANGE_TAB_ANIMATION_DURATION_IN_SECONDS),
                FillBehavior = FillBehavior.Stop
            };
            animation.Completed += (s, a) =>
            {
                _window.labelTitle.Opacity = target;
                _window.richTextbox.Opacity = target;
                foreach (Button button2 in _modButtons.Values) button2.Opacity = target;
                foreach (Label label2 in _modLabels.Values.Where(v => v != _modLabels[CurrentMod])) label2.Opacity = target;
                _comboboxMapPack.Opacity = target;
            };
            _window.labelTitle.BeginAnimation(UIElement.OpacityProperty, animation);
            _window.richTextbox.BeginAnimation(UIElement.OpacityProperty, animation);
            foreach (Button button in _modButtons.Values) button.BeginAnimation(UIElement.OpacityProperty, animation);
            foreach (Label label in _modLabels.Values.Where(v => v != _modLabels[CurrentMod])) label.BeginAnimation(UIElement.OpacityProperty, animation);
            _comboboxMapPack.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public static void ButtonAnimation(Button button, double target, double duration)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = target,
                Duration = TimeSpan.FromSeconds(duration),
                AccelerationRatio = 0.1,
                DecelerationRatio = 0.25,
            };
            ((GrayscaleEffect.GrayscaleEffect)button.Effect).BeginAnimation(GrayscaleEffect.GrayscaleEffect.DesaturationFactorProperty, animation);
        }
        #endregion

        #region Event handlers
        private void ButtonChangeMod_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)Properties.Settings.Default[string.Format("Changing{0}Mod", _gameName)] || _window.canvasLoading.Visibility == Visibility.Visible) return;
            if (MainWindow.IsGameRunning()) return;

            Button button = (Button)sender;
            string modId = button.Name.Substring(9 + _gameName.Length);
            if (modId == CurrentMod) return;

            // Animation
            DoubleAnimation animation1 = new DoubleAnimation
            {
                To = 0,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = TimeSpan.FromSeconds(MainWindow.CHANGE_MOD_ANIMATION_DURATION_IN_SECONDS),
                FillBehavior = FillBehavior.Stop
            };
            animation1.Completed += (s, a) =>
            {
                _window.richTextbox.Opacity = 0;

                // Animation
                DoubleAnimation animation2 = new DoubleAnimation
                {
                    To = 1,
                    BeginTime = TimeSpan.FromSeconds(0),
                    Duration = TimeSpan.FromSeconds(MainWindow.CHANGE_MOD_ANIMATION_DURATION_IN_SECONDS),
                    FillBehavior = FillBehavior.Stop
                };
                animation2.Completed += (s2, a2) =>
                {
                    _window.labelTitle.Opacity = 1;
                    _window.richTextbox.Opacity = 1;
                };
                _window.labelTitle.BeginAnimation(UIElement.OpacityProperty, animation2);
                _window.richTextbox.BeginAnimation(UIElement.OpacityProperty, animation2);
            };
            _window.labelTitle.BeginAnimation(UIElement.OpacityProperty, animation1);
            _window.richTextbox.BeginAnimation(UIElement.OpacityProperty, animation1);

            // Changement de mod
            ChangeMod(modId);
            RefreshControls();
        }

        private void _comboboxMapPack_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int oldValue = (int)Properties.Settings.Default[string.Format("Current{0}MapPack", _gameName)];
            int newValue = ((ComboBox)sender).SelectedIndex;
            if (oldValue.Equals(newValue)) return;
            ChangeMapPack(newValue);
        }
        #endregion

        #region Helpers
        private Button GetButton(string modId)
        {
            return _modButtons[modId];
        }

        private void CleanPreviousMod()
        {
            // Rien à faire si original
            if (CurrentMod.Equals(ORIGINAL_MOD_ID)) return;

            // Remettre les fichiers et dossiers du mod à leur place
            foreach (string filename in _definitions[CurrentMod].Contenu.Mod.Files)
            {
                if ((_isGentool && GENTOOL_FILES.Contains(filename, StringComparer.OrdinalIgnoreCase))
                    || (_isPatch4g && PATCH4G_FILES.Contains(filename, StringComparer.OrdinalIgnoreCase)))
                {
                    continue;
                }
                MoveFile(GetGameFilePath(filename), GetModFilePath(CurrentMod, filename));
            }
            foreach (string foldername in _definitions[CurrentMod].Contenu.Mod.Folders)
            {
                MoveFolder(GetGameFolderPath(foldername), GetModFolderPath(CurrentMod, foldername));
            }

            // Restaurer les fichiers et dossiers originaux
            foreach (string filename in _definitions[CurrentMod].Contenu.Original.Files)
            {
                MoveFile(GetSafeFilePath(filename), GetGameFilePath(filename));
            }
            foreach (string foldername in _definitions[CurrentMod].Contenu.Original.Folders)
            {
                MoveFolder(GetSafeFolderPath(foldername), GetGameFolderPath(foldername));
            }
        }

        private bool ApplyNextMod(string modId)
        {
            bool res = true;
            if (!modId.Equals(ORIGINAL_MOD_ID))
            {
                // Sauvegarder les fichiers et dossiers originaux
                foreach (string filename in _definitions[modId].Contenu.Original.Files)
                {
                    MoveFile(GetGameFilePath(filename), GetSafeFilePath(filename));
                }
                foreach (string foldername in _definitions[modId].Contenu.Original.Folders)
                {
                    MoveFolder(GetGameFolderPath(foldername), GetSafeFolderPath(foldername));
                }

                // Mettre le fichiers et dossiers du mod
                foreach (string filename in _definitions[modId].Contenu.Mod.Files)
                {
                    if ((_isGentool && GENTOOL_FILES.Contains(filename, StringComparer.OrdinalIgnoreCase))
                    || (_isPatch4g && PATCH4G_FILES.Contains(filename, StringComparer.OrdinalIgnoreCase)))
                    {
                        // Désactiver le boutton launch
                        res = false;
                        continue;
                    }
                    MoveFile(GetModFilePath(modId, filename), GetGameFilePath(filename));
                }
                foreach (string foldername in _definitions[modId].Contenu.Mod.Folders)
                {
                    MoveFolder(GetModFolderPath(modId, foldername), GetGameFolderPath(foldername));
                }
            }

            // GameData.ini
            RefreshGameDataIni(modId); // rétablir paramètres

            // Sortir avec le paramètre da'ctivation
            return res;
        }

        private void ChangeMapPack(int newValue, bool noWaitCursor = false, bool noMessage = false)
        {
            // Curseur d'attente
            if (!noWaitCursor) Mouse.OverrideCursor = Cursors.Wait;

            // Initialisations
            bool messageShowed = false;
            int oldValue = (int)Properties.Settings.Default[string.Format("Current{0}MapPack", _gameName)];
            string oldValueName = ((ComboBoxItem)_comboboxMapPack.Items.GetItemAt(oldValue)).Content.ToString();
            string newValueName = ((ComboBoxItem)_comboboxMapPack.Items.GetItemAt(newValue)).Content.ToString();

            // Enlever les maps du pack précédent
            if (oldValue != 0)
            {
                List<string> mapsTodelete;
                if (oldValue == 1) mapsTodelete = _definitions[CurrentMod].Contenu.Mod.Maps;
                else mapsTodelete = _mapPacks[oldValueName];
                foreach (string mapsFoldername in mapsTodelete)
                {
                    try
                    {
                        string source = GetUserMapsFolderPath(mapsFoldername);
                        if (Directory.Exists(source))
                        {
                            DirectoryInfo di = new DirectoryInfo(source);
                            SetAttributesNormal(di);
                            di.Delete(true);
                        }
                    }
                    catch (Exception)
                    {
                        if (!noMessage && !messageShowed)
                        {
                            CustomMessageBox.Show(_window, string.Format("Problème lors de la suppression des maps du dossier Documents. Tu risques de ne pas te retrouver avec les bonnes maps.{0}{0}Pense à faire un nettoyage des maps via les options pour régler le problème.",
                                Environment.NewLine), "Suppression de map impossible", MessageBoxButton.OK, MessageBoxImage.Error);
                            messageShowed = true;
                        }
                    }
                }
            }

            // Mettre le pack
            if (newValue != 0)
            {
                if (newValue == 1)
                {
                    foreach (string mapsFoldername in _definitions[CurrentMod].Contenu.Mod.Maps)
                    {
                        try
                        {
                            string source = GetModMapsFolderPath(CurrentMod, mapsFoldername);
                            string cible = GetUserMapsFolderPath(mapsFoldername);
                            if (!Directory.Exists(cible))
                            {
                                Directory.CreateDirectory(cible);
                                CopyFolder(source, cible);
                            }
                        }
                        catch (Exception)
                        {
                            if (!noMessage && !messageShowed)
                            {
                                CustomMessageBox.Show(_window, string.Format("Problème lors de l'ajout de maps dans le dossier Documents. Il va te manquer certaines maps.{0}{0}Pense à faire un nettoyage des maps via les options pour régler le problème.",
                                    Environment.NewLine), "Ajout de map impossible", MessageBoxButton.OK, MessageBoxImage.Error);
                                messageShowed = true;
                            }
                        }
                    }
                }
                else
                {
                    foreach (string mapsFoldername in _mapPacks[newValueName])
                    {
                        try
                        {
                            string source = GetMapPackFolderPath(newValueName, mapsFoldername);
                            string cible = GetUserMapsFolderPath(mapsFoldername);
                            if (!Directory.Exists(cible))
                            {
                                Directory.CreateDirectory(cible);
                                CopyFolder(source, cible);
                            }
                        }
                        catch (Exception)
                        {
                            if (!noMessage && !messageShowed)
                            {
                                CustomMessageBox.Show(_window, string.Format("Problème lors de l'ajout de maps dans le dossier Documents. Il va te manquer certaines maps.{0}{0}Pense à faire un nettoyage des maps via les options pour régler le problème.",
                                    Environment.NewLine), "Ajout de map impossible", MessageBoxButton.OK, MessageBoxImage.Error);
                                messageShowed = true;
                            }
                        }
                    }
                }
            }

            // Mettre à jour les settings
            Properties.Settings.Default[string.Format("Current{0}MapPack", _gameName)] = newValue;
            Properties.Settings.Default.Save();

            // Remettre le curseur par défaut
            if (!noWaitCursor) Mouse.OverrideCursor = null;
        }

        private void MoveFile(string sourcePath, string destinationPath)
        {
            bool success = false;
            int i = 0;
            while (!success)
            {
                try
                {
                    File.Move(sourcePath, destinationPath);
                    success = true;
                }
                catch (IOException e)
                {
                    Thread.Sleep(DELAY_BETWEEN_IO_RETRY_MS);
                    i++;
                    if (i >= NB_OF_IO_RETRY)
                    {
                        if (RetryAccess("fichier", sourcePath, destinationPath, e.Message)) i = 0;
                        else throw;
                    }
                }
            }
        }

        // Source : http://stackoverflow.com/a/3822913
        private void CopyFolder(string sourcePath, string destinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }

        private void MoveFolder(string sourcePath, string destinationPath)
        {
            bool success = false;
            bool isSameRoot = System.IO.Path.GetPathRoot(sourcePath) == System.IO.Path.GetPathRoot(destinationPath);
            int i = 0;
            while (!success)
            {
                try
                {
                    if (isSameRoot)
                    {
                        Directory.Move(sourcePath, destinationPath);
                        success = true;
                    }
                    else
                    {
                        CopyDir.Copy(sourcePath, destinationPath);
                        Directory.Delete(sourcePath, true);
                        success = true;
                    }
                }
                catch (IOException e)
                {
                    Thread.Sleep(DELAY_BETWEEN_IO_RETRY_MS);
                    i++;
                    if (i >= NB_OF_IO_RETRY)
                    {
                        if (RetryAccess("dossier", sourcePath, destinationPath, e.Message)) i = 0;
                        else throw;
                    }
                }
            }
        }

        private bool RetryAccess(string element, string source, string destination, string exceptionMessage)
        {
            string message1 = string.Format(MESSAGE_ACCESS_PROBLEM_1, "fichier", Environment.NewLine, source, destination, exceptionMessage);
            string message2 = string.Format(MESSAGE_ACCESS_PROBLEM_2, Environment.NewLine);
            if (CustomMessageBox.Show(_window, message1, "Erreur d'accès", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) return true;
            else if (CustomMessageBox.Show(_window, message2, "Là c'est grave", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.No) return true;
            else return false;
        }

        private string GetGameFilePath(string filename)
        {
            return string.Format("{0}\\{1}", _pathToGame, filename);
        }

        private string GetGameFolderPath(string foldername)
        {
            return string.Format("{0}\\{1}", _pathToGame, foldername);
        }

        private string GetSafeFilePath(string filename)
        {
            return string.Format("{0}\\{1}\\{2}", _pathToMod, ORIGINAL_MOD_ID, filename);
        }

        private string GetSafeFolderPath(string foldername)
        {
            return string.Format("{0}\\{1}\\{2}", _pathToMod, ORIGINAL_MOD_ID, foldername);
        }

        private string GetModFilePath(string modId, string filename)
        {
            return string.Format("{0}\\{1}\\{2}", _pathToMod, modId, filename);
        }

        private string GetModFolderPath(string modId, string foldername)
        {
            return string.Format("{0}\\{1}\\{2}", _pathToMod, modId, foldername);
        }

        private string GetModMapsFolderPath(string modId, string mapsFolderName)
        {
            return string.Format("{0}\\{1}\\UserMaps\\{2}", _pathToMod, modId, mapsFolderName);
        }

        private string GetModCustomGameDataIni(string modId)
        {
            return string.Format("{0}\\{1}\\{2}", _pathToMod, modId, GAMEDATA_CUSTOM_FILE_RELATIVE_PATH);
        }

        private string GetMapPackFolderPath(string mapPackName, string mapsFolderName)
        {
            return string.Format("{0}\\{1}\\{2}", _pathToMapPacks, mapPackName, mapsFolderName);
        }

        private string GetUserMapsFolderPath(string mapsFolderName)
        {
            return string.Format("{0}\\{1}", _pathToMaps, mapsFolderName);
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

        private void LoadXml(string xmlPath)
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            using (XmlReader reader = XmlReader.Create(xmlPath, readerSettings))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(reader);

                foreach (XmlNode rootNode in xml.DocumentElement.ChildNodes)
                {
                    if (rootNode.Name.Equals("MapPacks"))
                    {
                        foreach (XmlNode gameNode in rootNode.ChildNodes)
                        {
                            if (gameNode.Name.Equals(_gameName))
                            {
                                foreach (XmlNode mapPackNode in gameNode.ChildNodes)
                                {
                                    string mapPackName = mapPackNode.Attributes["name"].Value;
                                    List<string> maps = new List<string>();
                                    foreach (XmlNode mapNode in mapPackNode.ChildNodes)
                                    {
                                        maps.Add(mapNode.InnerText);
                                    }
                                    _mapPacks.Add(mapPackName, maps);
                                }
                            }
                        }
                    }
                    else if (rootNode.Name.Equals("mods"))
                    {
                        foreach (XmlNode gameNode in rootNode.ChildNodes)
                        {
                            if (gameNode.Name.Equals(_gameName))
                            {
                                foreach (XmlNode modNode in gameNode.ChildNodes)
                                {
                                    string modId = modNode.Attributes["id"].Value;
                                    Elements elements = new Elements();
                                    elements.ImageName = modNode.Attributes["imageName"].Value;
                                    elements.DisplayName = modNode.Attributes["displayName"].Value;
                                    elements.Order = Int32.Parse(modNode.Attributes["order"].Value);
                                    elements.Contenu = new Contenu();
                                    elements.Contenu.Original = new ContenuOriginal();
                                    elements.Contenu.Original.Files = new List<string>();
                                    elements.Contenu.Original.Folders = new List<string>();
                                    elements.Contenu.Mod = new ContenuMod();
                                    elements.Contenu.Mod.Files = new List<string>();
                                    elements.Contenu.Mod.Folders = new List<string>();
                                    elements.Contenu.Mod.Maps = new List<string>();

                                    foreach (XmlNode detailNode in modNode.ChildNodes)
                                    {
                                        if (detailNode.Name.Equals("title")) elements.Title = detailNode.InnerText;
                                        else if (detailNode.Name.Equals("description")) elements.Description = detailNode.InnerText;
                                        else if (detailNode.Name.Equals("content"))
                                        {
                                            foreach (XmlNode contentNode in detailNode.ChildNodes)
                                            {
                                                if (contentNode.Name.Equals("original"))
                                                {
                                                    foreach (XmlNode originalNodes in contentNode.ChildNodes)
                                                    {
                                                        if (originalNodes.Name.Equals("files"))
                                                        {
                                                            foreach (XmlNode fileNode in originalNodes.ChildNodes)
                                                            {
                                                                elements.Contenu.Original.Files.Add(fileNode.InnerText);
                                                            }
                                                        }
                                                        else if (originalNodes.Name.Equals("folders"))
                                                        {
                                                            foreach (XmlNode folderNode in originalNodes.ChildNodes)
                                                            {
                                                                elements.Contenu.Original.Folders.Add(folderNode.InnerText);
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (contentNode.Name.Equals("mod"))
                                                {
                                                    foreach (XmlNode modNodes in contentNode.ChildNodes)
                                                    {
                                                        if (modNodes.Name.Equals("files"))
                                                        {
                                                            foreach (XmlNode fileNode in modNodes.ChildNodes)
                                                            {
                                                                elements.Contenu.Mod.Files.Add(fileNode.InnerText);
                                                            }
                                                        }
                                                        else if (modNodes.Name.Equals("folders"))
                                                        {
                                                            foreach (XmlNode folderNode in modNodes.ChildNodes)
                                                            {
                                                                elements.Contenu.Mod.Folders.Add(folderNode.InnerText);
                                                            }
                                                        }
                                                        else if (modNodes.Name.Equals("maps"))
                                                        {
                                                            foreach (XmlNode mapNode in modNodes.ChildNodes)
                                                            {
                                                                elements.Contenu.Mod.Maps.Add(mapNode.InnerText);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    _definitions.Add(modId, elements);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}