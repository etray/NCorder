namespace UserInterface
{
    partial class NCorder
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NCorder));
            this.stopButton = new System.Windows.Forms.Button();
            this.startButton = new System.Windows.Forms.Button();
            this.addToQueue = new System.Windows.Forms.Button();
            this.statusText = new System.Windows.Forms.TextBox();
            this.mainPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.queueGrid = new System.Windows.Forms.DataGridView();
            this.title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.url = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.playListModeCheckBox = new System.Windows.Forms.CheckBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.recordingIndicator = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.queueGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(282, 9);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(129, 37);
            this.stopButton.TabIndex = 2;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(147, 9);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(129, 37);
            this.startButton.TabIndex = 1;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // addToQueue
            // 
            this.addToQueue.Location = new System.Drawing.Point(12, 9);
            this.addToQueue.Name = "addToQueue";
            this.addToQueue.Size = new System.Drawing.Size(129, 37);
            this.addToQueue.TabIndex = 0;
            this.addToQueue.Text = "Add To Queue";
            this.addToQueue.UseVisualStyleBackColor = true;
            this.addToQueue.Click += new System.EventHandler(this.addToQueue_Click);
            // 
            // statusText
            // 
            this.statusText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.statusText.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.statusText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.statusText.Location = new System.Drawing.Point(12, 152);
            this.statusText.Name = "statusText";
            this.statusText.ReadOnly = true;
            this.statusText.Size = new System.Drawing.Size(481, 16);
            this.statusText.TabIndex = 6;
            this.statusText.TabStop = false;
            this.statusText.Text = "Idle.";
            // 
            // mainPanel
            // 
            this.mainPanel.AutoSize = true;
            this.mainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.mainPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Margin = new System.Windows.Forms.Padding(0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(0, 0);
            this.mainPanel.TabIndex = 7;
            // 
            // queueGrid
            // 
            this.queueGrid.AllowUserToOrderColumns = true;
            this.queueGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.queueGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.title,
            this.url});
            this.queueGrid.Location = new System.Drawing.Point(12, 52);
            this.queueGrid.Name = "queueGrid";
            this.queueGrid.RowTemplate.Height = 24;
            this.queueGrid.Size = new System.Drawing.Size(620, 84);
            this.queueGrid.TabIndex = 5;
            this.queueGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.queueGrid_CellEndEdit);
            this.queueGrid.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.queueGrid_RowsAdded);
            this.queueGrid.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.queueGrid_RowsRemoved);
            this.queueGrid.MouseLeave += new System.EventHandler(this.queueGrid_MouseLeave);
            // 
            // title
            // 
            this.title.HeaderText = "Title";
            this.title.Name = "title";
            this.title.Width = 270;
            // 
            // url
            // 
            this.url.HeaderText = "URL";
            this.url.Name = "url";
            this.url.Width = 290;
            // 
            // playListModeCheckBox
            // 
            this.playListModeCheckBox.AutoSize = true;
            this.playListModeCheckBox.Location = new System.Drawing.Point(506, 9);
            this.playListModeCheckBox.Name = "playListModeCheckBox";
            this.playListModeCheckBox.Size = new System.Drawing.Size(121, 21);
            this.playListModeCheckBox.TabIndex = 4;
            this.playListModeCheckBox.Text = "Playlist mode";
            this.playListModeCheckBox.UseVisualStyleBackColor = true;
            this.playListModeCheckBox.CheckedChanged += new System.EventHandler(this.playListModeCheckBox_CheckedChanged);
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(417, 9);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(76, 37);
            this.clearButton.TabIndex = 3;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
            // 
            // recordingIndicator
            // 
            this.recordingIndicator.AutoSize = true;
            this.recordingIndicator.FlatAppearance.CheckedBackColor = System.Drawing.Color.Red;
            this.recordingIndicator.Location = new System.Drawing.Point(522, 147);
            this.recordingIndicator.Name = "recordingIndicator";
            this.recordingIndicator.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.recordingIndicator.Size = new System.Drawing.Size(99, 21);
            this.recordingIndicator.TabIndex = 8;
            this.recordingIndicator.Text = "Recording";
            this.recordingIndicator.UseVisualStyleBackColor = true;
            // 
            // NCorder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(639, 171);
            this.Controls.Add(this.recordingIndicator);
            this.Controls.Add(this.statusText);
            this.Controls.Add(this.queueGrid);
            this.Controls.Add(this.playListModeCheckBox);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.addToQueue);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.mainPanel);
            this.Font = new System.Drawing.Font("Verdana", 6.985075F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "NCorder";
            this.Text = "NCorder";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Maroon;
            ((System.ComponentModel.ISupportInitialize)(this.queueGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button addToQueue;
        private System.Windows.Forms.TextBox statusText;
        private System.Windows.Forms.FlowLayoutPanel mainPanel;
        private System.Windows.Forms.DataGridView queueGrid;
        private System.Windows.Forms.CheckBox playListModeCheckBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn title;
        private System.Windows.Forms.DataGridViewTextBoxColumn url;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.RadioButton recordingIndicator;
    }
}

