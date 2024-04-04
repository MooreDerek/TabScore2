// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using SharedContracts;
using System.Reflection;
using TabScore2.Classes;

namespace TabScore2.DataServices
{
    public class Database(IBwsDatabaseService iClient, ISettings iSettings) : IDatabase
    {
        private readonly IBwsDatabaseService client = iClient;
        private readonly ISettings settings = iSettings;

        private readonly List<Section> sectionsList = [];   // Internal copy

        // ============================
        // Prepare the database for use
        // ============================
        public string Initialize()  // Called from main form when path to database is set
        {
            string returnMessage = client!.Initialize(new GrpcPathToDatabaseRequest() { PathToDatabase = settings.PathToDatabase}).ReturnMessage;
            if (returnMessage == "")
            {
                settings.IsIndividual = client!.GetIsIndividual().IsIndividual;
            }
            return returnMessage;
        }

        private static bool sessionStarted = false;
        public void Prepare()  // Called from webapp StartScreen 
        {
            if (!sessionStarted)
            {
                client!.SetConnectionString(new GrpcPathToDatabaseRequest() { PathToDatabase = settings.PathToDatabase });

                // Get internal copy of sections list
                sectionsList.Clear();
                foreach (GrpcSection sectionResponse in client!.GetDatabaseSectionsList())
                {
                    sectionsList.Add(ObjectConvert<Section>(sectionResponse));
                }

                // Update internal copy of sections list with the number of devices per table in each section
                foreach (Section section in GetSectionsList())
                {
                    section.DevicesPerTable = 1;
                    if (settings.TabletsMove)
                    {
                        if (settings.IsIndividual)
                        {
                            section.DevicesPerTable = 4;
                        }
                        else
                        {
                            if (section.Winners == 1) section.DevicesPerTable = 2;
                        }
                    }
                }
                sessionStarted = true;
            }
        }

        public bool IsDatabaseConnectionOK()
        {
            return client!.IsDatabaseConnectionOK().IsDatabaseConnectionOK;
        } 
        

        // ========================================
        // Implement methods to access the database
        // ========================================

        // SECTION
        public Section GetSection(int sectionID)
        {
            Section? section = sectionsList.Find(x => x.ID == sectionID);
            section ??= sectionsList[0];
            return section;
        }

        public List<Section> GetSectionsList()
        {
            return sectionsList;
        }

        // TABLE
        public void RegisterTable(int sectionID, int tableNumber)
        {
            client!.RegisterTable(new GrpcSectionTableRequest(){ SectionID = sectionID, TableNumber = tableNumber });
        }

        // ROUND
        public int GetNumberOfRoundsInEvent(int sectionID, int roundNumber = 999)
        {
            return client!.GetNumberOfRoundsInEvent(new GrpcSectionRoundRequest() { SectionID = sectionID, RoundNumber = roundNumber }).NumberOfRoundsInEvent;
        }

        public int GetNumberOfLastRoundWithResults(int sectionID, int tableNumber)
        {
            return client!.GetNumberOfLastRoundWithResults(new GrpcSectionTableRequest() { SectionID = sectionID, TableNumber = tableNumber }).NumberOfLastRoundWithResults;
        }

        public List<Round> GetRoundsList(int sectionID, int roundNumber) 
        {
            List<Round> roundsList = [];
            foreach (GrpcRound roundResponse in client!.GetRoundsList(new GrpcSectionRoundRequest() { SectionID = sectionID, RoundNumber = roundNumber }))
            {
                roundsList.Add(ObjectConvert<Round>(roundResponse));
            }
            return roundsList;
        }

        public Round GetRoundData(int sectionID, int tableNumber, int roundNumber)
        {
            return ObjectConvert<Round>(client!.GetRoundData(new GrpcSectionTableRoundRequest { SectionID = sectionID, TableNumber = tableNumber, RoundNumber = roundNumber }));
        }

        // RECEIVEDDATA
        public Result GetResult(int sectionID, int tableNumber, int roundNumber, int boardNumber)
        {
            return ObjectConvert<Result>(client!.GetResult(new GrpcSectionTableRoundBoardRequest { SectionID = sectionID, TableNumber = tableNumber, RoundNumber = roundNumber, BoardNumber = boardNumber }));
        }

        public void SetResult(Result result)
        {
            client!.SetResult(ObjectConvert<GrpcResult>(result));
        }

        public List<Result> GetResultsList(int sectionID = 0, int lowBoard = 0, int highBoard = 0, int tableNumber = 0, int roundNumber = 0)
        {
            List<Result> resultsList = [];
            foreach (GrpcResult result in client!.GetResultsList(new GrpcResultsListRequest() { SectionID = sectionID, LowBoard = lowBoard, HighBoard = highBoard, TableNumber = tableNumber, RoundNumber = roundNumber }))
            {
                resultsList.Add(ObjectConvert<Result>(result));
            }
            return resultsList;
        }

