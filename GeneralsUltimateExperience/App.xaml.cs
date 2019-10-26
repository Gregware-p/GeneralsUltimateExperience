using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Diagnostics;

namespace GeneralsUltimateExperience
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private bool _contentLoaded;

        #region Stuff pour changer le focus à un autre process
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);
        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);
        public static void BringWindowToFront(Process bProcess)
        {
            //get the  hWnd of the process
            IntPtr hwnd = bProcess.MainWindowHandle;
            if (hwnd == IntPtr.Zero)
            {
                //the window is hidden so try to restore it before setting focus.
                ShowWindow(bProcess.Handle, ShowWindowEnum.Restore);
            }

            //set user the focus to the window
            SetForegroundWindow(bProcess.MainWindowHandle);
        }
        #endregion

        [STAThread]
        public static void Main()
        {
            // Vérification de registre
            RegistryKey keyUge = Registry.LocalMachine.OpenSubKey("Software\\GeneralsUltimateExperience");
            RegistryKey keyEaGames = Registry.LocalMachine.OpenSubKey("Software\\EA Games\\Command and Conquer Generals Zero Hour");
            RegistryKey keyGenerals = Registry.LocalMachine.OpenSubKey("Software\\Electronic Arts\\EA GAMES\\Generals");
            RegistryKey keyZeroHour = Registry.LocalMachine.OpenSubKey("Software\\Electronic Arts\\EA GAMES\\Command and Conquer Generals Zero Hour");
            if (keyUge == null || keyEaGames == null || keyGenerals == null || keyZeroHour == null)
            {
                CustomMessageBox.Show("Votre installation de Generals Ultimate Experience est corrompue, Veuillez réinstaller.", "Installation incorrecte", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Si application déjà lancée lui passer le focus
            String thisprocessname = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
            {
                Process process = Process.GetProcesses().Where(p => p.ProcessName == thisprocessname).First();
                BringWindowToFront(process);
                return;
            }

            // Lancer l'application
            var application = new App();
            application.InitializeComponentCustom();
            application.Run();
        }

        

        public void InitializeComponentCustom()
        {
            if (_contentLoaded)
            {
                return;
            }
            _contentLoaded = true;

            SplashScreen splashScreen = new SplashScreen("Images/Splash.png");
            splashScreen.Show(true);

            this.StartupUri = new System.Uri("MainWindow.xaml", System.UriKind.Relative);
            System.Uri resourceLocater = new System.Uri("app.xaml", System.UriKind.Relative);
            System.Windows.Application.LoadComponent(this, resourceLocater);
        }
    }
}