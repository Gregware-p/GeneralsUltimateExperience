using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MahApps.Metro.Controls;

namespace GeneralsUltimateExperience
{
    /// <summary>
    /// Interaction logic for Resolution.xaml
    /// </summary>
    public partial class Settings : MetroWindow
    {
        #region Stuff pour la détection des résolution supportées
        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(
              string deviceName, int modeNum, ref DEVMODE devMode);
        private const int ENUM_CURRENT_SETTINGS = -1;

        private const int ENUM_REGISTRY_SETTINGS = -2;

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            private const int CCHDEVICENAME = 0x20;
            private const int CCHFORMNAME = 0x20;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public ScreenOrientation dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }
        #endregion

        private struct Dimension
        {
            public int Width;
            public int Height;
        }

        private string _pathToOptionIniGenerals;
        private string _pathToOptionIniHeureH;
        private IniHelper.OptionIni _optionIniResolutionGenerals;
        private IniHelper.OptionIni _optionIniResolutionHeureH;
        private Dictionary<int, Dimension> _resolutionsItems;
        private bool _resolutionGeneralsChanged = false;
        private bool _resolutionHeureHChanged = false;
        private bool _scrollChanged = false;
        private int _zoomCameraHeightInitialValue;
        private double _zoomCameraSpeedInitialValue;
        private int _fullScreenModeInitialValue;
        private bool _isScrollWasdInitialValue;
        private bool _isZoomLibreInitialValue;
        private bool _is4gInitialValue;

