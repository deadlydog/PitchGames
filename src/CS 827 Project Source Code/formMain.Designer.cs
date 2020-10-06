namespace CS_827_Project_Source_Code
{
    partial class formMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.playerWindowsMediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            ((System.ComponentModel.ISupportInitialize)(this.playerWindowsMediaPlayer)).BeginInit();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(68, 15);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(658, 472);
            this.listBox1.TabIndex = 1;
            this.listBox1.TabStop = false;
            this.listBox1.Visible = false;
            // 
            // playerWindowsMediaPlayer
            // 
            this.playerWindowsMediaPlayer.Enabled = true;
            this.playerWindowsMediaPlayer.Location = new System.Drawing.Point(263, 507);
            this.playerWindowsMediaPlayer.Name = "playerWindowsMediaPlayer";
            this.playerWindowsMediaPlayer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("playerWindowsMediaPlayer.OcxState")));
            this.playerWindowsMediaPlayer.Size = new System.Drawing.Size(269, 58);
            this.playerWindowsMediaPlayer.TabIndex = 2;
            this.playerWindowsMediaPlayer.TabStop = false;
            this.playerWindowsMediaPlayer.Visible = false;
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 568);
            this.Controls.Add(this.playerWindowsMediaPlayer);
            this.Controls.Add(this.listBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "formMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Daniel Schroeder\'s Pitch Games";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.formMain_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.formMain_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.formMain_MouseMove);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.formMain_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.playerWindowsMediaPlayer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private AxWMPLib.AxWindowsMediaPlayer playerWindowsMediaPlayer;




    }
}

