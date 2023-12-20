namespace TabScore2.Forms
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            labelScoringDatabase = new Label();
            labelPathToDatabase = new Label();
            buttonAddDatabaseFile = new Button();
            labelHandRecordFile = new Label();
            labelPathToHandRecordFile = new Label();
            buttonAddHandRecordFile = new Button();
            progressBarAnalysing = new ProgressBar();
            labelAnalysing = new Label();
            buttonSettings = new Button();
            buttonResultsViewer = new Button();
            labelSessionStatus = new Label();
            databaseFileDialog = new OpenFileDialog();
            handRecordFileDialog = new OpenFileDialog();
            analysisCalculationBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            SuspendLayout();
            // 
            // labelScoringDatabase
            // 
            resources.ApplyResources(labelScoringDatabase, "labelScoringDatabase");
            labelScoringDatabase.Name = "labelScoringDatabase";
            // 
            // labelPathToDatabase
            // 
            resources.ApplyResources(labelPathToDatabase, "labelPathToDatabase");
            labelPathToDatabase.BackColor = SystemColors.ControlLightLight;
            labelPathToDatabase.BorderStyle = BorderStyle.FixedSingle;
            labelPathToDatabase.Name = "labelPathToDatabase";
            // 
            // buttonAddDatabaseFile
            // 
            resources.ApplyResources(buttonAddDatabaseFile, "buttonAddDatabaseFile");
            buttonAddDatabaseFile.Name = "buttonAddDatabaseFile";
            buttonAddDatabaseFile.UseVisualStyleBackColor = true;
            buttonAddDatabaseFile.Click += ButtonAddDatabaseFile_Click;
            // 
            // labelHandRecordFile
            // 
            resources.ApplyResources(labelHandRecordFile, "labelHandRecordFile");
            labelHandRecordFile.Name = "labelHandRecordFile";
            // 
            // labelPathToHandRecordFile
            // 
            resources.ApplyResources(labelPathToHandRecordFile, "labelPathToHandRecordFile");
            labelPathToHandRecordFile.BackColor = SystemColors.ControlLightLight;
            labelPathToHandRecordFile.BorderStyle = BorderStyle.FixedSingle;
            labelPathToHandRecordFile.Name = "labelPathToHandRecordFile";
            // 
            // buttonAddHandRecordFile
            // 
            resources.ApplyResources(buttonAddHandRecordFile, "buttonAddHandRecordFile");
            buttonAddHandRecordFile.Name = "buttonAddHandRecordFile";
            buttonAddHandRecordFile.UseVisualStyleBackColor = true;
            buttonAddHandRecordFile.Click += ButtonAddHandRecordFile_Click;
            // 
            // progressBarAnalysing
            // 
            resources.ApplyResources(progressBarAnalysing, "progressBarAnalysing");
            progressBarAnalysing.Name = "progressBarAnalysing";
            // 
            // labelAnalysing
            // 
            resources.ApplyResources(labelAnalysing, "labelAnalysing");
            labelAnalysing.Name = "labelAnalysing";
            // 
            // buttonSettings
            // 
            resources.ApplyResources(buttonSettings, "buttonSettings");
            buttonSettings.Name = "buttonSettings";
            buttonSettings.UseVisualStyleBackColor = true;
            buttonSettings.Click += ButtonSettings_Click;
            // 
            // buttonResultsViewer
            // 
            resources.ApplyResources(buttonResultsViewer, "buttonResultsViewer");
            buttonResultsViewer.Name = "buttonResultsViewer";
            buttonResultsViewer.UseVisualStyleBackColor = true;
            buttonResultsViewer.Click += ButtonResultsViewer_Click;
            // 
            // labelSessionStatus
            // 
            resources.ApplyResources(labelSessionStatus, "labelSessionStatus");
            labelSessionStatus.ForeColor = Color.Red;
            labelSessionStatus.Name = "labelSessionStatus";
            // 
            // databaseFileDialog
            // 
            resources.ApplyResources(databaseFileDialog, "databaseFileDialog");
            // 
            // handRecordFileDialog
            // 
            resources.ApplyResources(handRecordFileDialog, "handRecordFileDialog");
            // 
            // analysisCalculationBackgroundWorker
            // 
            analysisCalculationBackgroundWorker.WorkerReportsProgress = true;
            analysisCalculationBackgroundWorker.DoWork += AnalysisCalculation_DoWork;
            analysisCalculationBackgroundWorker.ProgressChanged += AnalysisCalculation_ProgressChanged;
            analysisCalculationBackgroundWorker.RunWorkerCompleted += AnalysisCalculation_RunWorkerCompleted;
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(labelSessionStatus);
            Controls.Add(buttonResultsViewer);
            Controls.Add(buttonSettings);
            Controls.Add(labelAnalysing);
            Controls.Add(progressBarAnalysing);
            Controls.Add(buttonAddHandRecordFile);
            Controls.Add(labelPathToHandRecordFile);
            Controls.Add(labelHandRecordFile);
            Controls.Add(buttonAddDatabaseFile);
            Controls.Add(labelPathToDatabase);
            Controls.Add(labelScoringDatabase);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Name = "MainForm";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelScoringDatabase;
        private Label labelPathToDatabase;
        private Button buttonAddDatabaseFile;
        private Label labelHandRecordFile;
        private Label labelPathToHandRecordFile;
        private Button buttonAddHandRecordFile;
        private ProgressBar progressBarAnalysing;
        private Label labelAnalysing;
        private Button buttonSettings;
        private Button buttonResultsViewer;
        private Label labelSessionStatus;
        private OpenFileDialog databaseFileDialog;
        private OpenFileDialog handRecordFileDialog;
        private System.ComponentModel.BackgroundWorker analysisCalculationBackgroundWorker;
    }
}