// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Forms
{
    partial class ViewResultsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewResultsForm));
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            EditResultButton = new Button();
            DataGridViewResults = new DataGridView();
            Section = new DataGridViewTextBoxColumn();
            Table = new DataGridViewTextBoxColumn();
            Round = new DataGridViewTextBoxColumn();
            Board = new DataGridViewTextBoxColumn();
            North = new DataGridViewTextBoxColumn();
            East = new DataGridViewTextBoxColumn();
            CloseButton = new Button();
            ((System.ComponentModel.ISupportInitialize)DataGridViewResults).BeginInit();
            SuspendLayout();
            // 
            // EditResultButton
            // 
            resources.ApplyResources(EditResultButton, "EditResultButton");
            EditResultButton.Name = "EditResultButton";
            EditResultButton.UseVisualStyleBackColor = true;
            EditResultButton.Click += EditResultButton_Click;
            // 
            // DataGridViewResults
            // 
            DataGridViewResults.AllowUserToAddRows = false;
            DataGridViewResults.AllowUserToDeleteRows = false;
            DataGridViewResults.AllowUserToResizeColumns = false;
            DataGridViewResults.AllowUserToResizeRows = false;
            DataGridViewResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.ControlDark;
            dataGridViewCellStyle1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            DataGridViewResults.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            DataGridViewResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DataGridViewResults.Columns.AddRange(new DataGridViewColumn[] { Section, Table, Round, Board, North, East });
            DataGridViewResults.EditMode = DataGridViewEditMode.EditProgrammatically;
            DataGridViewResults.GridColor = SystemColors.Control;
            resources.ApplyResources(DataGridViewResults, "DataGridViewResults");
            DataGridViewResults.MultiSelect = false;
            DataGridViewResults.Name = "DataGridViewResults";
            DataGridViewResults.ReadOnly = true;
            DataGridViewResults.RowHeadersVisible = false;
            DataGridViewResults.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            DataGridViewResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            DataGridViewResults.CellContentClick += DataGridViewResults_CellContentClick;
            // 
            // Section
            // 
            resources.ApplyResources(Section, "Section");
            Section.Name = "Section";
            Section.ReadOnly = true;
            // 
            // Table
            // 
            resources.ApplyResources(Table, "Table");
            Table.Name = "Table";
            Table.ReadOnly = true;
            // 
            // Round
            // 
            resources.ApplyResources(Round, "Round");
            Round.Name = "Round";
            Round.ReadOnly = true;
            // 
            // Board
            // 
            resources.ApplyResources(Board, "Board");
            Board.Name = "Board";
            Board.ReadOnly = true;
            // 
            // North
            // 
            resources.ApplyResources(North, "North");
            North.Name = "North";
            North.ReadOnly = true;
            // 
            // East
            // 
            resources.ApplyResources(East, "East");
            East.Name = "East";
            East.ReadOnly = true;
            // 
            // CloseButton
            // 
            resources.ApplyResources(CloseButton, "CloseButton");
            CloseButton.Name = "CloseButton";
            CloseButton.UseVisualStyleBackColor = true;
            CloseButton.Click += CloseButton_Click;
            // 
            // ViewResultsForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(DataGridViewResults);
            Controls.Add(EditResultButton);
            Controls.Add(CloseButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ViewResultsForm";
            Load += ViewResultsForm_Load;
            ((System.ComponentModel.ISupportInitialize)DataGridViewResults).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button EditResultButton;
        private System.Windows.Forms.DataGridView DataGridViewResults;
        private System.Windows.Forms.Button CloseButton;
        private DataGridViewTextBoxColumn Section;
        private DataGridViewTextBoxColumn Table;
        private DataGridViewTextBoxColumn Round;
        private DataGridViewTextBoxColumn Board;
        private DataGridViewTextBoxColumn North;
        private DataGridViewTextBoxColumn East;
    }

}