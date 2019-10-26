using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GeneralsUltimateExperience
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : MetroWindow
    {
        public UpdateWindow(MainWindow.Version localVersion, MainWindow.Version remoteVersion, List<MainWindow.Update> updateList)
        {
            InitializeComponent();
            IsOK = false;

            labelActuel.Content = string.Format("Version actuellement installée : {0}.{1}", localVersion.Major, localVersion.Minor);
            labelDispo.Content = string.Format("Dernière version disponible : {0}.{1}", remoteVersion.Major, remoteVersion.Minor);

            StringBuilder sb = new StringBuilder();
            foreach(MainWindow.Update update in updateList.Where(
                u => u.Version.Major > localVersion.Major || 
                (u.Version.Major == localVersion.Major && u.Version.Minor > localVersion.Minor)).OrderBy(
                u => u.Version.Major).ThenBy
                (u => u.Version.Minor))
            {
                sb.AppendLine(string.Format("*** VERSION {0}.{1} ***", update.Version.Major, update.Version.Minor));
                sb.Append(update.ChangeLog);
                sb.AppendLine();
                sb.AppendLine();
            }
            textBox.Text = sb.ToString();
        }

        public bool IsOK
        {
            get; private set;
        }

        private void buttonInstall_Click(object sender, RoutedEventArgs e)
        {
            IsOK = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
