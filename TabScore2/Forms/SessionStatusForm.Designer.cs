namespace TabScore2.Forms
{
    partial class SessionStatusForm
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
        /// All display text is set from the Strings resources in SessionStatusForm_Load.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            tabControl = new TabControl();
            tabPageStartOfPlay = new TabPage();
            dataGridViewTables = new DataGridView();
            columnSection = new DataGridViewTextBoxColumn();
            columnTable = new DataGridViewTextBoxColumn();
            columnNorthSouth = new DataGridViewTextBoxColumn();
            columnEastWest = new DataGridViewTextBoxColumn();
            columnRegistration = new DataGridViewTextBoxColumn();
            columnNamesNorthSouth = new DataGridViewTextBoxColumn();
            columnNamesEastWest = new DataGridViewTextBoxColumn();
            labelUnexpected = new Label();
            labelSummary = new Label();
            panelBottom = new Panel();
            labelScorerPage = new Label();
            labelNamesChecked = new Label();
            buttonRefreshNames = new Button();
            buttonClose = new Button();
            timerRefresh = new System.Windows.Forms.Timer(components);
            tabControl.SuspendLayout();
            tabPageStartOfPlay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewTables).BeginInit();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageStartOfPlay);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(884, 453);
            tabControl.TabIndex = 0;
            // 
            // tabPageStartOfPlay
            // 
            tabPageStartOfPlay.Controls.Add(dataGridViewTables);
            tabPageStartOfPlay.Controls.Add(labelUnexpected);
            tabPageStartOfPlay.Controls.Add(labelSummary);
            tabPageStartOfPlay.Location = new Point(4, 26);
            tabPageStartOfPlay.Name = "tabPageStartOfPlay";
            tabPageStartOfPlay.Padding = new Padding(6);
            tabPageStartOfPlay.Size = new Size(876, 423);
            tabPageStartOfPlay.TabIndex = 0;
            tabPageStartOfPlay.UseVisualStyleBackColor = true;
            // 
            // dataGridViewTables
            // 
            dataGridViewTables.AllowUserToAddRows = false;
            dataGridViewTables.AllowUserToDeleteRows = false;
            dataGridViewTables.AllowUserToResizeRows = false;
            dataGridViewTables.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewTables.BackgroundColor = SystemColors.Window;
            headerStyle.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            headerStyle.WrapMode = DataGridViewTriState.True;
            dataGridViewTables.ColumnHeadersDefaultCellStyle = headerStyle;
            dataGridViewTables.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTables.Columns.AddRange(new DataGridViewColumn[] { columnSection, columnTable, columnNorthSouth, columnEastWest, columnRegistration, columnNamesNorthSouth, columnNamesEastWest });
            dataGridViewTables.Dock = DockStyle.Fill;
            dataGridViewTables.Location = new Point(6, 36);
            dataGridViewTables.MultiSelect = false;
            dataGridViewTables.Name = "dataGridViewTables";
            dataGridViewTables.ReadOnly = true;
            dataGridViewTables.RowHeadersVisible = false;
            dataGridViewTables.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTables.Size = new Size(864, 341);
            dataGridViewTables.TabIndex = 1;
            // 
            // columnSection
            // 
            columnSection.FillWeight = 40F;
            columnSection.Name = "columnSection";
            columnSection.ReadOnly = true;
            columnSection.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // columnTable
            // 
            columnTable.FillWeight = 40F;
            columnTable.Name = "columnTable";
            columnTable.ReadOnly = true;
            columnTable.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // columnNorthSouth
            // 
            columnNorthSouth.FillWeight = 45F;
            columnNorthSouth.Name = "columnNorthSouth";
            columnNorthSouth.ReadOnly = true;
            columnNorthSouth.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // columnEastWest
            // 
            columnEastWest.FillWeight = 45F;
            columnEastWest.Name = "columnEastWest";
            columnEastWest.ReadOnly = true;
            columnEastWest.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // columnRegistration
            // 
            columnRegistration.FillWeight = 110F;
            columnRegistration.Name = "columnRegistration";
            columnRegistration.ReadOnly = true;
            columnRegistration.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // columnNamesNorthSouth
            // 
            columnNamesNorthSouth.FillWeight = 130F;
            columnNamesNorthSouth.Name = "columnNamesNorthSouth";
            columnNamesNorthSouth.ReadOnly = true;
            columnNamesNorthSouth.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // columnNamesEastWest
            // 
            columnNamesEastWest.FillWeight = 130F;
            columnNamesEastWest.Name = "columnNamesEastWest";
            columnNamesEastWest.ReadOnly = true;
            columnNamesEastWest.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // labelUnexpected
            // 
            labelUnexpected.Dock = DockStyle.Bottom;
            labelUnexpected.ForeColor = Color.DarkRed;
            labelUnexpected.Location = new Point(6, 377);
            labelUnexpected.Name = "labelUnexpected";
            labelUnexpected.Padding = new Padding(0, 6, 0, 0);
            labelUnexpected.Size = new Size(864, 40);
            labelUnexpected.TabIndex = 2;
            labelUnexpected.Visible = false;
            // 
            // labelSummary
            // 
            labelSummary.Dock = DockStyle.Top;
            labelSummary.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            labelSummary.Location = new Point(6, 6);
            labelSummary.Name = "labelSummary";
            labelSummary.Size = new Size(864, 30);
            labelSummary.TabIndex = 0;
            labelSummary.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(labelScorerPage);
            panelBottom.Controls.Add(labelNamesChecked);
            panelBottom.Controls.Add(buttonRefreshNames);
            panelBottom.Controls.Add(buttonClose);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 453);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(884, 58);
            panelBottom.TabIndex = 1;
            // 
            // labelScorerPage
            // 
            labelScorerPage.AutoSize = true;
            labelScorerPage.Font = new Font("Segoe UI", 9F);
            labelScorerPage.Location = new Point(10, 32);
            labelScorerPage.Name = "labelScorerPage";
            labelScorerPage.Size = new Size(0, 15);
            labelScorerPage.TabIndex = 3;
            // 
            // labelNamesChecked
            // 
            labelNamesChecked.AutoSize = true;
            labelNamesChecked.Font = new Font("Segoe UI", 9F);
            labelNamesChecked.Location = new Point(10, 10);
            labelNamesChecked.Name = "labelNamesChecked";
            labelNamesChecked.Size = new Size(0, 15);
            labelNamesChecked.TabIndex = 2;
            // 
            // buttonRefreshNames
            // 
            buttonRefreshNames.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRefreshNames.Location = new Point(560, 12);
            buttonRefreshNames.Name = "buttonRefreshNames";
            buttonRefreshNames.Size = new Size(200, 34);
            buttonRefreshNames.TabIndex = 0;
            buttonRefreshNames.UseVisualStyleBackColor = true;
            buttonRefreshNames.Click += ButtonRefreshNames_Click;
            // 
            // buttonClose
            // 
            buttonClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonClose.Location = new Point(770, 12);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(100, 34);
            buttonClose.TabIndex = 1;
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += ButtonClose_Click;
            // 
            // timerRefresh
            // 
            timerRefresh.Interval = 3000;
            timerRefresh.Tick += TimerRefresh_Tick;
            // 
            // SessionStatusForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 511);
            Controls.Add(tabControl);
            Controls.Add(panelBottom);
            Font = new Font("Segoe UI", 9.75F);
            MinimumSize = new Size(700, 400);
            Name = "SessionStatusForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.Manual;
            FormClosed += SessionStatusForm_FormClosed;
            Load += SessionStatusForm_Load;
            tabControl.ResumeLayout(false);
            tabPageStartOfPlay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewTables).EndInit();
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabPageStartOfPlay;
        private DataGridView dataGridViewTables;
        private DataGridViewTextBoxColumn columnSection;
        private DataGridViewTextBoxColumn columnTable;
        private DataGridViewTextBoxColumn columnNorthSouth;
        private DataGridViewTextBoxColumn columnEastWest;
        private DataGridViewTextBoxColumn columnRegistration;
        private DataGridViewTextBoxColumn columnNamesNorthSouth;
        private DataGridViewTextBoxColumn columnNamesEastWest;
        private Label labelUnexpected;
        private Label labelSummary;
        private Panel panelBottom;
        private Label labelScorerPage;
        private Label labelNamesChecked;
        private Button buttonRefreshNames;
        private Button buttonClose;
        private System.Windows.Forms.Timer timerRefresh;
    }
}
