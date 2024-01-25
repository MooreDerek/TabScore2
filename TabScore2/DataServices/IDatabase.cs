// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;
using TabScore2.Globals;

namespace TabScore2.DataServices
{
    // IDatabase is the interface for a service that allows access to data that is normally stored in the scoring database
    public interface IDatabase
    {
        // GENERAL
        string PathToDatabase { get; set; }
        bool InitializationComplete { get; set; }
        bool IsIndividual { get; }
        string? Initialize();
        public bool IsDatabaseConnectionOK();

        // SECTION
        Section GetSection(int sectionID);
        List<Section> GetSectionsList();

        // TABLE
        void RegisterTable(int sectionID, int tableNumber);

        // ROUND
        int GetNumberOfRoundsInEvent(int sectionID, int roundNumber = 999);
        int GetNumberOfLastRoundWithResults(int sectionID, int tableNumber);
        public List<Round> GetRoundsList(int sectionID, int roundNumber);
        void GetRoundData(TableStatus tableStatus);

        // RESULT = RECEIVEDDATA
        Result GetResult(int sectionID, int tableNumber, int roundNumber, int boardNumber);
        void SetResult(Result result);
        List<Result> GetResultsList(int sectionID = 0, int lowBoard = 0, int highBoard = 0, int tableNumber = 0, int roundNumber = 0);

        // PLAYERNAMES
        string GetInternalPlayerName(string playerID);

        // PLAYERNUMBERS
        void UpdatePlayer(TableStatus tableStatus, Direction direction, string playerID, string playerName);
        public void GetNamesForRound(TableStatus tableStatus);

        // HANDRECORD
        int HandsCount { get; }
        List<Hand> HandsList { get; }
        Hand? GetHand(int sectionID, int boardNumber);
        public void AddHand(Hand hand);
        void GetHandsFromFile(StreamReader file);

        // SETTINGS
        DatabaseSettings GetDatabaseSettings();
        void UpdateDatabaseSettings(DatabaseSettings databaseSettings);

        // RANKINGLIST
        List<Ranking> GetRankingList(int sectionID);
    }
}
