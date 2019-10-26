namespace Installer
{
    partial class FormInstall
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormInstall));
            this.labelDossierInatallation = new System.Windows.Forms.Label();
            this.textBoxInstallpath = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.buttonInstall = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelAvancement = new System.Windows.Forms.Label();
            this.textBoxSerialGenerals = new System.Windows.Forms.TextBox();
            this.labelSerialGenerals = new System.Windows.Forms.Label();
            this.textBoxSerialHeureH = new System.Windows.Forms.TextBox();
            this.labelSerialHeureH = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelDossierInatallation
            // 
            this.labelDossierInatallation.AutoSize = true;
            this.labelDossierInatallation.BackColor = System.Drawing.Color.Transparent;
            this.labelDossierInatallation.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDossierInatallation.ForeColor = System.Drawing.Color.White;
            this.labelDossierInatallation.Location = new System.Drawing.Point(12, 289);
            this.labelDossierInatallation.Name = "labelDossierInatallation";
            this.labelDossierInatallation.Size = new System.Drawing.Size(171, 18);
            this.labelDossierInatallation.TabIndex = 5;
            this.labelDossierInatallation.Text = "Dossier d\'installation :";
            // 
            // textBoxInstallpath
            // 
            this.textBoxInstallpath.BackColor = System.Drawing.Color.Black;
            this.textBoxInstallpath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxInstallpath.ForeColor = System.Drawing.Color.White;
            this.textBoxInstallpath.Location = new System.Drawing.Point(15, 310);
            this.textBoxInstallpath.Name = "textBoxInstallpath";
            this.textBoxInstallpath.Size = new System.Drawing.Size(645, 26);
            this.textBoxInstallpath.TabIndex = 6;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(666, 311);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(26, 26);
            this.buttonBrowse.TabIndex = 7;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // labelCopyright
            // 
            this.labelCopyright.AutoSize = true;
            this.labelCopyright.BackColor = System.Drawing.Color.Transparent;
            this.labelCopyright.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCopyright.ForeColor = System.Drawing.Color.White;
            this.labelCopyright.Location = new System.Drawing.Point(-1, 485);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(110, 13);
            this.labelCopyright.TabIndex = 10;
            this.labelCopyright.Text = "© 2016 Gregware";
            // 
            // buttonInstall
            // 
            this.buttonInstall.BackColor = System.Drawing.SystemColors.Highlight;
            this.buttonInstall.FlatAppearance.BorderSize = 0;
            this.buttonInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonInstall.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonInstall.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.buttonInstall.Location = new System.Drawing.Point(433, 448);
            this.buttonInstall.Name = "buttonInstall";
            this.buttonInstall.Size = new System.Drawing.Size(117, 41);
            this.buttonInstall.TabIndex = 11;
            this.buttonInstall.Text = "Installer";
            this.buttonInstall.UseVisualStyleBackColor = false;
            this.buttonInstall.Click += new System.EventHandler(this.buttonInstall_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.SystemColors.Highlight;
            this.buttonCancel.FlatAppearance.BorderSize = 0;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.buttonCancel.Location = new System.Drawing.Point(575, 448);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(117, 41);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Annuler";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // progressBar
            // 
            this.progressBar.ForeColor = System.Drawing.Color.White;
            this.progressBar.Location = new System.Drawing.Point(15, 404);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(677, 26);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 9;
            // 
            // labelAvancement
            // 
            this.labelAvancement.AutoSize = true;
            this.labelAvancement.BackColor = System.Drawing.Color.Transparent;
            this.labelAvancement.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAvancement.ForeColor = System.Drawing.Color.White;
            this.labelAvancement.Location = new System.Drawing.Point(12, 383);
            this.labelAvancement.Name = "labelAvancement";
            this.labelAvancement.Size = new System.Drawing.Size(220, 18);
            this.labelAvancement.TabIndex = 8;
            this.labelAvancement.Text = "Téléchargement (étape 1/2)";
            this.labelAvancement.Visible = false;
            // 
            // textBoxSerialGenerals
            // 
            this.textBoxSerialGenerals.BackColor = System.Drawing.Color.Black;
            this.textBoxSerialGenerals.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSerialGenerals.ForeColor = System.Drawing.Color.White;
            this.textBoxSerialGenerals.Location = new System.Drawing.Point(15, 246);
            this.textBoxSerialGenerals.Name = "textBoxSerialGenerals";
            this.textBoxSerialGenerals.Size = new System.Drawing.Size(292, 26);
            this.textBoxSerialGenerals.TabIndex = 2;
            // 
            // labelSerialGenerals
            // 
            this.labelSerialGenerals.AutoSize = true;
            this.labelSerialGenerals.BackColor = System.Drawing.Color.Transparent;
            this.labelSerialGenerals.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSerialGenerals.ForeColor = System.Drawing.Color.White;
            this.labelSerialGenerals.Location = new System.Drawing.Point(12, 225);
            this.labelSerialGenerals.Name = "labelSerialGenerals";
            this.labelSerialGenerals.Size = new System.Drawing.Size(172, 18);
            this.labelSerialGenerals.TabIndex = 1;
            this.labelSerialGenerals.Text = "N° de série Generals :";
            // 
            // textBoxSerialHeureH
            // 
            this.textBoxSerialHeureH.BackColor = System.Drawing.Color.Black;
            this.textBoxSerialHeureH.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSerialHeureH.ForeColor = System.Drawing.Color.White;
            this.textBoxSerialHeureH.Location = new System.Drawing.Point(400, 246);
            this.textBoxSerialHeureH.Name = "textBoxSerialHeureH";
            this.textBoxSerialHeureH.Size = new System.Drawing.Size(292, 26);
            this.textBoxSerialHeureH.TabIndex = 4;
            // 
            // labelSerialHeureH
            // 
            this.labelSerialHeureH.AutoSize = true;
            this.labelSerialHeureH.BackColor = System.Drawing.Color.Transparent;
            this.labelSerialHeureH.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSerialHeureH.ForeColor = System.Drawing.Color.White;
            this.labelSerialHeureH.Location = new System.Drawing.Point(397, 225);
            this.labelSerialHeureH.Name = "labelSerialHeureH";
            this.labelSerialHeureH.Size = new System.Drawing.Size(162, 18);
            this.labelSerialHeureH.TabIndex = 3;
            this.labelSerialHeureH.Text = "N° de série HeureH :";
            // 
            // FormInstall
            // 
            this.AcceptButton = this.buttonInstall;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::Setup.Properties.Resources.Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(704, 501);
            this.Controls.Add(this.textBoxSerialHeureH);
            this.Controls.Add(this.labelSerialHeureH);
            this.Controls.Add(this.textBoxSerialGenerals);
            this.Controls.Add(this.labelSerialGenerals);
            this.Controls.Add(this.labelAvancement);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonInstall);
            this.Controls.Add(this.labelCopyright);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxInstallpath);
            this.Controls.Add(this.labelDossierInatallation);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormInstall";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Installation de Generals Ultimate Experience";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDossierInatallation;
        private System.Windows.Forms.TextBox textBoxInstallpath;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Button buttonInstall;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelAvancement;
        private System.Windows.Forms.TextBox textBoxSerialGenerals;
        private System.Windows.Forms.Label labelSerialGenerals;
        private System.Windows.Forms.TextBox textBoxSerialHeureH;
        private System.Windows.Forms.Label labelSerialHeureH;
    }
}

