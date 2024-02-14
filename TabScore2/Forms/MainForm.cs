// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using Microsoft.Extensions.Localization;
using System.Reflection;
using TabScore2.Classes;
using TabScore2.DataServices;
using TabScore2.Resources;

namespace TabScore2.Forms
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IStringLocalizer<Strings> localizer;
        private readonly IDatabase database;
        private readonly IAppData appData;

        public MainForm(IServiceProvider iServiceProvider, IStringLocalizer<Strings> iLocalizer, IDatabase iDatabase, IAppData iAppData)
        {
            serviceProvider = iServiceProvider; 
            localizer = iLocalizer;
            database = iDatabase;
            appData = iAppData;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Text = $"TabScore2 - {localizer["Version"]} {Assembly.GetExecutingAssembly().GetName().Version}";

            if (database.PathToDatabase == string.Empty)
            {
                buttonAddDatabaseFile.Visible = true;
            }
            else
            {
                IntializeDatabase();
            }
        }

        private void ButtonAddDatabaseFile_Click(object sender, EventArgs e)
        {
            if (databaseFileDialog.ShowDialog() == DialogResult.OK)
            {
                database.InitializationComplete = false;
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
                    buttonSettings.Enabled = false;
                    buttonResultsViewer.Enabled = false;
                    labelSessionStatus.Text = localizer["SessionPaused"];
                    labelSessionStatus.ForeColor = Color.OrangeRed;
                    database.InitializationComplete = false;
                    Refresh();
                    AnalyseHands();
                    buttonSettings.Enabled = true;
                    buttonResultsViewer.Enabled = true;
                    labelSessionStatus.Text = localizer["SessionRunning"];
                    labelSessionStatus.ForeColor = Color.Green;
                    database.InitializationComplete = true;
                }
            }
        }

        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            // Two-step process to inject SettingsForm with a free parameter
            Func<Point, SettingsForm> settingsFormTemplate = serviceProvider.GetRequiredService<Func<Point, SettingsForm>>();
            SettingsForm settingsForm = settingsFormTemplate(new Point(Location.X + 30, Location.Y + 30));
            settingsForm.ShowDialog();
        }

        private void ButtonResultsViewer_Click(object sender, EventArgs e)
        {
            // Two-step process to inject ViewResultsForm with a free parameter
            Func<Point, ViewResultsForm> viewResultsFormTemplate = serviceProvider.GetRequiredService<Func<Point, ViewResultsForm>>();
            ViewResultsForm viewResultsForm = viewResultsFormTemplate(new Point(Location.X + 30, Location.Y + 30));
            viewResultsForm.ShowDialog();
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show(localizer["ClosingMessage"], "TabScore2", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.No)
            {
                e.Cancel = true;
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
                buttonSettings.Enabled = false;
                buttonResultsViewer.Enabled = false;
                buttonSettings.Visible = true;
                buttonResultsViewer.Visible = true;
                buttonAddDatabaseFile.Visible = false;

                // Analyse any hand records in the database
                if (database.HandsCount > 0)
                {
                    labelPathToHandRecordFile.Text = localizer["IncludedInDatabase"];
                    AnalyseHands();
                }

                buttonAddHandRecordFile.Visible = true;
                buttonSettings.Enabled = true;
                buttonResultsViewer.Enabled = true;
                labelSessionStatus.Text = localizer["SessionRunning"];
                labelSessionStatus.ForeColor = Color.Green;
                database.InitializationComplete = true;
            }
        }

        private void AnalyseHands()
        {
            buttonAddHandRecordFile.Enabled = false;
            labelAnalysing.Text = localizer["Analysing"];
            labelAnalysing.Visible = true;
            progressBarAnalysing.Visible = true;
            Refresh();
            double counter = 0.0;
            appData.ClearHandEvaluations();
            foreach (Hand hand in database.HandsList)
            {
                appData.AddHandEvaluation(hand);
                counter++;
                progressBarAnalysing.Value = Convert.ToInt32(counter / database.HandsCount * 100.0);
            }
            progressBarAnalysing.Value = 100;
            labelAnalysing.Text = localizer["AnalysisComplete"];
            buttonAddHandRecordFile.Text = localizer["ChangeHandRecordFile"];
            buttonAddHandRecordFile.Enabled = true;
        }
    }
}

