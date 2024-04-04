// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.DataServices
{
    // Settings contains an internal copy of those values from the database Settings table needed by the application.
    // They are refreshed once at the start of every round in case of any changes
    // Settings that are specific to TabScore2 are instead stored as Properties, and persist from session to session

    public class Settings : ISettings
    {
        // Application settings stored in the scoring database
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

        // Application property settings
        public bool TabletsMove {
            get { return Properties.Settings.Default.TabletsMove; }
            set { Properties.Settings.Default.TabletsMove = value; Properties.Settings.Default.Save(); }
        }
        public string ShowHandRecordFromDirection
        {
            get { return Properties.Settings.Default.ShowHandRecordFromDirection; }
            set { Properties.Settings.Default.ShowHandRecordFromDirection = value; Properties.Settings.Default.Save(); }
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
        public int SuppressRankingListForFirstXRounds
        {
            get { return Properties.Settings.Default.SuppressRankingListForFirstXRounds; }
            set { Properties.Settings.Default.SuppressRankingListForFirstXRounds = value; Properties.Settings.Default.Save(); }
        }
        public int SuppressRankingListForLastXRounds
        {
            get { return Properties.Settings.Default.SuppressRankingListForLastXRounds; }
            set { Properties.Settings.Default.SuppressRankingListForLastXRounds = value; Properties.Settings.Default.Save(); }
        }

        // Settings related to the operation of the scoring database
        public string PathToDatabase
        {
            get { return Properties.Settings.Default.PathToDatabase; }
            set { Properties.Settings.Default.PathToDatabase = value; Properties.Settings.Default.Save(); }
        }
        public bool DatabaseReady
        {
            get { return Properties.Settings.Default.DatabaseReady; }
            set { Properties.Settings.Default.DatabaseReady = value; Properties.Settings.Default.Save(); }
        }
        public bool IsIndividual
        {
            get { return Properties.Settings.Default.IsIndividual; }
            set { Properties.Settings.Default.IsIndividual = value; Properties.Settings.Default.Save(); }
        }
    }
}
