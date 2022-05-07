using System;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using MahApps.Metro.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GeneralsUltimateExperience
{
    /// <summary>
    /// Interaction logic for Resolution.xaml
    /// </summary>
    public partial class SerialNumbers : MetroWindow
    {
        private RegistryKey _keyGenerals;
        private RegistryKey _keyZeroHour;
        private string _originalGeneralsKey;
        private string _originalZeroHoursKey;
        private readonly string _pathToExe;
        private readonly TaskScheduler _uiScheduler;
        Process _process;

        public SerialNumbers(string pathToExe, TaskScheduler uiScheduler)
        {
            InitializeComponent();
            _pathToExe = pathToExe;
            _uiScheduler = uiScheduler;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxSerialGenerals.MaxLength = 25;
            textBoxSerialHeureH.MaxLength = 24;

            _keyGenerals = Registry.LocalMachine.OpenSubKey("Software\\Electronic Arts\\EA GAMES\\Generals\\ergc");
            _keyZeroHour = Registry.LocalMachine.OpenSubKey("Software\\Electronic Arts\\EA GAMES\\Command and Conquer Generals Zero Hour\\ergc");
            if(_keyGenerals == null || _keyZeroHour == null)
            {
                CustomMessageBox.Show(this, "Impossible de trouver les numéros de série dans la base de regsitre, l'installation est corrompue :-(", "Serious error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            try
            {
                string generalsKey = _keyGenerals.GetValue(null).ToString();
                _originalGeneralsKey = string.Format("{0}-{1}-{2}-{3}",
                    generalsKey.Substring(0, 4),
                    generalsKey.Substring(4, 7),
                    generalsKey.Substring(11, 7),
                    generalsKey.Substring(18, 4));
                textBoxSerialGenerals.Text = _originalGeneralsKey;
            }
            catch
            {
                textBoxSerialGenerals.Text = "XXXX-XXXXXXX-XXXXXXX-XXXX";
            }

            try
            {
                string zeroHourKey = _keyZeroHour.GetValue(null).ToString();
                _originalZeroHoursKey = string.Format("{0}-{1}-{2}-{3}-{4}",
                    zeroHourKey.Substring(0, 4),
                    zeroHourKey.Substring(4, 4),
                    zeroHourKey.Substring(8, 4),
                    zeroHourKey.Substring(12, 4),
                    zeroHourKey.Substring(16, 4));
                textBoxSerialHeureH.Text = _originalZeroHoursKey;
            }
            catch
            {
                textBoxSerialHeureH.Text = "XXXX-XXXX-XXXX-XXXX-XXXX";
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            // Déasactiver les contrôles
            SetActivationDesControles(false);

            // Validité
            textBoxSerialGenerals.Text = textBoxSerialGenerals.Text.Trim();
            textBoxSerialHeureH.Text = textBoxSerialHeureH.Text.Trim();
            textBoxSerialHeureH.Text = textBoxSerialHeureH.Text.ToUpper();

            string serialGenerals = textBoxSerialGenerals.Text;
            string[] splittedSerial = serialGenerals.Split('-');
            if (splittedSerial.Length != 4 ||
                splittedSerial[0].Length != 4 ||
                splittedSerial[1].Length != 7 ||
                splittedSerial[2].Length != 7 ||
                splittedSerial[3].Length != 4)
            {
                CustomMessageBox.Show(this, "Le n° de série Generals doit être sous la forme XXXX-XXXXXXX-XXXXXXX-XXXX", "N° de série Generals invalide", MessageBoxButton.OK, MessageBoxImage.Error);
                SetActivationDesControles(true);
                textBoxSerialGenerals.Focus();
                return;
            }
            if (!IsDigitsOnly(splittedSerial))
            {
                CustomMessageBox.Show(this, "Le n° de série Generals n'est pas valable", "N° de série Generals invalide", MessageBoxButton.OK, MessageBoxImage.Error);
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
                CustomMessageBox.Show(this, "Le n° de série HeureH doit être sous la forme XXXX-XXXX-XXXX-XXXX-XXXX", "N° de série HeureH invalide", MessageBoxButton.OK, MessageBoxImage.Error);
                SetActivationDesControles(true);
                textBoxSerialHeureH.Focus();
                return;
            }

            if (MainWindow.IsGameRunning())
            {
                CustomMessageBox.Show(this, string.Format("Changement de numéro de série impossible : le jeu est en cours d'exécution.{0}{0}Quitte d'abord le jeu (si nécessaire via Gestionnaire des tâches => fin de tâche sur generals.exe et/ou game.dat)",
                        Environment.NewLine), "Changement de numéro de série", MessageBoxButton.OK, MessageBoxImage.Error);
                SetActivationDesControles(true);
                return;
            }

            // Traitement
            if (textBoxSerialGenerals.Text.Equals(_originalGeneralsKey) && textBoxSerialHeureH.Text.Equals(_originalZeroHoursKey))
            {
                Close();
            }
            else if (CustomMessageBox.Show(this, "Tu es sur le point de modifier les numéros de série, ce qui exige une élévation des droits car ils sont inscrits dans le registre. On continue ?", "Modification du numéro de série", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _process = new Process();
                _process.StartInfo.FileName = _pathToExe + "\\Setup.exe";
                _process.StartInfo.Arguments = string.Format("-serial {0};{1}", textBoxSerialGenerals.Text.Replace("-", string.Empty), textBoxSerialHeureH.Text.Replace("-", string.Empty));
                _process.EnableRaisingEvents = true;
                _process.Exited += Process_Exited;

                try
                {
                    _process.Start();
                }
                catch
                {
                    CustomMessageBox.Show(this, "Le n° de série n'a pas pu être changé. As-tu accordé suffisamment de droits ?", "Serious error", MessageBoxButton.OK, MessageBoxImage.Error);
                    SetActivationDesControles(true);
                }
            }
            else
            {
                SetActivationDesControles(true);
            }
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                if (_process.ExitCode == 0)
                {
                    CustomMessageBox.Show(this, "Numéro(s) de série changé(s) avec succès :-)", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    CustomMessageBox.Show(this, "Le n° de série n'a pas pu être changé pour une raison inconnue :-(", "Serious error", MessageBoxButton.OK, MessageBoxImage.Error);
                    SetActivationDesControles(true);
                }
            }, CancellationToken.None, TaskCreationOptions.None, _uiScheduler);
        }

        private void SetActivationDesControles(bool enabled)
        {
            textBoxSerialGenerals.IsEnabled = enabled;
            textBoxSerialHeureH.IsEnabled = enabled;
            buttonCancel.IsEnabled = enabled;
            buttonOK.IsEnabled = enabled;
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

        private void Window_Closed(object sender, EventArgs e)
        {
            _keyGenerals.Close();
            _keyZeroHour.Close();
        }
    }
}
