// TabScore, a wireless bridge scoring program.  Copyright(C) 2023 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;
using TabScore2.Globals;

namespace TabScore.Models
{
    public class EnterContract(int tabletDeviceNumber, Result result, LeadValidationOptions leadValidation = LeadValidationOptions.NoWarning)
    {
        public int TabletDeviceNumber { get; private set; } = tabletDeviceNumber;
        public Result ResultData { get; private set; } = result;
        public LeadValidationOptions LeadValidation { get; set; } = leadValidation;
    }
}