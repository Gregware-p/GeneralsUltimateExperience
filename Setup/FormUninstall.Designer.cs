namespace Installer
{
    partial class FormUninstall
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUninstall));
            this.labelPoint4 = new System.Windows.Forms.Label();
            this.labelPoint2 = new System.Windows.Forms.Label();
            this.labelPoint1 = new System.Windows.Forms.Label();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonUninstall = new System.Windows.Forms.Button();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.checkBoxDeleteDocumentFolder = new System.Windows.Forms.CheckBox();
            this.labelPoint3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelPoint4
            // 
            this.labelPoint4.AutoSize = true;
            this.labelPoint4.BackColor = System.Drawing.Color.Transparent;
            this.labelPoint4.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPoint4.ForeColor = System.Drawing.Color.White;
            this.labelPoint4.Location = new System.Drawing.Point(17, 269);
            this.labelPoint4.Name = "labelPoint4";
            this.labelPoint4.Size = new System.Drawing.Size(82, 16);
            this.labelPoint4.TabIndex = 37;
            this.labelPoint4.Text = "► Terminé !";
            this.labelPoint4.Visible = false;
            // 
            // labelPoint2
            // 
            this.labelPoint2.AutoSize = true;
            this.labelPoint2.BackColor = System.Drawing.Color.Transparent;
            this.labelPoint2.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPoint2.ForeColor = System.Drawing.Color.White;
            this.labelPoint2.Location = new System.Drawing.Point(17, 227);
            this.labelPoint2.Name = "labelPoint2";
            this.labelPoint2.Size = new System.Drawing.Size(180, 16);
            this.labelPoint2.TabIndex = 36;
            this.labelPoint2.Text = "► Suppression des fichiers";
            this.labelPoint2.Visible = false;
            // 
            // labelPoint1
            // 
            this.labelPoint1.AutoSize = true;
            this.labelPoint1.BackColor = System.Drawing.Color.Transparent;
            this.labelPoint1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPoint1.ForeColor = System.Drawing.Color.White;
            this.labelPoint1.Location = new System.Drawing.Point(17, 206);
            this.labelPoint1.Name = "labelPoint1";
            this.labelPoint1.Size = new System.Drawing.Size(264, 16);
            this.labelPoint1.TabIndex = 35;
            this.labelPoint1.Text = "► Suppression des données de registre";
            this.labelPoint1.Visible = false;
            // 
            // labelSubtitle
            // 
            this.labelSubtitle.AutoSize = true;
            this.labelSubtitle.BackColor = System.Drawing.Color.Transparent;
            this.labelSubtitle.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubtitle.ForeColor = System.Drawing.Color.White;
            this.labelSubtitle.Location = new System.Drawing.Point(12, 180);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(273, 16);
            this.labelSubtitle.TabIndex = 34;
            this.labelSubtitle.Text = "Désolé de te voir partir... Grave erreur...";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.BackColor = System.Drawing.SystemColors.Highlight;
            this.buttonCancel.FlatAppearance.BorderSize = 0;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancel.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.buttonCancel.Location = new System.Drawing.Point(455, 358);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(117, 41);
            this.buttonCancel.TabIndex = 32;
            this.buttonCancel.Text = "Annuler";
            this.buttonCancel.UseVisualStyleBackColor = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonUninstall
            // 
            this.buttonUninstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUninstall.BackColor = System.Drawing.SystemColors.Highlight;
            this.buttonUninstall.FlatAppearance.BorderSize = 0;
            this.buttonUninstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonUninstall.Font = new System.Drawing.Font("Verdana", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUninstall.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.buttonUninstall.Location = new System.Drawing.Point(313, 358);
            this.buttonUninstall.Name = "buttonUninstall";
            this.buttonUninstall.Size = new System.Drawing.Size(117, 41);
            this.buttonUninstall.TabIndex = 31;
            this.buttonUninstall.Text = "Désinstaller";
            this.buttonUninstall.UseVisualStyleBackColor = false;
            this.buttonUninstall.Click += new System.EventHandler(this.buttonUninstall_Click);
            // 
            // labelCopyright
            // 
            this.labelCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCopyright.AutoSize = true;
            this.labelCopyright.BackColor = System.Drawing.Color.Transparent;
            this.labelCopyright.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCopyright.ForeColor = System.Drawing.Color.White;
            this.labelCopyright.Location = new System.Drawing.Point(-1, 395);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(110, 13);
            this.labelCopyright.TabIndex = 30;
            this.labelCopyright.Text = "© 2016 Gregware";
            // 
            // checkBoxDeleteDocumentFolder
            // 
            this.checkBoxDeleteDocumentFolder.AutoSize = true;
            this.checkBoxDeleteDocumentFolder.BackColor = System.Drawing.Color.Transparent;
            this.checkBoxDeleteDocumentFolder.Checked = true;
            this.checkBoxDeleteDocumentFolder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDeleteDocumentFolder.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxDeleteDocumentFolder.ForeColor = System.Drawing.Color.White;
            this.checkBoxDeleteDocumentFolder.Location = new System.Drawing.Point(313, 332);
            this.checkBoxDeleteDocumentFolder.Name = "checkBoxDeleteDocumentFolder";
            this.checkBoxDeleteDocumentFolder.Size = new System.Drawing.Size(266, 20);
            this.checkBoxDeleteDocumentFolder.TabIndex = 38;
            this.checkBoxDeleteDocumentFolder.Text = "Supprimer le contenu de Documents";
            this.checkBoxDeleteDocumentFolder.UseVisualStyleBackColor = false;
            // 
            // labelPoint3
            // 
            this.labelPoint3.AutoSize = true;
            this.labelPoint3.BackColor = System.Drawing.Color.Transparent;
            this.labelPoint3.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPoint3.ForeColor = System.Drawing.Color.White;
            this.labelPoint3.Location = new System.Drawing.Point(17, 248);
            this.labelPoint3.Name = "labelPoint3";
            this.labelPoint3.Size = new System.Drawing.Size(294, 16);
            this.labelPoint3.TabIndex = 39;
            this.labelPoint3.Text = "► Suppression du contenu dans Documents";
            this.labelPoint3.Visible = false;
            // 
            // FormUninstall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Setup.Properties.Resources.Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(584, 411);
            this.Controls.Add(this.labelPoint3);
            this.Controls.Add(this.checkBoxDeleteDocumentFolder);
            this.Controls.Add(this.labelPoint4);
            this.Controls.Add(this.labelPoint2);
            this.Controls.Add(this.labelPoint1);
            this.Controls.Add(this.labelSubtitle);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonUninstall);
            this.Controls.Add(this.labelCopyright);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormUninstall";
            this.Text = "Désinstallation de Generals Ultimate Experience";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormUninstall_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelPoint4;
        private System.Windows.Forms.Label labelPoint2;
        private System.Windows.Forms.Label labelPoint1;
        private System.Windows.Forms.Label labelSubtitle;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonUninstall;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.CheckBox checkBoxDeleteDocumentFolder;
        private System.Windows.Forms.Label labelPoint3;
    }
}