        public Settings(string pathToOptionIniGenerals, string pathToOptionIniHeureH)
        {
            InitializeComponent();

            // Initialisation
            _pathToOptionIniGenerals = pathToOptionIniGenerals;
            _pathToOptionIniHeureH = pathToOptionIniHeureH;

            // Lire le fichier option.ini
            _optionIniResolutionGenerals = IniHelper.GetOptionIni(_pathToOptionIniGenerals);
            _optionIniResolutionHeureH = IniHelper.GetOptionIni(_pathToOptionIniHeureH);

            // Trouver les résolutions supportées
            List<Dimension> resolutionsDispo = new List<Dimension>();
            DEVMODE vDevMode = new DEVMODE();
            int i = 0;
            while (EnumDisplaySettings(null, i, ref vDevMode))
            {
                if (vDevMode.dmPelsWidth >= 800 && 
                    vDevMode.dmPelsHeight >= 600 && 
                    resolutionsDispo.Where(d => d.Width == vDevMode.dmPelsWidth && d.Height == vDevMode.dmPelsHeight).Count() <= 0)
                {
                    resolutionsDispo.Add(new Dimension { Width = vDevMode.dmPelsWidth, Height = vDevMode.dmPelsHeight });
                }
                i++;
            }

            // Initialiser les combobox résolution
            i = 0;
            _resolutionsItems = new Dictionary<int, Dimension>(); 
            foreach (Dimension dim in resolutionsDispo.OrderBy(d => d.Width).ThenBy(d => d.Height))
            {
                _resolutionsItems.Add(i, dim);
                string description = string.Format("{0}x{1}", dim.Width, dim.Height);
                comboBoxGenerals.Items.Add(new ComboBoxItem { Content = description });
                comboBoxHeureH.Items.Add(new ComboBoxItem { Content = description });
                i++;
            }
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            // Validité
            if (!File.Exists(_pathToOptionIniGenerals) || !File.Exists(_pathToOptionIniHeureH))
            {
                CustomMessageBox.Show(this, "Le fichier Options.ini n'a pas pu être lu, la résolution ne peut pas être modifiée.", "Options.ini introuvable", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Trouver le bon numéro de ligne
            if(_optionIniResolutionGenerals.ResolutionNoLigne < 0 || _optionIniResolutionHeureH.ResolutionNoLigne < 0)
            {
                CustomMessageBox.Show(this, "Le fichier Options.ini n'est pas valable.", "Options.ini invalide", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Voir si on peut sélectionner la bonne résolution dans la combobox
            if (_resolutionsItems.Values.Where(d => d.Width == _optionIniResolutionGenerals.ResolutionX && d.Height == _optionIniResolutionGenerals.ResolutionY).Count() > 0)
            {
                comboBoxGenerals.SelectedItem = comboBoxGenerals.SelectedIndex = _resolutionsItems.First(d => d.Value.Width == _optionIniResolutionGenerals.ResolutionX && d.Value.Height == _optionIniResolutionGenerals.ResolutionY).Key;
            }
            if (_resolutionsItems.Values.Where(d => d.Width == _optionIniResolutionHeureH.ResolutionX && d.Height == _optionIniResolutionHeureH.ResolutionY).Count() > 0)
            {
                comboBoxHeureH.SelectedItem = comboBoxHeureH.SelectedIndex = _resolutionsItems.First(d => d.Value.Width == _optionIniResolutionHeureH.ResolutionX && d.Value.Height == _optionIniResolutionHeureH.ResolutionY).Key;
            }

            // Initialiser le mode fullscreen
            _fullScreenModeInitialValue = (int)Properties.Settings.Default["FullscreenMode"];
            switch (_fullScreenModeInitialValue)
            {
                case 1:
                    radioButtonFullscreenModeOriginal.IsChecked = false;
                    radioButtonFullscreenModeGregware.IsChecked = true;
                    radioButtonFullscreenModeGentool.IsChecked = false;
                    break;
                case 2:
                    radioButtonFullscreenModeOriginal.IsChecked = false;
                    radioButtonFullscreenModeGregware.IsChecked = false;
                    radioButtonFullscreenModeGentool.IsChecked = true;
                    checkBoxOther4g.IsEnabled = false;
                    break;
                default:
                    radioButtonFullscreenModeOriginal.IsChecked = true;
                    radioButtonFullscreenModeGregware.IsChecked = false;
                    radioButtonFullscreenModeGentool.IsChecked = false;
                    break;
            }

            // Initialiser les sliders 
            _zoomCameraHeightInitialValue = (int)Properties.Settings.Default["ZoomMaxCameraHeight"];
            sliderCameraHeight.Value = _zoomCameraHeightInitialValue;
            labelCameraHeight.Content = string.Format("Hauteur caméra({0})", _zoomCameraHeightInitialValue);

            _zoomCameraSpeedInitialValue = (double)Properties.Settings.Default["ZoomCameraAdjustSpeed"];
            int cameraSpeed = (int)(_zoomCameraSpeedInitialValue * 100);
            sliderCameraSpeed.Value = cameraSpeed;
            labelCameraSpeed.Content = string.Format("Vitesse du zoom ({0}%)", cameraSpeed);

            sliderScrollFactor.Value = _optionIniResolutionHeureH.ScrollFactor;
            labelScrollFactor.Content = string.Format("Vitesse du scroll ({0}%)", (int)((int)sliderScrollFactor.Value / 145.0 * 100));

            // Initialiser le scroll wasd
            _isScrollWasdInitialValue = (bool)Properties.Settings.Default["ScrollGregware"];
            checkBoxMiscScrollWasd.IsChecked = _isScrollWasdInitialValue;

            // Initialiser le zoom libre
            _isZoomLibreInitialValue = (bool)Properties.Settings.Default["CurrentForceZoom"];
            checkBoxZoomLibre.IsChecked = _isZoomLibreInitialValue;
            if (!_isZoomLibreInitialValue)
            {
                labelCameraHeight.IsEnabled = false;
                sliderCameraHeight.IsEnabled = false;
                labelCameraSpeed.IsEnabled = false;
                sliderCameraSpeed.IsEnabled = false;
            }

            // Initialiser 4g 
            _is4gInitialValue = (bool)Properties.Settings.Default["Current4g"];
            checkBoxOther4g.IsChecked = _is4gInitialValue;
            if (_is4gInitialValue) radioButtonFullscreenModeGentool.IsEnabled = false;

            // Surveiller les changements
            comboBoxGenerals.SelectionChanged += ComboBoxGenerals_SelectionChanged;
            comboBoxHeureH.SelectionChanged += ComboBoxHeureH_SelectionChanged;
            sliderCameraHeight.ValueChanged += SliderCameraHeight_ValueChanged;
            sliderCameraSpeed.ValueChanged += SliderCameraSpeed_ValueChanged;
            sliderScrollFactor.ValueChanged += SliderScrollFactor_ValueChanged;
            checkBoxOther4g.Checked += CheckBoxOther4g_Checked;
            checkBoxOther4g.Unchecked += CheckBoxOther4g_Unchecked;
            radioButtonFullscreenModeGentool.Checked += RadioButtonFullscreenModeGentool_Checked;
            radioButtonFullscreenModeGentool.Unchecked += RadioButtonFullscreenModeGentool_Unchecked;
            checkBoxZoomLibre.Checked += CheckBoxZoomLibre_CheckedChange;
            checkBoxZoomLibre.Unchecked += CheckBoxZoomLibre_CheckedChange;
        }

        private void CheckBoxZoomLibre_CheckedChange(object sender, RoutedEventArgs e)
        {
            bool enabled = (bool)checkBoxZoomLibre.IsChecked;
            labelCameraHeight.IsEnabled = enabled;
            sliderCameraHeight.IsEnabled = enabled;
            labelCameraSpeed.IsEnabled = enabled;
            sliderCameraSpeed.IsEnabled = enabled;
        }

        private void RadioButtonFullscreenModeGentool_Unchecked(object sender, RoutedEventArgs e)
        {
            checkBoxOther4g.IsEnabled = true;
        }

        private void RadioButtonFullscreenModeGentool_Checked(object sender, RoutedEventArgs e)
        {
            checkBoxOther4g.IsEnabled = false;
        }

        private void CheckBoxOther4g_Checked(object sender, RoutedEventArgs e)
        {
            radioButtonFullscreenModeGentool.IsEnabled = false;
        }

        private void CheckBoxOther4g_Unchecked(object sender, RoutedEventArgs e)
        {
            radioButtonFullscreenModeGentool.IsEnabled = true;
        }

        private void SliderScrollFactor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelScrollFactor.Content = string.Format("Vitesse du scroll ({0}%)", (int)((int)e.NewValue / 145.0 * 100));
            _scrollChanged = true;
        }

        private void SliderCameraHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelCameraHeight.Content = string.Format("Hauteur caméra({0})", (int)e.NewValue);
        }

        private void SliderCameraSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelCameraSpeed.Content = string.Format("Vitesse du zoom ({0}%)", (int)e.NewValue);
        }

        private void ComboBoxGenerals_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _resolutionGeneralsChanged = true;
        }

