// TabScore2, a wireless bridge scoring program.Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

setNSEW(model.PerspectiveDirection);

function setNSEW(direction) {
    var btn = document.getElementById('btnNorth');
    if (btn) btn.className = "btn btn-secondary m-1";
    btn = document.getElementById('btnSouth')
    if (btn) btn.className = "btn btn-secondary m-1";
    btn = document.getElementById('btnEast');
    if (btn) btn.className = "btn btn-secondary m-1";
    btn = document.getElementById('btnWest')
    if (btn) btn.className = "btn btn-secondary m-1";
    if (direction == 'North') {
        btn = document.getElementById('btnNorth');
        if (btn) btn.className = "btn btn-warning m-1";
        document.getElementById('TS').innerHTML = model.SouthSpadesDisplay;
        document.getElementById('TH').innerHTML = model.SouthHeartsDisplay;
        document.getElementById('TD').innerHTML = model.SouthDiamondsDisplay;
        document.getElementById('TC').innerHTML = model.SouthClubsDisplay;
        document.getElementById('LS').innerHTML = model.EastSpadesDisplay;
        document.getElementById('LH').innerHTML = model.EastHeartsDisplay;
        document.getElementById('LD').innerHTML = model.EastDiamondsDisplay;
        document.getElementById('LC').innerHTML = model.EastClubsDisplay;
        document.getElementById('RS').innerHTML = model.WestSpadesDisplay;
        document.getElementById('RH').innerHTML = model.WestHeartsDisplay;
        document.getElementById('RD').innerHTML = model.WestDiamondsDisplay;
        document.getElementById('RC').innerHTML = model.WestClubsDisplay;
        document.getElementById('BS').innerHTML = model.NorthSpadesDisplay;
        document.getElementById('BH').innerHTML = model.NorthHeartsDisplay;
        document.getElementById('BD').innerHTML = model.NorthDiamondsDisplay;
        document.getElementById('BC').innerHTML = model.NorthClubsDisplay;
        document.getElementById('T').innerHTML = stringS + " (" + model.HCPSouth + ")";
        document.getElementById('L').innerHTML = stringE + " (" + model.HCPEast + ")";
        document.getElementById('R').innerHTML = "(" + model.HCPWest + ") " + stringW;
        document.getElementById('B').innerHTML = stringN + " (" + model.HCPNorth + ")";
    }
    else if (direction == 'South') {
        btn = document.getElementById('btnSouth');
        if (btn) btn.className = "btn btn-warning m-1";
        document.getElementById('TS').innerHTML = model.NorthSpadesDisplay;
        document.getElementById('TH').innerHTML = model.NorthHeartsDisplay;
        document.getElementById('TD').innerHTML = model.NorthDiamondsDisplay;
        document.getElementById('TC').innerHTML = model.NorthClubsDisplay;
        document.getElementById('LS').innerHTML = model.WestSpadesDisplay;
        document.getElementById('LH').innerHTML = model.WestHeartsDisplay;
        document.getElementById('LD').innerHTML = model.WestDiamondsDisplay;
        document.getElementById('LC').innerHTML = model.WestClubsDisplay;
        document.getElementById('RS').innerHTML = model.EastSpadesDisplay;
        document.getElementById('RH').innerHTML = model.EastHeartsDisplay;
        document.getElementById('RD').innerHTML = model.EastDiamondsDisplay;
        document.getElementById('RC').innerHTML = model.EastClubsDisplay;
        document.getElementById('BS').innerHTML = model.SouthSpadesDisplay;
        document.getElementById('BH').innerHTML = model.SouthHeartsDisplay;
        document.getElementById('BD').innerHTML = model.SouthDiamondsDisplay;
        document.getElementById('BC').innerHTML = model.SouthClubsDisplay;
        document.getElementById('T').innerHTML = stringN + " (" + model.HCPNorth + ")";
        document.getElementById('L').innerHTML = stringW + " (" + model.HCPWest + ")";
        document.getElementById('R').innerHTML = "(" + model.HCPEast + ") " + stringE;
        document.getElementById('B').innerHTML = stringS + " (" + model.HCPSouth + ")";
    }
    else if (direction == 'East')
    {
        btn = document.getElementById('btnEast');
        if (btn) btn.className = "btn btn-warning m-1";
        document.getElementById('TS').innerHTML = model.WestSpadesDisplay;
        document.getElementById('TH').innerHTML = model.WestHeartsDisplay;
        document.getElementById('TD').innerHTML = model.WestDiamondsDisplay;
        document.getElementById('TC').innerHTML = model.WestClubsDisplay;
        document.getElementById('LS').innerHTML = model.SouthSpadesDisplay;
        document.getElementById('LH').innerHTML = model.SouthHeartsDisplay;
        document.getElementById('LD').innerHTML = model.SouthDiamondsDisplay;
        document.getElementById('LC').innerHTML = model.SouthClubsDisplay;
        document.getElementById('RS').innerHTML = model.NorthSpadesDisplay;
        document.getElementById('RH').innerHTML = model.NorthHeartsDisplay;
        document.getElementById('RD').innerHTML = model.NorthDiamondsDisplay;
        document.getElementById('RC').innerHTML = model.NorthClubsDisplay;
        document.getElementById('BS').innerHTML = model.EastSpadesDisplay;
        document.getElementById('BH').innerHTML = model.EastHeartsDisplay;
        document.getElementById('BD').innerHTML = model.EastDiamondsDisplay;
        document.getElementById('BC').innerHTML = model.EastClubsDisplay;
        document.getElementById('T').innerHTML = stringW + " (" + model.HCPWest + ")";
        document.getElementById('L').innerHTML = stringS + " (" + model.HCPSouth + ")";
        document.getElementById('R').innerHTML = "(" + model.HCPNorth + ") " + stringN;
        document.getElementById('B').innerHTML = stringE + " (" + model.HCPEast + ")";
    }
    else if (direction == 'West') {
        btn = document.getElementById('btnWest')
        if (btn) btn.className = "btn btn-warning m-1";
        document.getElementById('TS').innerHTML = model.EastSpadesDisplay;
        document.getElementById('TH').innerHTML = model.EastHeartsDisplay;
        document.getElementById('TD').innerHTML = model.EastDiamondsDisplay;
        document.getElementById('TC').innerHTML = model.EastClubsDisplay;
        document.getElementById('LS').innerHTML = model.NorthSpadesDisplay;
        document.getElementById('LH').innerHTML = model.NorthHeartsDisplay;
        document.getElementById('LD').innerHTML = model.NorthDiamondsDisplay;
        document.getElementById('LC').innerHTML = model.NorthClubsDisplay;
        document.getElementById('RS').innerHTML = model.SouthSpadesDisplay;
        document.getElementById('RH').innerHTML = model.SouthHeartsDisplay;
        document.getElementById('RD').innerHTML = model.SouthDiamondsDisplay;
        document.getElementById('RC').innerHTML = model.SouthClubsDisplay;
        document.getElementById('BS').innerHTML = model.WestSpadesDisplay;
        document.getElementById('BH').innerHTML = model.WestHeartsDisplay;
        document.getElementById('BD').innerHTML = model.WestDiamondsDisplay;
        document.getElementById('BC').innerHTML = model.WestClubsDisplay;
        document.getElementById('T').innerHTML = stringE + " (" + model.HCPEast + ")";
        document.getElementById('L').innerHTML = stringN + " (" + model.HCPNorth + ")";
        document.getElementById('R').innerHTML = "(" + model.HCPSouth + ") " + stringS;
        document.getElementById('B').innerHTML = stringW + " (" + model.HCPWest + ")";
    }
}