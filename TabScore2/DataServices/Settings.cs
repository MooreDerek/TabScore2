// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;

namespace TabScore2.DataServices
{
    // Settings contains an internal copy of those values from the database Settings table needed by the application.
    // They are refreshed at the start of every round in case of any changes
    // Settings that are specific to TabScore2 are instead stored as Properties, and persist from session to session

    public class Settings(IDatabase iDatabase) : ISettings
    {
        private readonly IDatabase database = iDatabase;
        private static int settingsRoundNumber = 0;

        public void DatabaseRefresh(int roundNumber)
        {
            if (roundNumber <= settingsRoundNumber) return;
            DatabaseSettings databaseSettings = database.GetDatabaseSettings();
            settingsRoundNumber++;
            ShowTraveller = databaseSettings.ShowTraveller;
            ShowPercentage = databaseSettings.ShowPercentage;
            EnterLeadCard = databaseSettings.EnterLeadCard;
            ValidateLeadCard = databaseSettings.EnterLeadCard;
            ShowRanking = databaseSettings.ShowRanking;
            ShowHandRecord = databaseSettings.ShowHandRecord;
            NumberEntryEachRound = databaseSettings.NumberEntryEachRound;
            NameSource = databaseSettings.NameSource;
            EnterResultsMethod = databaseSettings.EnterResultsMethod;
        }

        // Database settings
        public bool ShowTraveller { get; set; }
        public bool ShowPercentage { get; set; }
        public bool EnterLeadCard { get; set; }
        public bool ValidateLeadCard { get; set; }
        public int ShowRanking { get; set; }
        public int EnterResultsMethod { get; set; }
        public bool ShowHandRecord { get; set; }
        public bool NumberEntryEachRound { get; set; }
        public int NameSource { get; set; }
        public bool ManualHandRecordEntry { get; set; }

        // Application properties
        public bool TabletsMove {
            get { return Properties.Settings.Default.TabletsMove; }
            set { Properties.Settings.Default.TabletsMove = value; Properties.Settings.Default.Save(); }
        }
        public bool HandRecordReversePerspective
        {
            get { return Properties.Settings.Default.HandRecordReversePerspective; }
            set { Properties.Settings.Default.HandRecordReversePerspective = value; Properties.Settings.Default.Save(); }
        }
        public bool ShowTimer
        {
            get { return Properties.Settings.Default.ShowTimer; }
            set { Properties.Settings.Default.ShowTimer = value; Properties.Settings.Default.Save(); }
        }
        public int SecondsPerBoard
        {
            get { return Properties.Settings.Default.SecondsPerBoard; }
            set { Properties.Settings.Default.SecondsPerBoard = value; Properties.Settings.Default.Save(); }
        }
        public int AdditionalSecondsPerRound
        {
            get { return Properties.Settings.Default.AdditionalSecondsPerRound; }
            set { Properties.Settings.Default.AdditionalSecondsPerRound = value; Properties.Settings.Default.Save(); }
        }
        public bool DoubleDummy
        {
            get { return Properties.Settings.Default.DoubleDummy; }
            set { Properties.Settings.Default.DoubleDummy = value; Properties.Settings.Default.Save(); }
        }
    }
}
