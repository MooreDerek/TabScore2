// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Forms
{
    partial class EditResultForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditResultForm));
            labelSection = new Label();
            labelTable = new Label();
            labelRound = new Label();
            labelBoard = new Label();
            labelNorth = new Label();
            labelEast = new Label();
            comboBoxContractLevel = new ComboBox();
            labelSuit = new Label();
            comboBoxSuit = new ComboBox();
            labelDouble = new Label();
            comboBoxDouble = new ComboBox();
            comboBoxDeclarer = new ComboBox();
            labelDeclarer = new Label();
            comboBoxLead = new ComboBox();
            labelLead = new Label();
            comboBoxTricksTaken = new ComboBox();
            labelResult = new Label();
            comboBoxRemarks = new ComboBox();
            labelRemarks = new Label();
            buttonSave = new Button();
            buttonCancel = new Button();
            boxSection = new Label();
            boxTable = new Label();
            boxRound = new Label();
            boxBoard = new Label();
            boxNorth = new Label();
            boxEast = new Label();
            labelLevel = new Label();
            SuspendLayout();
            // 
            // labelSection
            // 
            resources.ApplyResources(labelSection, "labelSection");
            labelSection.Name = "labelSection";
            // 
            // labelTable
            // 
            resources.ApplyResources(labelTable, "labelTable");
            labelTable.Name = "labelTable";
            // 
            // labelRound
            // 
            resources.ApplyResources(labelRound, "labelRound");
            labelRound.Name = "labelRound";
            // 
            // labelBoard
            // 
            resources.ApplyResources(labelBoard, "labelBoard");
            labelBoard.Name = "labelBoard";
            // 
            // labelNorth
            // 
            resources.ApplyResources(labelNorth, "labelNorth");
            labelNorth.Name = "labelNorth";
            // 
            // labelEast
            // 
            resources.ApplyResources(labelEast, "labelEast");
            labelEast.Name = "labelEast";
            // 
            // comboBoxContractLevel
            // 
            comboBoxContractLevel.FormattingEnabled = true;
            comboBoxContractLevel.Items.AddRange(new object[] { resources.GetString("comboBoxContractLevel.Items"), resources.GetString("comboBoxContractLevel.Items1"), resources.GetString("comboBoxContractLevel.Items2"), resources.GetString("comboBoxContractLevel.Items3"), resources.GetString("comboBoxContractLevel.Items4"), resources.GetString("comboBoxContractLevel.Items5"), resources.GetString("comboBoxContractLevel.Items6"), resources.GetString("comboBoxContractLevel.Items7") });
            resources.ApplyResources(comboBoxContractLevel, "comboBoxContractLevel");
            comboBoxContractLevel.Name = "comboBoxContractLevel";
            comboBoxContractLevel.SelectedIndexChanged += ComboBoxContractLevel_SelectedIndexChanged;
            // 
            // labelSuit
            // 
            resources.ApplyResources(labelSuit, "labelSuit");
            labelSuit.Name = "labelSuit";
            // 
            // comboBoxSuit
            // 
            comboBoxSuit.FormattingEnabled = true;
            comboBoxSuit.Items.AddRange(new object[] { resources.GetString("comboBoxSuit.Items"), resources.GetString("comboBoxSuit.Items1"), resources.GetString("comboBoxSuit.Items2"), resources.GetString("comboBoxSuit.Items3"), resources.GetString("comboBoxSuit.Items4") });
            resources.ApplyResources(comboBoxSuit, "comboBoxSuit");
            comboBoxSuit.Name = "comboBoxSuit";
            // 
            // labelDouble
            // 
            resources.ApplyResources(labelDouble, "labelDouble");
            labelDouble.Name = "labelDouble";
            // 
            // comboBoxDouble
            // 
            comboBoxDouble.FormattingEnabled = true;
            comboBoxDouble.Items.AddRange(new object[] { resources.GetString("comboBoxDouble.Items"), resources.GetString("comboBoxDouble.Items1"), resources.GetString("comboBoxDouble.Items2") });
            resources.ApplyResources(comboBoxDouble, "comboBoxDouble");
            comboBoxDouble.Name = "comboBoxDouble";
            // 
            // comboBoxDeclarer
            // 
            comboBoxDeclarer.FormattingEnabled = true;
            comboBoxDeclarer.Items.AddRange(new object[] { resources.GetString("comboBoxDeclarer.Items"), resources.GetString("comboBoxDeclarer.Items1"), resources.GetString("comboBoxDeclarer.Items2"), resources.GetString("comboBoxDeclarer.Items3") });
            resources.ApplyResources(comboBoxDeclarer, "comboBoxDeclarer");
            comboBoxDeclarer.Name = "comboBoxDeclarer";
            // 
            // labelDeclarer
            // 
            resources.ApplyResources(labelDeclarer, "labelDeclarer");
            labelDeclarer.Name = "labelDeclarer";
            // 
            // comboBoxLead
            // 
            comboBoxLead.FormattingEnabled = true;
            comboBoxLead.Items.AddRange(new object[] { resources.GetString("comboBoxLead.Items"), resources.GetString("comboBoxLead.Items1"), resources.GetString("comboBoxLead.Items2"), resources.GetString("comboBoxLead.Items3"), resources.GetString("comboBoxLead.Items4"), resources.GetString("comboBoxLead.Items5"), resources.GetString("comboBoxLead.Items6"), resources.GetString("comboBoxLead.Items7"), resources.GetString("comboBoxLead.Items8"), resources.GetString("comboBoxLead.Items9"), resources.GetString("comboBoxLead.Items10"), resources.GetString("comboBoxLead.Items11"), resources.GetString("comboBoxLead.Items12"), resources.GetString("comboBoxLead.Items13"), resources.GetString("comboBoxLead.Items14"), resources.GetString("comboBoxLead.Items15"), resources.GetString("comboBoxLead.Items16"), resources.GetString("comboBoxLead.Items17"), resources.GetString("comboBoxLead.Items18"), resources.GetString("comboBoxLead.Items19"), resources.GetString("comboBoxLead.Items20"), resources.GetString("comboBoxLead.Items21"), resources.GetString("comboBoxLead.Items22"), resources.GetString("comboBoxLead.Items23"), resources.GetString("comboBoxLead.Items24"), resources.GetString("comboBoxLead.Items25"), resources.GetString("comboBoxLead.Items26"), resources.GetString("comboBoxLead.Items27"), resources.GetString("comboBoxLead.Items28"), resources.GetString("comboBoxLead.Items29"), resources.GetString("comboBoxLead.Items30"), resources.GetString("comboBoxLead.Items31"), resources.GetString("comboBoxLead.Items32"), resources.GetString("comboBoxLead.Items33"), resources.GetString("comboBoxLead.Items34"), resources.GetString("comboBoxLead.Items35"), resources.GetString("comboBoxLead.Items36"), resources.GetString("comboBoxLead.Items37"), resources.GetString("comboBoxLead.Items38"), resources.GetString("comboBoxLead.Items39"), resources.GetString("comboBoxLead.Items40"), resources.GetString("comboBoxLead.Items41"), resources.GetString("comboBoxLead.Items42"), resources.GetString("comboBoxLead.Items43"), resources.GetString("comboBoxLead.Items44"), resources.GetString("comboBoxLead.Items45"), resources.GetString("comboBoxLead.Items46"), resources.GetString("comboBoxLead.Items47"), resources.GetString("comboBoxLead.Items48"), resources.GetString("comboBoxLead.Items49"), resources.GetString("comboBoxLead.Items50"), resources.GetString("comboBoxLead.Items51"), resources.GetString("comboBoxLead.Items52") });
            resources.ApplyResources(comboBoxLead, "comboBoxLead");
            comboBoxLead.Name = "comboBoxLead";
            // 
            // labelLead
            // 
            resources.ApplyResources(labelLead, "labelLead");
            labelLead.Name = "labelLead";
            // 
            // comboBoxTricksTaken
            // 
            comboBoxTricksTaken.FormattingEnabled = true;
            comboBoxTricksTaken.Items.AddRange(new object[] { resources.GetString("comboBoxTricksTaken.Items") });
            resources.ApplyResources(comboBoxTricksTaken, "comboBoxTricksTaken");
            comboBoxTricksTaken.Name = "comboBoxTricksTaken";
            // 
            // labelResult
            // 
            resources.ApplyResources(labelResult, "labelResult");
            labelResult.Name = "labelResult";
            // 
            // comboBoxRemarks
            // 
            comboBoxRemarks.FormattingEnabled = true;
            comboBoxRemarks.Items.AddRange(new object[] { resources.GetString("comboBoxRemarks.Items"), resources.GetString("comboBoxRemarks.Items1"), resources.GetString("comboBoxRemarks.Items2"), resources.GetString("comboBoxRemarks.Items3"), resources.GetString("comboBoxRemarks.Items4"), resources.GetString("comboBoxRemarks.Items5"), resources.GetString("comboBoxRemarks.Items6"), resources.GetString("comboBoxRemarks.Items7"), resources.GetString("comboBoxRemarks.Items8"), resources.GetString("comboBoxRemarks.Items9"), resources.GetString("comboBoxRemarks.Items10"), resources.GetString("comboBoxRemarks.Items11"), resources.GetString("comboBoxRemarks.Items12") });
            resources.ApplyResources(comboBoxRemarks, "comboBoxRemarks");
            comboBoxRemarks.Name = "comboBoxRemarks";
            comboBoxRemarks.SelectedIndexChanged += ComboBoxRemarks_SelectedIndexChanged;
            // 
            // labelRemarks
            // 
            resources.ApplyResources(labelRemarks, "labelRemarks");
            labelRemarks.Name = "labelRemarks";
            // 
            // buttonSave
            // 
            resources.ApplyResources(buttonSave, "buttonSave");
            buttonSave.Name = "buttonSave";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += SaveButton_Click;
            // 
            // buttonCancel
            // 
            resources.ApplyResources(buttonCancel, "buttonCancel");
            buttonCancel.Name = "buttonCancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += CanxButton_Click;
            // 
            // boxSection
            // 
            boxSection.BackColor = SystemColors.ControlLightLight;
            boxSection.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(boxSection, "boxSection");
            boxSection.Name = "boxSection";
            // 
            // boxTable
            // 
            boxTable.BackColor = SystemColors.ControlLightLight;
            boxTable.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(boxTable, "boxTable");
            boxTable.Name = "boxTable";
            // 
            // boxRound
            // 
            boxRound.BackColor = SystemColors.ControlLightLight;
            boxRound.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(boxRound, "boxRound");
            boxRound.Name = "boxRound";
            // 
            // boxBoard
            // 
            boxBoard.BackColor = SystemColors.ControlLightLight;
            boxBoard.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(boxBoard, "boxBoard");
            boxBoard.Name = "boxBoard";
            // 
            // boxNorth
            // 
            boxNorth.BackColor = SystemColors.ControlLightLight;
            boxNorth.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(boxNorth, "boxNorth");
            boxNorth.Name = "boxNorth";
            // 
            // boxEast
            // 
            boxEast.BackColor = SystemColors.ControlLightLight;
            boxEast.BorderStyle = BorderStyle.FixedSingle;
            resources.ApplyResources(boxEast, "boxEast");
            boxEast.Name = "boxEast";
            // 
            // labelLevel
            // 
            resources.ApplyResources(labelLevel, "labelLevel");
            labelLevel.Name = "labelLevel";
            // 
            // EditResultForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(labelLevel);
            Controls.Add(boxEast);
            Controls.Add(boxNorth);
            Controls.Add(boxBoard);
            Controls.Add(boxRound);
            Controls.Add(boxTable);
            Controls.Add(boxSection);
            Controls.Add(buttonSave);
            Controls.Add(buttonCancel);
            Controls.Add(labelRemarks);
            Controls.Add(comboBoxRemarks);
            Controls.Add(labelResult);
            Controls.Add(comboBoxTricksTaken);
            Controls.Add(labelLead);
            Controls.Add(comboBoxLead);
            Controls.Add(labelDeclarer);
            Controls.Add(comboBoxDeclarer);
            Controls.Add(comboBoxDouble);
            Controls.Add(labelDouble);
            Controls.Add(comboBoxSuit);
            Controls.Add(labelSuit);
            Controls.Add(comboBoxContractLevel);
            Controls.Add(labelEast);
            Controls.Add(labelNorth);
            Controls.Add(labelBoard);
            Controls.Add(labelRound);
            Controls.Add(labelTable);
            Controls.Add(labelSection);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditResultForm";
            Load += EditResultForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label labelSection;
        private System.Windows.Forms.Label labelTable;
        private System.Windows.Forms.Label labelRound;
        private System.Windows.Forms.Label labelBoard;
        private System.Windows.Forms.Label labelNorth;
        private System.Windows.Forms.Label labelEast;
        private System.Windows.Forms.ComboBox comboBoxContractLevel;
        private System.Windows.Forms.Label labelSuit;
        private System.Windows.Forms.ComboBox comboBoxSuit;
        private System.Windows.Forms.Label labelDouble;
        private System.Windows.Forms.ComboBox comboBoxDouble;
        private System.Windows.Forms.ComboBox comboBoxDeclarer;
        private System.Windows.Forms.Label labelDeclarer;
        private System.Windows.Forms.ComboBox comboBoxLead;
        private System.Windows.Forms.Label labelLead;
        private System.Windows.Forms.ComboBox comboBoxTricksTaken;
        private System.Windows.Forms.Label labelResult;
        private System.Windows.Forms.ComboBox comboBoxRemarks;
        private System.Windows.Forms.Label labelRemarks;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label boxSection;
        private System.Windows.Forms.Label boxTable;
        private System.Windows.Forms.Label boxRound;
        private System.Windows.Forms.Label boxBoard;
        private System.Windows.Forms.Label boxNorth;
        private System.Windows.Forms.Label boxEast;
        private System.Windows.Forms.Label labelLevel;
    }
}