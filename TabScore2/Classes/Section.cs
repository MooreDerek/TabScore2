// TabScore2, a wireless bridge scoring program.  Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Classes
{
    public class Section
    {
        public int ID { get; set; }
        public string Letter { get; set; } = "A";
        public int Tables { get; set; }
        public int MissingPair { get; set; }
        public int Winners { get; set; }
        public int TabletDevicesPerTable { get; set; } = 1;
    }
}
