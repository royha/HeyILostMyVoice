namespace HeyILostMyVoice
{
    partial class HeyILostMyVoiceForm
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
            this.components = new System.ComponentModel.Container();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.panelControls = new System.Windows.Forms.Panel();
            this.buttonSettings = new System.Windows.Forms.Button();
            this.comboBoxParaVoice = new System.Windows.Forms.ComboBox();
            this.trackBarParaVolume = new System.Windows.Forms.TrackBar();
            this.trackBarParaRate = new System.Windows.Forms.TrackBar();
            this.checkBoxSpeakOnPara = new System.Windows.Forms.CheckBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonSkipAhead = new System.Windows.Forms.Button();
            this.buttonPlayPause = new System.Windows.Forms.Button();
            this.buttonSkipBack = new System.Windows.Forms.Button();
            this.trackBarWordVolume = new System.Windows.Forms.TrackBar();
            this.trackBarWordRate = new System.Windows.Forms.TrackBar();
            this.checkBoxSpeakOnWord = new System.Windows.Forms.CheckBox();
            this.labelVolume = new System.Windows.Forms.Label();
            this.labelRate = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarParaVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarParaRate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWordVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWordRate)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.HideSelection = false;
            this.richTextBox1.Location = new System.Drawing.Point(12, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(497, 351);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBox1_KeyDown);
            this.richTextBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richTextBox1_KeyPress);
            // 
            // panelControls
            // 
            this.panelControls.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.panelControls.Controls.Add(this.buttonSettings);
            this.panelControls.Controls.Add(this.comboBoxParaVoice);
            this.panelControls.Controls.Add(this.trackBarParaVolume);
            this.panelControls.Controls.Add(this.trackBarParaRate);
            this.panelControls.Controls.Add(this.checkBoxSpeakOnPara);
            this.panelControls.Controls.Add(this.buttonStop);
            this.panelControls.Controls.Add(this.buttonSkipAhead);
            this.panelControls.Controls.Add(this.buttonPlayPause);
            this.panelControls.Controls.Add(this.buttonSkipBack);
            this.panelControls.Controls.Add(this.trackBarWordVolume);
            this.panelControls.Controls.Add(this.trackBarWordRate);
            this.panelControls.Controls.Add(this.checkBoxSpeakOnWord);
            this.panelControls.Controls.Add(this.labelVolume);
            this.panelControls.Controls.Add(this.labelRate);
            this.panelControls.Location = new System.Drawing.Point(12, 369);
            this.panelControls.Name = "panelControls";
            this.panelControls.Size = new System.Drawing.Size(497, 89);
            this.panelControls.TabIndex = 1000;
            // 
            // buttonSettings
            // 
            this.buttonSettings.BackgroundImage = global::HeyILostMyVoice.Properties.Resources.cog_2x;
            this.buttonSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonSettings.Enabled = false;
            this.buttonSettings.Location = new System.Drawing.Point(470, 18);
            this.buttonSettings.Name = "buttonSettings";
            this.buttonSettings.Size = new System.Drawing.Size(24, 24);
            this.buttonSettings.TabIndex = 9;
            this.toolTip1.SetToolTip(this.buttonSettings, "Settings");
            this.buttonSettings.UseVisualStyleBackColor = true;
            // 
            // comboBoxParaVoice
            // 
            this.comboBoxParaVoice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxParaVoice.FormattingEnabled = true;
            this.comboBoxParaVoice.Location = new System.Drawing.Point(333, 49);
            this.comboBoxParaVoice.Name = "comboBoxParaVoice";
            this.comboBoxParaVoice.Size = new System.Drawing.Size(161, 21);
            this.comboBoxParaVoice.TabIndex = 13;
            this.toolTip1.SetToolTip(this.comboBoxParaVoice, "Voice");
            this.comboBoxParaVoice.SelectedIndexChanged += new System.EventHandler(this.comboBoxParaVoice_SelectedIndexChanged);
            // 
            // trackBarParaVolume
            // 
            this.trackBarParaVolume.LargeChange = 10;
            this.trackBarParaVolume.Location = new System.Drawing.Point(220, 51);
            this.trackBarParaVolume.Maximum = 100;
            this.trackBarParaVolume.Name = "trackBarParaVolume";
            this.trackBarParaVolume.Size = new System.Drawing.Size(104, 45);
            this.trackBarParaVolume.SmallChange = 10;
            this.trackBarParaVolume.TabIndex = 12;
            this.trackBarParaVolume.TickFrequency = 10;
            this.toolTip1.SetToolTip(this.trackBarParaVolume, "Volume of speech for speak on enter, speak selection, and speak all text");
            this.trackBarParaVolume.Value = 100;
            this.trackBarParaVolume.Scroll += new System.EventHandler(this.trackBarParaVolume_Scroll);
            // 
            // trackBarParaRate
            // 
            this.trackBarParaRate.LargeChange = 1;
            this.trackBarParaRate.Location = new System.Drawing.Point(110, 51);
            this.trackBarParaRate.Minimum = -10;
            this.trackBarParaRate.Name = "trackBarParaRate";
            this.trackBarParaRate.Size = new System.Drawing.Size(104, 45);
            this.trackBarParaRate.TabIndex = 11;
            this.trackBarParaRate.TickFrequency = 5;
            this.toolTip1.SetToolTip(this.trackBarParaRate, "Rate of speech for speak on enter, speak selection, and speak all text");
            this.trackBarParaRate.Value = -1;
            this.trackBarParaRate.Scroll += new System.EventHandler(this.trackBarParaRate_Scroll);
            // 
            // checkBoxSpeakOnPara
            // 
            this.checkBoxSpeakOnPara.AutoSize = true;
            this.checkBoxSpeakOnPara.Checked = true;
            this.checkBoxSpeakOnPara.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSpeakOnPara.Location = new System.Drawing.Point(3, 51);
            this.checkBoxSpeakOnPara.Name = "checkBoxSpeakOnPara";
            this.checkBoxSpeakOnPara.Size = new System.Drawing.Size(103, 17);
            this.checkBoxSpeakOnPara.TabIndex = 10;
            this.checkBoxSpeakOnPara.Text = "Speak on Enter:";
            this.toolTip1.SetToolTip(this.checkBoxSpeakOnPara, "Speak the paragraph when Enter is pressed");
            this.checkBoxSpeakOnPara.UseVisualStyleBackColor = true;
            this.checkBoxSpeakOnPara.CheckedChanged += new System.EventHandler(this.checkBoxSpeakOnPara_CheckedChanged);
            // 
            // buttonStop
            // 
            this.buttonStop.BackgroundImage = global::HeyILostMyVoice.Properties.Resources.media_stop_2x;
            this.buttonStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonStop.Location = new System.Drawing.Point(423, 18);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(24, 24);
            this.buttonStop.TabIndex = 8;
            this.toolTip1.SetToolTip(this.buttonStop, "Stop");
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonSkipAhead
            // 
            this.buttonSkipAhead.BackgroundImage = global::HeyILostMyVoice.Properties.Resources.media_skip_forward_2x;
            this.buttonSkipAhead.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonSkipAhead.Location = new System.Drawing.Point(393, 18);
            this.buttonSkipAhead.Name = "buttonSkipAhead";
            this.buttonSkipAhead.Size = new System.Drawing.Size(24, 24);
            this.buttonSkipAhead.TabIndex = 7;
            this.toolTip1.SetToolTip(this.buttonSkipAhead, "Skip ahead");
            this.buttonSkipAhead.UseVisualStyleBackColor = true;
            this.buttonSkipAhead.Click += new System.EventHandler(this.buttonSkipAhead_Click);
            // 
            // buttonPlayPause
            // 
            this.buttonPlayPause.BackgroundImage = global::HeyILostMyVoice.Properties.Resources.media_play_2x;
            this.buttonPlayPause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonPlayPause.Location = new System.Drawing.Point(363, 18);
            this.buttonPlayPause.Name = "buttonPlayPause";
            this.buttonPlayPause.Size = new System.Drawing.Size(24, 24);
            this.buttonPlayPause.TabIndex = 6;
            this.toolTip1.SetToolTip(this.buttonPlayPause, "Play");
            this.buttonPlayPause.UseVisualStyleBackColor = true;
            this.buttonPlayPause.Click += new System.EventHandler(this.buttonPlayPause_Click);
            // 
            // buttonSkipBack
            // 
            this.buttonSkipBack.BackgroundImage = global::HeyILostMyVoice.Properties.Resources.media_skip_backward_2x;
            this.buttonSkipBack.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.buttonSkipBack.Location = new System.Drawing.Point(333, 18);
            this.buttonSkipBack.Name = "buttonSkipBack";
            this.buttonSkipBack.Size = new System.Drawing.Size(24, 24);
            this.buttonSkipBack.TabIndex = 5;
            this.toolTip1.SetToolTip(this.buttonSkipBack, "Skip back");
            this.buttonSkipBack.UseVisualStyleBackColor = true;
            this.buttonSkipBack.Click += new System.EventHandler(this.buttonSkipBack_Click);
            // 
            // trackBarWordVolume
            // 
            this.trackBarWordVolume.LargeChange = 10;
            this.trackBarWordVolume.Location = new System.Drawing.Point(220, 23);
            this.trackBarWordVolume.Maximum = 100;
            this.trackBarWordVolume.Name = "trackBarWordVolume";
            this.trackBarWordVolume.Size = new System.Drawing.Size(104, 45);
            this.trackBarWordVolume.SmallChange = 10;
            this.trackBarWordVolume.TabIndex = 4;
            this.trackBarWordVolume.TickFrequency = 10;
            this.toolTip1.SetToolTip(this.trackBarWordVolume, "Volume of speech for speak on word");
            this.trackBarWordVolume.Value = 60;
            this.trackBarWordVolume.Scroll += new System.EventHandler(this.trackBarWordVolume_Scroll);
            // 
            // trackBarWordRate
            // 
            this.trackBarWordRate.LargeChange = 1;
            this.trackBarWordRate.Location = new System.Drawing.Point(110, 23);
            this.trackBarWordRate.Minimum = -10;
            this.trackBarWordRate.Name = "trackBarWordRate";
            this.trackBarWordRate.Size = new System.Drawing.Size(104, 45);
            this.trackBarWordRate.TabIndex = 3;
            this.trackBarWordRate.TickFrequency = 5;
            this.toolTip1.SetToolTip(this.trackBarWordRate, "Rate of speech for speak on word");
            this.trackBarWordRate.Value = 3;
            this.trackBarWordRate.Scroll += new System.EventHandler(this.trackBarWordRate_Scroll);
            // 
            // checkBoxSpeakOnWord
            // 
            this.checkBoxSpeakOnWord.AutoSize = true;
            this.checkBoxSpeakOnWord.Location = new System.Drawing.Point(3, 23);
            this.checkBoxSpeakOnWord.Name = "checkBoxSpeakOnWord";
            this.checkBoxSpeakOnWord.Size = new System.Drawing.Size(101, 17);
            this.checkBoxSpeakOnWord.TabIndex = 2;
            this.checkBoxSpeakOnWord.Text = "Speak on word:";
            this.toolTip1.SetToolTip(this.checkBoxSpeakOnWord, "Speak each word as it is entered");
            this.checkBoxSpeakOnWord.UseVisualStyleBackColor = true;
            this.checkBoxSpeakOnWord.CheckedChanged += new System.EventHandler(this.checkBoxSpeakOnWord_CheckedChanged);
            // 
            // labelVolume
            // 
            this.labelVolume.Location = new System.Drawing.Point(220, 4);
            this.labelVolume.Name = "labelVolume";
            this.labelVolume.Size = new System.Drawing.Size(104, 13);
            this.labelVolume.TabIndex = 1000;
            this.labelVolume.Text = "Volume";
            this.labelVolume.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // labelRate
            // 
            this.labelRate.Location = new System.Drawing.Point(110, 4);
            this.labelRate.Name = "labelRate";
            this.labelRate.Size = new System.Drawing.Size(104, 13);
            this.labelRate.TabIndex = 1000;
            this.labelRate.Text = "Rate";
            this.labelRate.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // HeyILostMyVoiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 458);
            this.Controls.Add(this.panelControls);
            this.Controls.Add(this.richTextBox1);
            this.Name = "HeyILostMyVoiceForm";
            this.Text = "Hey, I Lost My Voice!";
            this.Layout += new System.Windows.Forms.LayoutEventHandler(this.HeyILostMyVoiceForm_Layout);
            this.panelControls.ResumeLayout(false);
            this.panelControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarParaVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarParaRate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWordVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarWordRate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.CheckBox checkBoxSpeakOnWord;
        private System.Windows.Forms.Label labelVolume;
        private System.Windows.Forms.Label labelRate;
        private System.Windows.Forms.CheckBox checkBoxSpeakOnPara;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonSkipAhead;
        private System.Windows.Forms.Button buttonPlayPause;
        private System.Windows.Forms.Button buttonSkipBack;
        private System.Windows.Forms.TrackBar trackBarWordVolume;
        private System.Windows.Forms.TrackBar trackBarWordRate;
        private System.Windows.Forms.TrackBar trackBarParaRate;
        private System.Windows.Forms.TrackBar trackBarParaVolume;
        private System.Windows.Forms.ComboBox comboBoxParaVoice;
        private System.Windows.Forms.Button buttonSettings;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

