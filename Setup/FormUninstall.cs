using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Installer
{
    public partial class FormUninstall : Form
    {
        private bool _running = false;

        public FormUninstall()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonUninstall_Click(object sender, EventArgs e)
        {
            // Initialisation
            _running = true;
            string installPath = null;
            labelPoint1.Visible = true;
            buttonCancel.Enabled = false;
            buttonUninstall.Enabled = false;
            checkBoxDeleteDocumentFolder.Enabled = false;
            string zeroHourDataLeaf = null;

            // Suppression des entrées du registre
            RegistryKey keySoftware = Registry.LocalMachine.OpenSubKey("Software", true);

            RegistryKey keyEaGames = keySoftware.OpenSubKey("EA Games", true);
            if (keyEaGames != null && keyEaGames.OpenSubKey("Command and Conquer Generals Zero Hour") != null) keyEaGames.DeleteSubKeyTree("Command and Conquer Generals Zero Hour");
            if (keyEaGames.ValueCount == 0 && keyEaGames.SubKeyCount == 0) keySoftware.DeleteSubKeyTree("EA Games");

            RegistryKey keyElectronicArts = keySoftware.OpenSubKey("Electronic Arts", true);
            if (keyElectronicArts != null)
            {
                RegistryKey keyElectronicArtsEaGames = keyElectronicArts.OpenSubKey("EA GAMES", true);
                if (keyElectronicArtsEaGames != null)
                {
                    if(keyElectronicArtsEaGames.GetValue("UserDataLeafName") != null) zeroHourDataLeaf = keyElectronicArtsEaGames.GetValue("UserDataLeafName").ToString();
                    if (keyElectronicArtsEaGames.OpenSubKey("Command and Conquer Generals Zero Hour") != null) keyElectronicArtsEaGames.DeleteSubKeyTree("Command and Conquer Generals Zero Hour");
                    if (keyElectronicArtsEaGames.OpenSubKey("Generals") != null) keyElectronicArtsEaGames.DeleteSubKeyTree("Generals");
                    if (keyElectronicArtsEaGames.ValueCount == 0 && keyElectronicArtsEaGames.SubKeyCount == 0) keyElectronicArts.DeleteSubKeyTree("EA GAMES");
                }
                if (keyElectronicArts.ValueCount == 0 && keyElectronicArts.SubKeyCount == 0) keySoftware.DeleteSubKeyTree("Electronic Arts");
            }

            RegistryKey keyUge = keySoftware.OpenSubKey("GeneralsUltimateExperience");
            if(keyUge != null)
            {
                installPath = keyUge.GetValue("InstallPath").ToString();
                keySoftware.DeleteSubKeyTree("GeneralsUltimateExperience");
            }

            RegistryKey keyUninstall = Registry.LocalMachine.OpenSubKey(FormInstall.UNINSTALL_REG_KEY_PATH, true);
            if (keyUninstall != null)
            {
                keyUninstall.DeleteSubKeyTree(new Guid("f416dd93-d270-4bff-aa66-c1ce6bbf95a8").ToString("B"));
            }

            // Suppression des fichiers
            labelPoint2.Visible = true;
            if(installPath == null)
            {
                MessageBox.Show("Impossible de trouver le dossier d'installation, l'entrée dans le registre ayant été supprimée. Merci d'effacer les fichiers manuellement.", "Suppression des fichiers impossible", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if(!Directory.Exists(installPath))
                {
                    MessageBox.Show("Impossible de trouver le dossier d'installation. S'il a été déplacé veuillez effacer les fichiers manuellement.", "Suppression des fichiers impossible", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(installPath);
                    FormInstall.SetAttributesNormal(di);
                    di.Delete(true);
                }
            }

            // Suppression du raccourci bureau
            string link = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + Path.DirectorySeparatorChar + "Generals Ultimate Experience.lnk";
            if (File.Exists(link)) File.Delete(link);

            // Suppression du contenu document
            if (checkBoxDeleteDocumentFolder.Checked)
            {
                labelPoint3.Visible = true;
                string documentGeneralsPath = Environment.SpecialFolder.MyDocuments + "\\Command and Conquer Generals Data";
                string documentHeureHPath = Environment.SpecialFolder.MyDocuments + "\\" + zeroHourDataLeaf;
                if (Directory.Exists(documentGeneralsPath))
                {
                    DirectoryInfo di = new DirectoryInfo(documentGeneralsPath);
                    FormInstall.SetAttributesNormal(di);
                    di.Delete(true);
                }
                if (Directory.Exists(documentHeureHPath))
                {
                    DirectoryInfo di = new DirectoryInfo(documentHeureHPath);
                    FormInstall.SetAttributesNormal(di);
                    di.Delete(true);
                }
            }

            // Fin
            _running = false;
            labelPoint4.Visible = true;
            MessageBox.Show("Désinstallation terminée avec succès ! Au revoir.", "Fin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        private void FormUninstall_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_running) e.Cancel = true; // Interdire la fermeture de la fenêtre pendant la désinstallation
        }
    }
}