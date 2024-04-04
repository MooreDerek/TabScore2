// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using System.Runtime.Serialization;
using System.ServiceModel;

namespace SharedContracts
{
    [DataContract]
    public class GrpcPathToDatabaseRequest
    {
        [DataMember(Order = 1)] public string PathToDatabase { get; set; } = string.Empty;
    }

    [DataContract]
    public class GrpcIsIndividual
    {
        [DataMember(Order = 1)] public bool IsIndividual { get; set; }
    }

    [DataContract]
    public class GrpcReturnMessage
    {
        [DataMember(Order = 1)] public string ReturnMessage { get; set; } = string.Empty;
    }

    [DataContract]
    public class GrpcIsDatabaseConnectionOK
    {
        [DataMember(Order = 1)] public bool IsDatabaseConnectionOK { get; set; }
    }

    [DataContract]
    public class GrpcSectionIDRequest
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
    }

    [DataContract]
    public class GrpcNumberOfRoundsInEvent
    {
        [DataMember(Order = 1)] public int NumberOfRoundsInEvent { get; set; }
    }

    [DataContract]
    public class GrpcNumberOfLastRoundWithResults
    {
        [DataMember(Order = 1)] public int NumberOfLastRoundWithResults { get; set; }
    }

    [DataContract]
    public class GrpcPlayerRequest
    {
        [DataMember(Order = 1)] public string PlayerID { get; set; } = string.Empty;
    }

    [DataContract]
    public class GrpcPlayerName
    {
        [DataMember(Order = 1)] public string PlayerName { get; set; } = string.Empty;
    }

    [DataContract]
    public class GrpcHandsCount
    {
        [DataMember(Order = 1)] public int HandsCount { get; set; }
    }

    [DataContract]
    public class GrpcSectionTableRequest
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
    }

    [DataContract]
    public class GrpcSectionRoundRequest
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int RoundNumber { get; set; } = 0;
    }
    [DataContract]
    public class GrpcSectionBoardRequest
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int BoardNumber { get; set; }
    }

    [DataContract]
    public class GrpcSectionTableRoundRequest
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
        [DataMember(Order = 3)] public int RoundNumber { get; set; }
    }
    [DataContract]
    public class GrpcSectionTableRoundBoardRequest
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
        [DataMember(Order = 3)] public int RoundNumber { get; set; }
        [DataMember(Order = 4)] public int BoardNumber { get; set; }
    }

    [DataContract]
    public class GrpcResultsListRequest
    {
        [DataMember(Order = 1)] public int SectionID { get; set; } = 0;
        [DataMember(Order = 2)] public int LowBoard { get; set; } = 0;
        [DataMember(Order = 3)] public int HighBoard { get; set; } = 0;
        [DataMember(Order = 4)] public int TableNumber { get; set; } = 0;
        [DataMember(Order = 5)] public int RoundNumber { get; set; } = 0;
    }

    [DataContract]
    public class GrpcUpdatePlayerNumberRequest
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int TableNumber { get; set; }
        [DataMember(Order = 3)] public int RoundNumber { get; set; }
        [DataMember(Order = 4)] public string DirectionLetter { get; set; } = string.Empty;
        [DataMember(Order = 5)] public int PairNumber { get; set; }
        [DataMember(Order = 6)] public string PlayerID { get; set; } = string.Empty;
        [DataMember(Order = 7)] public string PlayerName { get; set; } = string.Empty;
    }

    [DataContract] 
    public class GrpcNamesForRoundRequest
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int RoundNumber { get; set; }
        [DataMember(Order = 3)] public int NumberNorth { get; set; }
        [DataMember(Order = 4)] public int NumberEast { get; set; }
        [DataMember(Order = 5)] public int NumberSouth { get; set; }
        [DataMember(Order = 6)] public int NumberWest { get; set; }
    }

    [DataContract] 
    public class GrpcSection
    {
        [DataMember(Order = 1)] public int ID { get; set; }
        [DataMember(Order = 2)] public string Letter { get; set; } = string.Empty;
        [DataMember(Order = 3)] public int Tables { get; set; }
        [DataMember(Order = 4)] public int MissingPair { get; set; }
        [DataMember(Order = 5)] public int Winners { get; set; }
        [DataMember(Order = 6)] public int DevicesPerTable { get; set; }
    }

    [DataContract]
    public class GrpcRound
    {
        [DataMember(Order = 1)] public int TableNumber { get; set; }
        [DataMember(Order = 2)] public int NumberNorth { get; set; }
        [DataMember(Order = 3)] public int NumberEast { get; set; }
        [DataMember(Order = 4)] public int NumberSouth { get; set; }
        [DataMember(Order = 5)] public int NumberWest { get; set; }
        [DataMember(Order = 6)] public string NameNorth { get; set; } = string.Empty;
        [DataMember(Order = 7)] public string NameSouth { get; set; } = string.Empty;
        [DataMember(Order = 8)] public string NameEast { get; set; } = string.Empty;
        [DataMember(Order = 9)] public string NameWest { get; set; } = string.Empty;
        [DataMember(Order = 10)] public bool GotAllNames { get; set; }
        [DataMember(Order = 11)] public int LowBoard { get; set; }
        [DataMember(Order = 12)] public int HighBoard { get; set; }
    }

    [DataContract]
    public class GrpcResult
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public string SectionLetter { get; set; } = string.Empty;
        [DataMember(Order = 3)] public int TableNumber { get; set; }
        [DataMember(Order = 4)] public int RoundNumber { get; set; }
        [DataMember(Order = 5)] public int BoardNumber { get; set; }
        [DataMember(Order = 6)] public int NumberNorth { get; set; }
        [DataMember(Order = 7)] public int NumberEast { get; set; }
        [DataMember(Order = 8)] public int NumberSouth { get; set; }
        [DataMember(Order = 9)] public int NumberWest { get; set; }
        [DataMember(Order = 10)] public string DeclarerNSEW { get; set; } = string.Empty;
        [DataMember(Order = 11)] public bool Vulnerable { get; set; }
        [DataMember(Order = 12)] public int ContractLevel { get; set; }
        [DataMember(Order = 13)] public string ContractSuit { get; set; } = string.Empty;
        [DataMember(Order = 14)] public string ContractX { get; set; } = string.Empty;
        [DataMember(Order = 15)] public string LeadCard { get; set; } = string.Empty;
        [DataMember(Order = 16)] public int TricksTaken { get; set; }
        [DataMember(Order = 17)] public string TricksTakenSymbol { get; set; } = string.Empty;
        [DataMember(Order = 18)] public string Remarks { get; set; } = string.Empty;
        [DataMember(Order = 19)] public int Score { get; set; }
        [DataMember(Order = 20)] public double MatchpointsNS { get; set; }
        [DataMember(Order = 21)] public double MatchpointsEW { get; set; }
    }

    [DataContract]
    public class GrpcNames
    {
        [DataMember(Order = 1)] public string NameNorth { get; set; } = string.Empty;
        [DataMember(Order = 2)] public string NameSouth { get; set; } = string.Empty;
        [DataMember(Order = 3)] public string NameEast { get; set; } = string.Empty;
        [DataMember(Order = 4)] public string NameWest { get; set; } = string.Empty;
        [DataMember(Order = 5)] public bool GotAllNames { get; set; }
    }

    [DataContract]
    public class GrpcHand
    {
        [DataMember(Order = 1)] public int SectionID { get; set; }
        [DataMember(Order = 2)] public int BoardNumber { get; set; }
        [DataMember(Order = 3)] public string NorthSpades { get; set; } = string.Empty;
        [DataMember(Order = 4)] public string NorthHearts { get; set; } = string.Empty;
        [DataMember(Order = 5)] public string NorthDiamonds { get; set; } = string.Empty;
        [DataMember(Order = 6)] public string NorthClubs { get; set; } = string.Empty;
        [DataMember(Order = 7)] public string EastSpades { get; set; } = string.Empty;
        [DataMember(Order = 8)] public string EastHearts { get; set; } = string.Empty;
        [DataMember(Order = 9)] public string EastDiamonds { get; set; } = string.Empty;
        [DataMember(Order = 10)] public string EastClubs { get; set; } = string.Empty;
        [DataMember(Order = 11)] public string SouthSpades { get; set; } = string.Empty;
        [DataMember(Order = 12)] public string SouthHearts { get; set; } = string.Empty;
        [DataMember(Order = 13)] public string SouthDiamonds { get; set; } = string.Empty;
        [DataMember(Order = 14)] public string SouthClubs { get; set; } = string.Empty;
        [DataMember(Order = 15)] public string WestSpades { get; set; } = string.Empty;
        [DataMember(Order = 16)] public string WestHearts { get; set; } = string.Empty;
        [DataMember(Order = 17)] public string WestDiamonds { get; set; } = string.Empty;
        [DataMember(Order = 18)] public string WestClubs { get; set; } = string.Empty;
    }

    [DataContract]
    public class GrpcDatabaseSettings
    {
        [DataMember(Order = 1)] public bool ShowTraveller { get; set; } = true;
        [DataMember(Order = 2)] public bool ShowPercentage { get; set; } = true;
        [DataMember(Order = 3)] public bool EnterLeadCard { get; set; } = true;
        [DataMember(Order = 5)] public bool ValidateLeadCard { get; set; } = true;
        [DataMember(Order = 6)] public int ShowRanking { get; set; } = 1;
        [DataMember(Order = 7)] public int EnterResultsMethod { get; set; } = 1;
        [DataMember(Order = 8)] public bool ShowHandRecord { get; set; } = true;
        [DataMember(Order = 9)] public bool NumberEntryEachRound { get; set; } = false;
        [DataMember(Order = 10)] public int NameSource { get; set; } = 0;
        [DataMember(Order = 11)] public bool ManualHandRecordEntry { get; set; } = false;
    }

    [DataContract]
    public class GrpcRanking
    {
        [DataMember(Order = 1)] public string Orientation { get; set; } = string.Empty;
        [DataMember(Order = 2)] public int PairNo { get; set; }
        [DataMember(Order = 3)] public string Score { get; set; } = string.Empty;
        [DataMember(Order = 4)] public double ScoreDecimal { get; set; }
        [DataMember(Order = 5)] public string Rank { get; set; } = string.Empty;
        [DataMember(Order = 6)] public double MP { get; set; }
        [DataMember(Order = 7)] public int MPMax { get; set; }
    }

    [ServiceContract]
    public interface IBwsDatabaseService
    {
        // GENERAL
        [OperationContract] void SetConnectionString(GrpcPathToDatabaseRequest request);
        [OperationContract] GrpcReturnMessage Initialize(GrpcPathToDatabaseRequest request);
        [OperationContract] GrpcIsIndividual GetIsIndividual();
        [OperationContract] GrpcIsDatabaseConnectionOK IsDatabaseConnectionOK();

        // SECTION
        [OperationContract] List<GrpcSection> GetDatabaseSectionsList();

        // TABLE
        [OperationContract] void RegisterTable(GrpcSectionTableRequest request);

        // ROUND
        [OperationContract] GrpcNumberOfRoundsInEvent GetNumberOfRoundsInEvent(GrpcSectionRoundRequest request);
        [OperationContract] GrpcNumberOfLastRoundWithResults GetNumberOfLastRoundWithResults(GrpcSectionTableRequest request);
        [OperationContract] List<GrpcRound> GetRoundsList(GrpcSectionRoundRequest request);
        [OperationContract] GrpcRound GetRoundData(GrpcSectionTableRoundRequest request);

        // RESULT = RECEIVEDDATA
        [OperationContract] GrpcResult GetResult(GrpcSectionTableRoundBoardRequest request);
        [OperationContract] void SetResult(GrpcResult grpcResult);
        [OperationContract] List<GrpcResult> GetResultsList(GrpcResultsListRequest request);

        // PLAYERNAMES
        [OperationContract] GrpcPlayerName GetInternalPlayerName(GrpcPlayerRequest request);

        // PLAYERNUMBERS
        [OperationContract] void UpdatePlayer(GrpcUpdatePlayerNumberRequest request);
        [OperationContract] GrpcNames GetNamesForRound(GrpcNamesForRoundRequest request);

        // HANDRECORD
        [OperationContract] GrpcHandsCount GetHandsCount();
        [OperationContract] List<GrpcHand> GetHandsList();
        [OperationContract] GrpcHand GetHand(GrpcSectionBoardRequest request);
        [OperationContract] void AddHand(GrpcHand hand);
        [OperationContract] void AddHands(List<GrpcHand> newHandsList);

        // SETTINGS
        [OperationContract] GrpcDatabaseSettings GetDatabaseSettings();
        [OperationContract] void SetDatabaseSettings(GrpcDatabaseSettings databaseSettings);

        // RANKINGLIST
        [OperationContract] List<GrpcRanking> GetRankingList(GrpcSectionIDRequest request);
    }

    [ServiceContract]
    public interface IExternalNamesDatabaseService
    {
        [OperationContract] GrpcPlayerName GetExternalPlayerName(GrpcPlayerRequest request);
    }
}