        private void ComboBoxHeureH_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _resolutionHeureHChanged = true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            // Resolution
            if (_resolutionGeneralsChanged)
            {
                string[] resolution = ((ComboBoxItem)comboBoxGenerals.SelectedItem).Content.ToString().Split('x');
                IniHelper.LineChanger(string.Format("Resolution = {0} {1}", resolution[0], resolution[1]), _pathToOptionIniGenerals, _optionIniResolutionGenerals.ResolutionNoLigne);
            }
            if(_resolutionHeureHChanged)
            {
                string[] resolution = ((ComboBoxItem)comboBoxHeureH.SelectedItem).Content.ToString().Split('x');
                IniHelper.LineChanger(string.Format("Resolution = {0} {1}", resolution[0], resolution[1]), _pathToOptionIniHeureH, _optionIniResolutionHeureH.ResolutionNoLigne);
            }

            // Scroll
            if(_scrollChanged)
            {
                IniHelper.LineChanger(String.Format("ScrollFactor = {0}", sliderScrollFactor.Value), _pathToOptionIniGenerals, _optionIniResolutionGenerals.ScrollFactorNoLigne);
                IniHelper.LineChanger(String.Format("ScrollFactor = {0}", sliderScrollFactor.Value), _pathToOptionIniHeureH, _optionIniResolutionHeureH.ScrollFactorNoLigne);
            }

            // Settings
            int newCameraHeight = (int)sliderCameraHeight.Value;
            double newCameraSpeed = Math.Round(sliderCameraSpeed.Value / 100, 1);
            int fullscreenMode;
            if ((bool)radioButtonFullscreenModeOriginal.IsChecked == true) fullscreenMode = 0;
            else if ((bool)radioButtonFullscreenModeGregware.IsChecked == true) fullscreenMode = 1;
            else if ((bool)radioButtonFullscreenModeGentool.IsChecked == true) fullscreenMode = 2;
            else throw new Exception("Mode de fullscreen inconnu");
            bool isScrollWasd = (bool)checkBoxMiscScrollWasd.IsChecked;
            bool isZoomLibre = (bool)checkBoxZoomLibre.IsChecked;
            bool is4g = (bool)checkBoxOther4g.IsChecked;
            Properties.Settings.Default["ScrollGregware"] = isScrollWasd;

            // Camera
            if(newCameraHeight != _zoomCameraHeightInitialValue || newCameraSpeed != _zoomCameraSpeedInitialValue)
            {
                Properties.Settings.Default["ZoomMaxCameraHeight"] = newCameraHeight;
                Properties.Settings.Default["ZoomCameraAdjustSpeed"] = newCameraSpeed;
                Properties.Settings.Default.Save();
                ModFactory.AllGamesRefreshCameraSettings();
            }

            // Fullscreen mode
            if(fullscreenMode != _fullScreenModeInitialValue)
            {
                Properties.Settings.Default["FullscreenMode"] = fullscreenMode;
                Properties.Settings.Default.Save();
                if(_fullScreenModeInitialValue == 1 || fullscreenMode == 1) ModFactory.AllGamesRefreshFullscreenMode();
                if(_fullScreenModeInitialValue == 2 || fullscreenMode == 2) ModFactory.AllGamesRefreshGentool();
            }

            // Zoom libre
            if(isZoomLibre != _isZoomLibreInitialValue)
            {
                Properties.Settings.Default["CurrentForceZoom"] = isZoomLibre;
                Properties.Settings.Default.Save();
                ModFactory.AllGamesRefreshZoomLibre(isZoomLibre);
            }

            // Patch 4g
            if(is4g != _is4gInitialValue)
            {
                Properties.Settings.Default["Current4g"] = is4g;
                Properties.Settings.Default.Save();
                ModFactory.AllGamesRefresh4g(is4g);
            }

            // Fermer la fenêtre
            Close();
        }
    }
}