        // PLAYERNAMES
        public string GetInternalPlayerName(string PlayerID)
        {
            string name = client!.GetInternalPlayerName(new GrpcPlayerRequest() { PlayerID = PlayerID }).PlayerName;
            if (name == "Unknown")
            {
                return "#" + PlayerID;
            }
            else
            {
                return name;
            }
        }

        // PLAYERNUMBERS
        public Names GetNamesForRound(int sectionID, int roundNumber, int numberNorth, int numberEast, int numberSouth, int numberWest)
        {
            return ObjectConvert<Names>(client!.GetNamesForRound(new GrpcNamesForRoundRequest { SectionID = sectionID, RoundNumber = roundNumber, NumberNorth = numberNorth, NumberEast = numberEast, NumberSouth = numberSouth, NumberWest = numberWest }));
        }

        public void UpdatePlayer(int sectionID, int tableNumber, int roundNumber, string directionLetter, int pairNumber, string playerID, string playerName)
        {
            client!.UpdatePlayer(new GrpcUpdatePlayerNumberRequest() { SectionID = sectionID, TableNumber = tableNumber, RoundNumber = roundNumber, DirectionLetter = directionLetter, PairNumber = pairNumber, PlayerID = playerID, PlayerName = playerName });
        }

        // HANDRECORD
        public int GetHandsCount() 
        {
            return client!.GetHandsCount().HandsCount;
        }

        public List<Hand> GetHandsList()
        {
            List<Hand> handsList = [];
            foreach (GrpcHand hand in client!.GetHandsList())
            {
                handsList.Add(ObjectConvert<Hand>(hand));
            }
            return handsList;
        }

        public Hand GetHand(int sectionID, int boardNumber)
        {
            return ObjectConvert<Hand>(client!.GetHand(new GrpcSectionBoardRequest { SectionID = sectionID, BoardNumber = boardNumber }));
        }

        public void AddHand(Hand hand)
        {
            client!.AddHand(ObjectConvert<GrpcHand>(hand));
        }

        public void AddHands(List<Hand> newHandsList)
        {
            List<GrpcHand> handsListResponse = [];
            foreach (Hand hand in newHandsList)
            {
                handsListResponse.Add(ObjectConvert<GrpcHand>(hand));
            }
            client!.AddHands(handsListResponse);
        }

        // SETTINGS
        private static int settingsRoundNumber = 0; 
        public void GetDatabaseSettings(int roundNumber = 1)
        {
            if (roundNumber <= settingsRoundNumber) return; 
            GrpcDatabaseSettings databaseSettings = client!.GetDatabaseSettings();
            settings.ShowTraveller = databaseSettings.ShowTraveller;
            settings.ShowPercentage = databaseSettings.ShowPercentage;
            settings.EnterLeadCard = databaseSettings.EnterLeadCard;
            settings.ValidateLeadCard = databaseSettings.EnterLeadCard;
            settings.ShowRanking = databaseSettings.ShowRanking;
            settings.ShowHandRecord = databaseSettings.ShowHandRecord;
            settings.NumberEntryEachRound = databaseSettings.NumberEntryEachRound;
            settings.NameSource = databaseSettings.NameSource;
            settings.EnterResultsMethod = databaseSettings.EnterResultsMethod;
            settings.ManualHandRecordEntry = databaseSettings.ManualHandRecordEntry;
            if (roundNumber == settingsRoundNumber + 1) settingsRoundNumber = roundNumber;
        }

        public void SetDatabaseSettings()
        {
            GrpcDatabaseSettings databaseSettings = new()
            {
                ShowTraveller = settings.ShowTraveller,
                ShowPercentage = settings.ShowPercentage,
                EnterLeadCard = settings.EnterLeadCard,
                ValidateLeadCard = settings.ValidateLeadCard,
                ShowRanking = settings.ShowRanking,
                ShowHandRecord = settings.ShowHandRecord,
                NumberEntryEachRound = settings.NumberEntryEachRound,
                NameSource = settings.NameSource,
                EnterResultsMethod = settings.EnterResultsMethod,
                ManualHandRecordEntry = settings.ManualHandRecordEntry
            };
            client!.SetDatabaseSettings(databaseSettings);
        }

        // RANKINGLIST
        public List<Ranking> GetRankingList(int sectionID)
        {
            List<Ranking> rankingList = [];
            foreach (GrpcRanking rankingResponse in client!.GetRankingList(new GrpcSectionIDRequest() { SectionID = sectionID }))
            {
                rankingList.Add(ObjectConvert<Ranking>(rankingResponse));
            }
            return rankingList;
        }

        private static T ObjectConvert<T>(object oldObject) where T : new()
        {
            T newObject = new();
            if (oldObject == null) return newObject;
            Type newObjectType = typeof(T);
            Type oldObjectType = oldObject.GetType();
            PropertyInfo[] propertyList = newObjectType.GetProperties();
            foreach (PropertyInfo newObjectProperty in propertyList)
            {
                PropertyInfo? oldObjectProperty = oldObjectType.GetProperty(newObjectProperty.Name);
                newObjectProperty.SetValue(newObject, oldObjectProperty!.GetValue(oldObject));
            }
            return newObject;
        }
    }
}
