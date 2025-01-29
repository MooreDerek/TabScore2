// TabScore2, a wireless bridge scoring program.  Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;

namespace TabScore2.Models
{
    public class ShowBoardsModel(bool showViewButton) : List<ShowBoardsResult>
    {
        public bool GotAllResults { get; set; } = true;
        public bool ShowViewButton { get; private set; } = showViewButton;
        public string Message { get; set; } = string.Empty;
    }
}