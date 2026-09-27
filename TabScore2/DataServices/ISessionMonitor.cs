// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

using TabScore2.Classes;

namespace TabScore2.DataServices
{
    // ISessionMonitor provides a read-only view of the state of play for the desktop admin screen and the scorer's web page.
    // It never creates or changes AppData state, and never writes to the database.
    public interface ISessionMonitor
    {
        // Compares the round 1 movement (and player names in the database) with the devices registered so far.
        // The movement and names are cached for a short time; set reloadLayout to force a fresh database read.
        // Device registrations are always current.
        SessionCheck GetStartOfPlayCheck(bool reloadLayout = false);
    }
}
