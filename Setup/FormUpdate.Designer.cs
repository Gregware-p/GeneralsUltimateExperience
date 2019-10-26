namespace Installer
{
    partial class FormUpdate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUpdate));
            this.labelCopyright = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelAvancement = new System.Windows.Forms.Label();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.labelPoint1 = new System.Windows.Forms.Label();
            this.labelPoint3 = new System.Windows.Forms.Label();
            this.labelPoint4 = new System.Windows.Forms.Label();
            this.labelPoint5 = new System.Windows.Forms.Label();
            this.labelPoint2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelCopyright
            // 
            this.labelCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelCopyright.AutoSize = true;
            this.labelCopyright.BackColor = System.Drawing.Color.Transparent;
            this.labelCopyright.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCopyright.ForeColor = System.Drawing.Color.White;
            this.labelCopyright.Location = new System.Drawing.Point(0, 474);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(110, 13);
            this.labelCopyright.TabIndex = 10;
            this.labelCopyright.Text = "© 2016 Gregware";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.ForeColor = System.Drawing.Color.White;
            this.progressBar.Location = new System.Drawing.Point(12, 414);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(668, 26);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 9;
            // 
            // labelAvancement
            // 
            this.labelAvancement.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelAvancement.AutoSize = true;
            this.labelAvancement.BackColor = System.Drawing.Color.Transparent;
            this.labelAvancement.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAvancement.ForeColor = System.Drawing.Color.White;
            this.labelAvancement.Location = new System.Drawing.Point(12, 393);
            this.labelAvancement.Name = "labelAvancement";
            this.labelAvancement.Size = new System.Drawing.Size(185, 18);
            this.labelAvancement.TabIndex = 8;
            this.labelAvancement.Text = "Avancement global :";
            // 
            // labelSubtitle
            // 
            this.labelSubtitle.AutoSize = true;
            this.labelSubtitle.BackColor = System.Drawing.Color.Transparent;
            this.labelSubtitle.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSubtitle.ForeColor = System.Drawing.Color.White;
            this.labelSubtitle.Location = new System.Drawing.Point(12, 218);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(271, 16);
            this.labelSubtitle.TabIndex = 14;
            this.labelSubtitle.Text = "Mise à jour version {0}.{1} => {2}.{3}";
            this.labelSubtitle.Visible = false;
            // 
            // labelPoint1
            // 
            this.labelPoint1.AutoSize = true;
            this.labelPoint1.BackColor = System.Drawing.Color.Transparent;
            this.labelPoint1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPoint1.ForeColor = System.Drawing.Color.White;
            this.labelPoint1.Location = new System.Drawing.Point(17, 244);
            this.labelPoint1.Name = "labelPoint1";
            this.labelPoint1.Size = new System.Drawing.Size(241, 16);
            this.labelPoint1.TabIndex = 15;
            this.labelPoint1.Text = "► Téléchargement de la mise à jour";
            this.labelPoint1.Visible = false;
            // 
            // labelPoint3
            // 
            this.labelPoint3.AutoSize = true;
            this.labelPoint3.BackColor = System.Drawing.Color.Transparent;
            this.labelPoint3.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPoint3.ForeColor = System.Drawing.Color.White;
            this.labelPoint3.Location = new System.Drawing.Point(17, 286);
            this.labelPoint3.Name = "labelPoint3";
            this.labelPoint3.Size = new System.Drawing.Size(205, 16);
            this.labelPoint3.TabIndex = 16;
            this.labelPoint3.Text = "► Copie des nouveaux fichiers";
            this.labelPoint3.Visible = false;
            // 
            // labelPoint4
            // 
            this.labelPoint4.AutoSize = true;
            this.labelPoint4.BackColor = System.Drawing.Color.Transparent;
            this.labelPoint4.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPoint4.ForeColor = System.Drawing.Color.White;
            this.labelPoint4.Location = new System.Drawing.Point(17, 307);
            this.labelPoint4.Name = "labelPoint4";
            this.labelPoint4.Size = new System.Drawing.Size(169, 16);
            this.labelPoint4.TabIndex = 17;
            this.labelPoint4.Text = "► Mise à jour du registre";
            this.labelPoint4.Visible = false;
            // 
            // labelPoint5
            // 
            this.labelPoint5.AutoSize = true;
            this.labelPoint5.BackColor = System.Drawing.Color.Transparent;
            this.labelPoint5.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPoint5.ForeColor = System.Drawing.Color.White;
            this.labelPoint5.Location = new System.Drawing.Point(17, 328);
            this.labelPoint5.Name = "labelPoint5";
            this.labelPoint5.Size = new System.Drawing.Size(82, 16);
            this.labelPoint5.TabIndex = 18;
            this.labelPoint5.Text = "► Terminé !";
            this.labelPoint5.Visible = false;
            // 
            // labelPoint2
            // 
            this.labelPoint2.AutoSize = true;
            this.labelPoint2.BackColor = System.Drawing.Color.Transparent;
            this.labelPoint2.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPoint2.ForeColor = System.Drawing.Color.White;
            this.labelPoint2.Location = new System.Drawing.Point(17, 265);
            this.labelPoint2.Name = "labelPoint2";
            this.labelPoint2.Size = new System.Drawing.Size(248, 16);
            this.labelPoint2.TabIndex = 19;
            this.labelPoint2.Text = "► Suppression des fichiers obsolètes";
            this.labelPoint2.Visible = false;
            // 
            // FormUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = global::Setup.Properties.Resources.Background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(692, 491);
            this.Controls.Add(this.labelPoint2);
            this.Controls.Add(this.labelPoint5);
            this.Controls.Add(this.labelPoint4);
            this.Controls.Add(this.labelPoint3);
            this.Controls.Add(this.labelPoint1);
            this.Controls.Add(this.labelSubtitle);
            this.Controls.Add(this.labelAvancement);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.labelCopyright);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormUpdate";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Mise à jour de Generals Ultimate Experience";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Shown += new System.EventHandler(this.FormUpdate_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelAvancement;
        private System.Windows.Forms.Label labelSubtitle;
        private System.Windows.Forms.Label labelPoint1;
        private System.Windows.Forms.Label labelPoint3;
        private System.Windows.Forms.Label labelPoint4;
        private System.Windows.Forms.Label labelPoint5;
        private System.Windows.Forms.Label labelPoint2;
    }
}

