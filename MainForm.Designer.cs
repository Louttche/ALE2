namespace ALE2
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel_graph = new System.Windows.Forms.Panel();
            this.pb_graph = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rtb_words = new System.Windows.Forms.RichTextBox();
            this.pb_wordinput = new System.Windows.Forms.PictureBox();
            this.tb_wordinput = new System.Windows.Forms.TextBox();
            this.lbl_words = new System.Windows.Forms.Label();
            this.pb_finite = new System.Windows.Forms.PictureBox();
            this.pb_dfa = new System.Windows.Forms.PictureBox();
            this.lbl_finite = new System.Windows.Forms.Label();
            this.lbl_dfa = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rtb_filecontents = new System.Windows.Forms.RichTextBox();
            this.lbl_fileloaded = new System.Windows.Forms.Label();
            this.btn_browse = new System.Windows.Forms.Button();
            this.panel_graph.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_graph)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_wordinput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_finite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_dfa)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_graph
            // 
            this.panel_graph.AutoScroll = true;
            this.panel_graph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_graph.Controls.Add(this.pb_graph);
            this.panel_graph.Location = new System.Drawing.Point(180, 12);
            this.panel_graph.Name = "panel_graph";
            this.panel_graph.Size = new System.Drawing.Size(553, 401);
            this.panel_graph.TabIndex = 2;
            // 
            // pb_graph
            // 
            this.pb_graph.Location = new System.Drawing.Point(5, 4);
            this.pb_graph.Name = "pb_graph";
            this.pb_graph.Size = new System.Drawing.Size(542, 391);
            this.pb_graph.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pb_graph.TabIndex = 2;
            this.pb_graph.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rtb_words);
            this.panel1.Controls.Add(this.pb_wordinput);
            this.panel1.Controls.Add(this.tb_wordinput);
            this.panel1.Controls.Add(this.lbl_words);
            this.panel1.Controls.Add(this.pb_finite);
            this.panel1.Controls.Add(this.pb_dfa);
            this.panel1.Controls.Add(this.lbl_finite);
            this.panel1.Controls.Add(this.lbl_dfa);
            this.panel1.Location = new System.Drawing.Point(13, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(161, 401);
            this.panel1.TabIndex = 8;
            // 
            // rtb_words
            // 
            this.rtb_words.Location = new System.Drawing.Point(13, 34);
            this.rtb_words.Name = "rtb_words";
            this.rtb_words.Size = new System.Drawing.Size(138, 260);
            this.rtb_words.TabIndex = 16;
            this.rtb_words.Text = "";
            // 
            // pb_wordinput
            // 
            this.pb_wordinput.Image = global::ALE2.Properties.Resources.tick;
            this.pb_wordinput.Location = new System.Drawing.Point(117, 296);
            this.pb_wordinput.Name = "pb_wordinput";
            this.pb_wordinput.Size = new System.Drawing.Size(34, 30);
            this.pb_wordinput.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pb_wordinput.TabIndex = 15;
            this.pb_wordinput.TabStop = false;
            // 
            // tb_wordinput
            // 
            this.tb_wordinput.Location = new System.Drawing.Point(13, 300);
            this.tb_wordinput.Name = "tb_wordinput";
            this.tb_wordinput.Size = new System.Drawing.Size(100, 23);
            this.tb_wordinput.TabIndex = 14;
            // 
            // lbl_words
            // 
            this.lbl_words.AutoSize = true;
            this.lbl_words.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbl_words.Location = new System.Drawing.Point(46, 6);
            this.lbl_words.Name = "lbl_words";
            this.lbl_words.Size = new System.Drawing.Size(67, 25);
            this.lbl_words.TabIndex = 9;
            this.lbl_words.Text = "Words";
            // 
            // pb_finite
            // 
            this.pb_finite.Image = global::ALE2.Properties.Resources.tick;
            this.pb_finite.Location = new System.Drawing.Point(92, 360);
            this.pb_finite.Name = "pb_finite";
            this.pb_finite.Size = new System.Drawing.Size(40, 36);
            this.pb_finite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pb_finite.TabIndex = 13;
            this.pb_finite.TabStop = false;
            // 
            // pb_dfa
            // 
            this.pb_dfa.Image = global::ALE2.Properties.Resources.tick;
            this.pb_dfa.Location = new System.Drawing.Point(27, 360);
            this.pb_dfa.Name = "pb_dfa";
            this.pb_dfa.Size = new System.Drawing.Size(40, 36);
            this.pb_dfa.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pb_dfa.TabIndex = 12;
            this.pb_dfa.TabStop = false;
            // 
            // lbl_finite
            // 
            this.lbl_finite.AutoSize = true;
            this.lbl_finite.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbl_finite.Location = new System.Drawing.Point(87, 336);
            this.lbl_finite.Name = "lbl_finite";
            this.lbl_finite.Size = new System.Drawing.Size(53, 21);
            this.lbl_finite.TabIndex = 11;
            this.lbl_finite.Text = "Finite";
            this.lbl_finite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_dfa
            // 
            this.lbl_dfa.AutoSize = true;
            this.lbl_dfa.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbl_dfa.Location = new System.Drawing.Point(27, 336);
            this.lbl_dfa.Name = "lbl_dfa";
            this.lbl_dfa.Size = new System.Drawing.Size(40, 21);
            this.lbl_dfa.TabIndex = 10;
            this.lbl_dfa.Text = "DFA";
            this.lbl_dfa.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rtb_filecontents);
            this.panel2.Controls.Add(this.lbl_fileloaded);
            this.panel2.Controls.Add(this.btn_browse);
            this.panel2.Location = new System.Drawing.Point(740, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(226, 401);
            this.panel2.TabIndex = 9;
            // 
            // rtb_filecontents
            // 
            this.rtb_filecontents.Location = new System.Drawing.Point(3, 34);
            this.rtb_filecontents.Name = "rtb_filecontents";
            this.rtb_filecontents.Size = new System.Drawing.Size(220, 362);
            this.rtb_filecontents.TabIndex = 8;
            this.rtb_filecontents.Text = "";
            // 
            // lbl_fileloaded
            // 
            this.lbl_fileloaded.AutoSize = true;
            this.lbl_fileloaded.Location = new System.Drawing.Point(85, 9);
            this.lbl_fileloaded.Name = "lbl_fileloaded";
            this.lbl_fileloaded.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lbl_fileloaded.Size = new System.Drawing.Size(38, 15);
            this.lbl_fileloaded.TabIndex = 7;
            this.lbl_fileloaded.Text = "label1";
            // 
            // btn_browse
            // 
            this.btn_browse.Location = new System.Drawing.Point(3, 3);
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.Size = new System.Drawing.Size(76, 26);
            this.btn_browse.TabIndex = 6;
            this.btn_browse.Text = "Open File";
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.btn_browse_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(978, 421);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel_graph);
            this.Name = "Form1";
            this.Text = "State Machine Graph";
            this.panel_graph.ResumeLayout(false);
            this.panel_graph.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_graph)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_wordinput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_finite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_dfa)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel_graph;
        private System.Windows.Forms.PictureBox pb_graph;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pb_finite;
        private System.Windows.Forms.PictureBox pb_dfa;
        private System.Windows.Forms.Label lbl_finite;
        private System.Windows.Forms.Label lbl_dfa;
        private System.Windows.Forms.Label lbl_words;
        private System.Windows.Forms.PictureBox pb_wordinput;
        private System.Windows.Forms.TextBox tb_wordinput;
        private System.Windows.Forms.RichTextBox rtb_words;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RichTextBox rtb_filecontents;
        private System.Windows.Forms.Label lbl_fileloaded;
        private System.Windows.Forms.Button btn_browse;
    }
}

