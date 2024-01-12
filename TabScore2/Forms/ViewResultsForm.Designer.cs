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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.EditResultButton = new System.Windows.Forms.Button();
            this.dataGridViewResults = new System.Windows.Forms.DataGridView();
            this.Section = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Table = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Round = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Board = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PairNS = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PairEW = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CloseButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResults)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.EditResultButton, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dataGridViewResults, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.CloseButton, 1, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // EditResultButton
            // 
            resources.ApplyResources(this.EditResultButton, "EditResultButton");
            this.EditResultButton.Name = "EditResultButton";
            this.EditResultButton.UseVisualStyleBackColor = true;
            this.EditResultButton.Click += new System.EventHandler(this.EditResultButton_Click);
            // 
            // dataGridViewResults
            // 
            this.dataGridViewResults.AllowUserToAddRows = false;
            this.dataGridViewResults.AllowUserToDeleteRows = false;
            this.dataGridViewResults.AllowUserToResizeColumns = false;
            this.dataGridViewResults.AllowUserToResizeRows = false;
            this.dataGridViewResults.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlDark;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewResults.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewResults.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Section,
            this.Table,
            this.Round,
            this.Board,
            this.PairNS,
            this.PairEW});
            this.tableLayoutPanel1.SetColumnSpan(this.dataGridViewResults, 2);
            resources.ApplyResources(this.dataGridViewResults, "dataGridViewResults");
            this.dataGridViewResults.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dataGridViewResults.GridColor = System.Drawing.SystemColors.Control;
            this.dataGridViewResults.MultiSelect = false;
            this.dataGridViewResults.Name = "dataGridViewResults";
            this.dataGridViewResults.ReadOnly = true;
            this.dataGridViewResults.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dataGridViewResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewResults.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridViewResults_CellContentClick);
            // 
            // Section
            // 
            resources.ApplyResources(this.Section, "Section");
            this.Section.Name = "Section";
            this.Section.ReadOnly = true;
            // 
            // Table
            // 
            resources.ApplyResources(this.Table, "Table");
            this.Table.Name = "Table";
            this.Table.ReadOnly = true;
            // 
            // Round
            // 
            resources.ApplyResources(this.Round, "Round");
            this.Round.Name = "Round";
            this.Round.ReadOnly = true;
            // 
            // Board
            // 
            resources.ApplyResources(this.Board, "Board");
            this.Board.Name = "Board";
            this.Board.ReadOnly = true;
            // 
            // PairNS
            // 
            resources.ApplyResources(this.PairNS, "PairNS");
            this.PairNS.Name = "PairNS";
            this.PairNS.ReadOnly = true;
            // 
            // PairEW
            // 
            resources.ApplyResources(this.PairEW, "PairEW");
            this.PairEW.Name = "PairEW";
            this.PairEW.ReadOnly = true;
            // 
            // CloseButton
            // 
            resources.ApplyResources(this.CloseButton, "CloseButton");
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // ViewResultsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ViewResultsForm";
            this.Load += new System.EventHandler(this.ViewResultsForm_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewResults)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button EditResultButton;
        private System.Windows.Forms.DataGridView dataGridViewResults;
        private System.Windows.Forms.DataGridViewTextBoxColumn Section;
        private System.Windows.Forms.DataGridViewTextBoxColumn Table;
        private System.Windows.Forms.DataGridViewTextBoxColumn Round;
        private System.Windows.Forms.DataGridViewTextBoxColumn Board;
        private System.Windows.Forms.DataGridViewTextBoxColumn PairNS;
        private System.Windows.Forms.DataGridViewTextBoxColumn PairEW;
        private System.Windows.Forms.Button CloseButton;
    }

}