// TabScore, a wireless bridge scoring program.  Copyright(C) 2023 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Globals;

namespace TabScore2.Models
{
    public class EnterContract(int deviceNumber)
    {
        public int TabletDeviceNumber { get; private set; } = deviceNumber;
        public int BoardNumber { get; set; }
        public string DeclarerNSEW { get; set; } = string.Empty;
        public string DeclarerNSEWDisplay { get; set; } = string.Empty;
        public int ContractLevel { get; set; } = -999;
        public string ContractSuit { get; set; } = string.Empty;
        public string ContractX { get; set; } = string.Empty;
        public string ContractDisplay { get; set; } = string.Empty;
        public string LeadCard { get; set; } = string.Empty;
        public int TricksTaken { get; set; }
        public int Score { get; set; }
        public LeadValidationOptions LeadValidation { get; set; }
    }
}