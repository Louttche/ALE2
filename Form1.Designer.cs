namespace ALE2
{
    partial class Form1
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
            this.btn_browse = new System.Windows.Forms.Button();
            this.lbl_fileloaded = new System.Windows.Forms.Label();
            this.rtb_filecontents = new System.Windows.Forms.RichTextBox();
            this.lbl_status = new System.Windows.Forms.Label();
            this.panel_graph.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_graph)).BeginInit();
            this.SuspendLayout();
            // 
            // panel_graph
            // 
            this.panel_graph.AutoScroll = true;
            this.panel_graph.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_graph.Controls.Add(this.pb_graph);
            this.panel_graph.Location = new System.Drawing.Point(12, 44);
            this.panel_graph.Name = "panel_graph";
            this.panel_graph.Size = new System.Drawing.Size(444, 404);
            this.panel_graph.TabIndex = 2;
            // 
            // pb_graph
            // 
            this.pb_graph.Location = new System.Drawing.Point(3, 3);
            this.pb_graph.Name = "pb_graph";
            this.pb_graph.Size = new System.Drawing.Size(436, 397);
            this.pb_graph.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pb_graph.TabIndex = 2;
            this.pb_graph.TabStop = false;
            // 
            // btn_browse
            // 
            this.btn_browse.Location = new System.Drawing.Point(462, 12);
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.Size = new System.Drawing.Size(76, 26);
            this.btn_browse.TabIndex = 3;
            this.btn_browse.Text = "Open File";
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.btn_browse_Click);
            // 
            // lbl_fileloaded
            // 
            this.lbl_fileloaded.AutoSize = true;
            this.lbl_fileloaded.Location = new System.Drawing.Point(544, 18);
            this.lbl_fileloaded.Name = "lbl_fileloaded";
            this.lbl_fileloaded.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lbl_fileloaded.Size = new System.Drawing.Size(38, 15);
            this.lbl_fileloaded.TabIndex = 4;
            this.lbl_fileloaded.Text = "label1";
            // 
            // rtb_filecontents
            // 
            this.rtb_filecontents.Location = new System.Drawing.Point(462, 44);
            this.rtb_filecontents.Name = "rtb_filecontents";
            this.rtb_filecontents.Size = new System.Drawing.Size(195, 404);
            this.rtb_filecontents.TabIndex = 5;
            this.rtb_filecontents.Text = "";
            // 
            // lbl_status
            // 
            this.lbl_status.AutoSize = true;
            this.lbl_status.Location = new System.Drawing.Point(13, 12);
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(38, 15);
            this.lbl_status.TabIndex = 6;
            this.lbl_status.Text = "label1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 458);
            this.Controls.Add(this.lbl_status);
            this.Controls.Add(this.rtb_filecontents);
            this.Controls.Add(this.lbl_fileloaded);
            this.Controls.Add(this.btn_browse);
            this.Controls.Add(this.panel_graph);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel_graph.ResumeLayout(false);
            this.panel_graph.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_graph)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel_graph;
        private System.Windows.Forms.PictureBox pb_graph;
        private System.Windows.Forms.Button btn_browse;
        private System.Windows.Forms.Label lbl_fileloaded;
        private System.Windows.Forms.RichTextBox rtb_filecontents;
        private System.Windows.Forms.Label lbl_status;
    }
}

