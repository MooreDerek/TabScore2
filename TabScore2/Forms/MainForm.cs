// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.Extensions.Localization;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Resources;

namespace TabScore2.Forms
{
    public partial class MainForm : Form
    {
        private readonly IStringLocalizer<Strings> localizer;
        private readonly IDatabase database;
        private readonly IAppData appData;
        private readonly ISettings settings;

        public MainForm(IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, IAppData iAppData, ISettings iSettings)
        {
            localizer = iLocalizer;
            database = iDatabase;
            appData = iAppData;
            settings = iSettings;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Text = $"TabScore2 - {localizer["Version"]} {Assembly.GetExecutingAssembly().GetName().Version}";

            if (database.PathToDatabase == "")
            {
                buttonAddDatabaseFile.Visible = true;
            }
            else
            {
                IntializeDatabase();
            }
        }

        private void IntializeDatabase()
        {
            string? databaseError = database.Initialize();
            if (databaseError != null)
            {
                // Database is not valid for some reason and Initialize failed
                MessageBox.Show(localizer[databaseError], "TabScore2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                buttonAddDatabaseFile.Visible = true;
            }
            else
            {
                labelPathToDatabase.Text = database.PathToDatabase;
                labelSessionStatus.Text = localizer["SessionRunning"];
                labelSessionStatus.ForeColor = Color.Green;
                buttonSettings.Visible = true;
                buttonResultsViewer.Visible = true;
                buttonAddHandRecordFile.Visible = true;
                buttonAddDatabaseFile.Visible = false;
                if (database.HandsCount > 0)
                {
                    buttonAddHandRecordFile.Enabled = false;
                    labelPathToHandRecordFile.Text = localizer["IncludedInDatabase"];
                    labelAnalysing.Text = localizer["Analysing"];
                    labelAnalysing.Visible = true;
                    progressBarAnalysing.Visible = true;
                    analysisCalculationBackgroundWorker.RunWorkerAsync();
                }

                // Set the number of tablet devices per table - possibly different for each section depending on the movements
                foreach (Section section in database.GetSectionsList())
                {
                    // Default TabletDevicesPerTable = 1
                    if (settings.TabletsMove)
                    {
                        if (database.IsIndividual)
                        {
                            section.TabletDevicesPerTable = 4;
                        }
                        else
                        {
                            if (section.Winners == 1) section.TabletDevicesPerTable = 2;
                        }
                    }
                }
            }
        }

        private void AnalysisCalculation_DoWork(object sender, DoWorkEventArgs e)
        {
            if (sender is not BackgroundWorker worker) return;
            int counter = 0;
            appData.ClearHandEvaluations();
            foreach (Hand hand in database.HandsList)
            {
                appData.AddHandEvaluation(hand);
                counter++;
                worker.ReportProgress((int)((float)counter / (float)database.HandsCount * 100.0));
            }
        }

        private void AnalysisCalculation_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBarAnalysing.Value = e.ProgressPercentage;
        }

        private void AnalysisCalculation_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBarAnalysing.Value = 100;
            labelAnalysing.Text = localizer["AnalysisComplete"];
            buttonAddHandRecordFile.Text = localizer["ChangeHandRecordFile"];
            buttonAddHandRecordFile.Enabled = true;
        }

        private void ButtonAddDatabaseFile_Click(object sender, EventArgs e)
        {
            if (databaseFileDialog.ShowDialog() == DialogResult.OK)
            {
                database.PathToDatabase = databaseFileDialog.FileName;
                IntializeDatabase();
            }
        }

        private void ButtonAddHandRecordFile_Click(object sender, EventArgs e)
        {
            if (handRecordFileDialog.ShowDialog() == DialogResult.OK)
            {
                labelPathToHandRecordFile.Text = handRecordFileDialog.FileName;
                database.GetHandsFromFile(new StreamReader(handRecordFileDialog.FileName));
                if (database.HandsCount == 0)
                {
                    MessageBox.Show(localizer["FileNoHandRecords"], "TabScoreStarter", MessageBoxButtons.OK);
                }
                else
                {
                    buttonAddHandRecordFile.Enabled = false;
                    labelAnalysing.Text = localizer["Analysing"];
                    labelAnalysing.Visible = true;
                    progressBarAnalysing.Visible = true;
                    analysisCalculationBackgroundWorker.RunWorkerAsync();
                }
            }
        }

        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            //            SettingsForm frmOptions = new SettingsForm(connectionString, new Point(Location.X + 30, Location.Y + 30));
            //            frmOptions.ShowDialog();
        }

        private void ButtonResultsViewer_Click(object sender, EventArgs e)
        {
            //            ViewResultsForm frmResultsViewer = new ViewResultsForm(connectionString, new Point(Location.X + 30, Location.Y + 30));
            //            frmResultsViewer.ShowDialog();
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(localizer["ClosingMessage"], "TabScore2", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}

