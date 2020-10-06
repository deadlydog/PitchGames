namespace CS_827_Project_Source_Code
{
    partial class formProfileSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formProfileSettings));
            this.comboProfiles = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonNewProfile = new System.Windows.Forms.Button();
            this.buttonDeleteProfile = new System.Windows.Forms.Button();
            this.numericHighPitch = new System.Windows.Forms.NumericUpDown();
            this.numericLowPitch = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonDetectPitch = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textCurrentPitch = new System.Windows.Forms.TextBox();
            this.buttonPitchTransitionExample = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericHighPitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLowPitch)).BeginInit();
            this.SuspendLayout();
            // 
            // comboProfiles
            // 
            this.comboProfiles.FormattingEnabled = true;
            this.comboProfiles.Location = new System.Drawing.Point(15, 25);
            this.comboProfiles.MaxLength = 20;
            this.comboProfiles.Name = "comboProfiles";
            this.comboProfiles.Size = new System.Drawing.Size(160, 21);
            this.comboProfiles.TabIndex = 1;
            this.comboProfiles.SelectedIndexChanged += new System.EventHandler(this.comboProfiles_SelectedIndexChanged);
            this.comboProfiles.Leave += new System.EventHandler(this.comboProfiles_Leave);
            this.comboProfiles.Enter += new System.EventHandler(this.comboProfiles_Enter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Profile To Use";
            // 
            // buttonNewProfile
            // 
            this.buttonNewProfile.BackColor = System.Drawing.Color.Transparent;
            this.buttonNewProfile.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonNewProfile.ForeColor = System.Drawing.Color.Black;
            this.buttonNewProfile.Location = new System.Drawing.Point(181, 25);
            this.buttonNewProfile.Name = "buttonNewProfile";
            this.buttonNewProfile.Size = new System.Drawing.Size(64, 22);
            this.buttonNewProfile.TabIndex = 3;
            this.buttonNewProfile.Text = "New";
            this.buttonNewProfile.UseVisualStyleBackColor = false;
            this.buttonNewProfile.Click += new System.EventHandler(this.buttonNewProfile_Click);
            // 
            // buttonDeleteProfile
            // 
            this.buttonDeleteProfile.BackColor = System.Drawing.Color.Transparent;
            this.buttonDeleteProfile.ForeColor = System.Drawing.Color.Black;
            this.buttonDeleteProfile.Location = new System.Drawing.Point(251, 25);
            this.buttonDeleteProfile.Name = "buttonDeleteProfile";
            this.buttonDeleteProfile.Size = new System.Drawing.Size(64, 22);
            this.buttonDeleteProfile.TabIndex = 4;
            this.buttonDeleteProfile.Text = "Delete";
            this.buttonDeleteProfile.UseVisualStyleBackColor = false;
            this.buttonDeleteProfile.Click += new System.EventHandler(this.buttonDeleteProfile_Click);
            // 
            // numericHighPitch
            // 
            this.numericHighPitch.Location = new System.Drawing.Point(70, 254);
            this.numericHighPitch.Maximum = new decimal(new int[] {
            130,
            0,
            0,
            0});
            this.numericHighPitch.Name = "numericHighPitch";
            this.numericHighPitch.Size = new System.Drawing.Size(44, 20);
            this.numericHighPitch.TabIndex = 5;
            this.numericHighPitch.Value = new decimal(new int[] {
            70,
            0,
            0,
            0});
            this.numericHighPitch.ValueChanged += new System.EventHandler(this.numericHighPitch_ValueChanged);
            // 
            // numericLowPitch
            // 
            this.numericLowPitch.Location = new System.Drawing.Point(70, 280);
            this.numericLowPitch.Maximum = new decimal(new int[] {
            130,
            0,
            0,
            0});
            this.numericLowPitch.Name = "numericLowPitch";
            this.numericLowPitch.Size = new System.Drawing.Size(44, 20);
            this.numericLowPitch.TabIndex = 6;
            this.numericLowPitch.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.numericLowPitch.ValueChanged += new System.EventHandler(this.numericLowPitch_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label2.Location = new System.Drawing.Point(12, 257);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "High Pitch";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label3.Location = new System.Drawing.Point(12, 283);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Low Pitch";
            // 
            // buttonDetectPitch
            // 
            this.buttonDetectPitch.BackColor = System.Drawing.Color.Transparent;
            this.buttonDetectPitch.ForeColor = System.Drawing.Color.Black;
            this.buttonDetectPitch.Location = new System.Drawing.Point(15, 225);
            this.buttonDetectPitch.Name = "buttonDetectPitch";
            this.buttonDetectPitch.Size = new System.Drawing.Size(127, 22);
            this.buttonDetectPitch.TabIndex = 9;
            this.buttonDetectPitch.Text = "Detect Pitch";
            this.buttonDetectPitch.UseVisualStyleBackColor = false;
            this.buttonDetectPitch.Click += new System.EventHandler(this.buttonDetectPitch_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.BackColor = System.Drawing.Color.Transparent;
            this.buttonClose.ForeColor = System.Drawing.Color.Black;
            this.buttonClose.Location = new System.Drawing.Point(15, 441);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(90, 47);
            this.buttonClose.TabIndex = 10;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = false;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label4.Location = new System.Drawing.Point(12, 58);
            this.label4.MaximumSize = new System.Drawing.Size(300, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(286, 26);
            this.label4.TabIndex = 11;
            this.label4.Text = "1 - Click the \"Detect Pitch\" button to have the system start detecting the pitch " +
                "range of your voice";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label5.Location = new System.Drawing.Point(120, 270);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Current Pitch";
            // 
            // textCurrentPitch
            // 
            this.textCurrentPitch.Enabled = false;
            this.textCurrentPitch.ForeColor = System.Drawing.Color.Black;
            this.textCurrentPitch.Location = new System.Drawing.Point(191, 267);
            this.textCurrentPitch.Name = "textCurrentPitch";
            this.textCurrentPitch.Size = new System.Drawing.Size(41, 20);
            this.textCurrentPitch.TabIndex = 13;
            // 
            // buttonPitchTransitionExample
            // 
            this.buttonPitchTransitionExample.BackColor = System.Drawing.Color.Transparent;
            this.buttonPitchTransitionExample.ForeColor = System.Drawing.Color.Black;
            this.buttonPitchTransitionExample.Location = new System.Drawing.Point(148, 225);
            this.buttonPitchTransitionExample.Name = "buttonPitchTransitionExample";
            this.buttonPitchTransitionExample.Size = new System.Drawing.Size(145, 22);
            this.buttonPitchTransitionExample.TabIndex = 14;
            this.buttonPitchTransitionExample.Text = "Pitch Transition Example";
            this.buttonPitchTransitionExample.UseVisualStyleBackColor = false;
            this.buttonPitchTransitionExample.Click += new System.EventHandler(this.buttonPitchTransitionExample_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label6.Location = new System.Drawing.Point(12, 86);
            this.label6.MaximumSize = new System.Drawing.Size(300, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(298, 39);
            this.label6.TabIndex = 15;
            this.label6.Text = "2 - Start by making a low-pitch tone into the microphone with your voice, then gr" +
                "adually move up to a high-pitch tone.  Click the \"Pitch Transition Example\" butt" +
                "on to hear an example";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label7.Location = new System.Drawing.Point(13, 139);
            this.label7.MaximumSize = new System.Drawing.Size(300, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(291, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "3 - Click the \"Detect Pitch\" button again to stop the dection.";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label8.Location = new System.Drawing.Point(21, 327);
            this.label8.MaximumSize = new System.Drawing.Size(300, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(291, 26);
            this.label8.TabIndex = 17;
            this.label8.Text = "- If the \"Current Pitch\" value is not changing, try \"speaking\" louder into the mi" +
                "crophone";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.Transparent;
            this.label9.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label9.Location = new System.Drawing.Point(13, 155);
            this.label9.MaximumSize = new System.Drawing.Size(260, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(259, 52);
            this.label9.TabIndex = 18;
            this.label9.Text = "4 - Adjust the High and Low Pitch manually until you can comfortably move the Pit" +
                "ch slider from the bottom to the top of the scale.  The difference between the H" +
                "igh and Low Pitches must be at least 10";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.Color.Transparent;
            this.label10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label10.Location = new System.Drawing.Point(20, 355);
            this.label10.MaximumSize = new System.Drawing.Size(300, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(284, 13);
            this.label10.TabIndex = 19;
            this.label10.Text = "- This and the game should be done in a quiet environment";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.Color.Transparent;
            this.label11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label11.Location = new System.Drawing.Point(21, 370);
            this.label11.MaximumSize = new System.Drawing.Size(300, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(298, 26);
            this.label11.TabIndex = 20;
            this.label11.Text = "- If an unintentional noise disrupts the detection, just stop and start it again";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.BackColor = System.Drawing.Color.Transparent;
            this.label12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label12.Location = new System.Drawing.Point(13, 312);
            this.label12.MaximumSize = new System.Drawing.Size(300, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(27, 13);
            this.label12.TabIndex = 21;
            this.label12.Text = "Tips";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.BackColor = System.Drawing.Color.Transparent;
            this.label13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label13.Location = new System.Drawing.Point(21, 398);
            this.label13.MaximumSize = new System.Drawing.Size(300, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(291, 26);
            this.label13.TabIndex = 22;
            this.label13.Text = "- The larger the range between the High and Low Pitch, the more precision you wil" +
                "l have with the Pitch Meter";
            // 
            // formProfileSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(594, 496);
            this.ControlBox = false;
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.buttonPitchTransitionExample);
            this.Controls.Add(this.textCurrentPitch);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonDetectPitch);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numericLowPitch);
            this.Controls.Add(this.numericHighPitch);
            this.Controls.Add(this.buttonDeleteProfile);
            this.Controls.Add(this.buttonNewProfile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboProfiles);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formProfileSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Profile Settings";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.formProfileSettings_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.numericHighPitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericLowPitch)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboProfiles;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonNewProfile;
        private System.Windows.Forms.Button buttonDeleteProfile;
        private System.Windows.Forms.NumericUpDown numericHighPitch;
        private System.Windows.Forms.NumericUpDown numericLowPitch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonDetectPitch;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textCurrentPitch;
        private System.Windows.Forms.Button buttonPitchTransitionExample;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;

    }
}