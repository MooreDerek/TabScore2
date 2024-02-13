// TabScore2, a wireless bridge scoring program.Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

setNSEW(model.perspectiveFromDirection);

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
        document.getElementById('TS').innerHTML = model.southSpadesDisplay;
        document.getElementById('TH').innerHTML = model.southHeartsDisplay;
        document.getElementById('TD').innerHTML = model.southDiamondsDisplay;
        document.getElementById('TC').innerHTML = model.southClubsDisplay;
        document.getElementById('LS').innerHTML = model.eastSpadesDisplay;
        document.getElementById('LH').innerHTML = model.eastHeartsDisplay;
        document.getElementById('LD').innerHTML = model.eastDiamondsDisplay;
        document.getElementById('LC').innerHTML = model.eastClubsDisplay;
        document.getElementById('RS').innerHTML = model.westSpadesDisplay;
        document.getElementById('RH').innerHTML = model.westHeartsDisplay;
        document.getElementById('RD').innerHTML = model.westDiamondsDisplay;
        document.getElementById('RC').innerHTML = model.westClubsDisplay;
        document.getElementById('BS').innerHTML = model.northSpadesDisplay;
        document.getElementById('BH').innerHTML = model.northHeartsDisplay;
        document.getElementById('BD').innerHTML = model.northDiamondsDisplay;
        document.getElementById('BC').innerHTML = model.northClubsDisplay;
        document.getElementById('TDir').innerHTML = stringS;
        document.getElementById('LDir').innerHTML = stringE;
        document.getElementById('RDir').innerHTML = stringW;
        document.getElementById('BDir').innerHTML = stringN;
        document.getElementById('THCP').innerHTML = "(" + model.hcpSouth + ")";
        document.getElementById('LHCP').innerHTML = "(" + model.hcpEast + ")";
        document.getElementById('RHCP').innerHTML = "(" + model.hcpWest + ")";
        document.getElementById('BHCP').innerHTML = "(" + model.hcpNorth + ")";
    }
    else if (direction == 'South') {
        btn = document.getElementById('btnSouth');
        if (btn) btn.className = "btn btn-warning m-1";
        document.getElementById('TS').innerHTML = model.northSpadesDisplay;
        document.getElementById('TH').innerHTML = model.northHeartsDisplay;
        document.getElementById('TD').innerHTML = model.northDiamondsDisplay;
        document.getElementById('TC').innerHTML = model.northClubsDisplay;
        document.getElementById('LS').innerHTML = model.westSpadesDisplay;
        document.getElementById('LH').innerHTML = model.westHeartsDisplay;
        document.getElementById('LD').innerHTML = model.westDiamondsDisplay;
        document.getElementById('LC').innerHTML = model.westClubsDisplay;
        document.getElementById('RS').innerHTML = model.eastSpadesDisplay;
        document.getElementById('RH').innerHTML = model.eastHeartsDisplay;
        document.getElementById('RD').innerHTML = model.eastDiamondsDisplay;
        document.getElementById('RC').innerHTML = model.eastClubsDisplay;
        document.getElementById('BS').innerHTML = model.southSpadesDisplay;
        document.getElementById('BH').innerHTML = model.southHeartsDisplay;
        document.getElementById('BD').innerHTML = model.southDiamondsDisplay;
        document.getElementById('BC').innerHTML = model.southClubsDisplay;
        document.getElementById('TDir').innerHTML = stringN;
        document.getElementById('LDir').innerHTML = stringW;
        document.getElementById('RDir').innerHTML = stringE;
        document.getElementById('BDir').innerHTML = stringS;
        document.getElementById('THCP').innerHTML = "(" + model.hcpNorth + ")";
        document.getElementById('LHCP').innerHTML = "(" + model.hcpWest + ")";
        document.getElementById('RHCP').innerHTML = "(" + model.hcpEast + ")";
        document.getElementById('BHCP').innerHTML = "(" + model.hcpSouth + ")";
    }
    else if (direction == 'East')
    {
        btn = document.getElementById('btnEast');
        if (btn) btn.className = "btn btn-warning m-1";
        document.getElementById('TS').innerHTML = model.westSpadesDisplay;
        document.getElementById('TH').innerHTML = model.westHeartsDisplay;
        document.getElementById('TD').innerHTML = model.westDiamondsDisplay;
        document.getElementById('TC').innerHTML = model.westClubsDisplay;
        document.getElementById('LS').innerHTML = model.southSpadesDisplay;
        document.getElementById('LH').innerHTML = model.southHeartsDisplay;
        document.getElementById('LD').innerHTML = model.southDiamondsDisplay;
        document.getElementById('LC').innerHTML = model.southClubsDisplay;
        document.getElementById('RS').innerHTML = model.northSpadesDisplay;
        document.getElementById('RH').innerHTML = model.northHeartsDisplay;
        document.getElementById('RD').innerHTML = model.northDiamondsDisplay;
        document.getElementById('RC').innerHTML = model.northClubsDisplay;
        document.getElementById('BS').innerHTML = model.eastSpadesDisplay;
        document.getElementById('BH').innerHTML = model.eastHeartsDisplay;
        document.getElementById('BD').innerHTML = model.eastDiamondsDisplay;
        document.getElementById('BC').innerHTML = model.eastClubsDisplay;
        document.getElementById('TDir').innerHTML = stringW;
        document.getElementById('LDir').innerHTML = stringS;
        document.getElementById('RDir').innerHTML = stringN;
        document.getElementById('BDir').innerHTML = stringE;
        document.getElementById('THCP').innerHTML = "(" + model.hcpWest + ")";
        document.getElementById('LHCP').innerHTML = "(" + model.hcpSouth + ")";
        document.getElementById('RHCP').innerHTML = "(" + model.hcpNorth + ")";
        document.getElementById('BHCP').innerHTML = "(" + model.hcpEast + ")";
    }
    else if (direction == 'West') {
        btn = document.getElementById('btnWest')
        if (btn) btn.className = "btn btn-warning m-1";
        document.getElementById('TS').innerHTML = model.eastSpadesDisplay;
        document.getElementById('TH').innerHTML = model.eastHeartsDisplay;
        document.getElementById('TD').innerHTML = model.eastDiamondsDisplay;
        document.getElementById('TC').innerHTML = model.eastClubsDisplay;
        document.getElementById('LS').innerHTML = model.northSpadesDisplay;
        document.getElementById('LH').innerHTML = model.northHeartsDisplay;
        document.getElementById('LD').innerHTML = model.northDiamondsDisplay;
        document.getElementById('LC').innerHTML = model.northClubsDisplay;
        document.getElementById('RS').innerHTML = model.southSpadesDisplay;
        document.getElementById('RH').innerHTML = model.southHeartsDisplay;
        document.getElementById('RD').innerHTML = model.southDiamondsDisplay;
        document.getElementById('RC').innerHTML = model.southClubsDisplay;
        document.getElementById('BS').innerHTML = model.westSpadesDisplay;
        document.getElementById('BH').innerHTML = model.westHeartsDisplay;
        document.getElementById('BD').innerHTML = model.westDiamondsDisplay;
        document.getElementById('BC').innerHTML = model.westClubsDisplay;
        document.getElementById('TDir').innerHTML = stringE;
        document.getElementById('LDir').innerHTML = stringN;
        document.getElementById('RDir').innerHTML = stringS;
        document.getElementById('BDir').innerHTML = stringW;
        document.getElementById('THCP').innerHTML = "(" + model.hcpEast + ")";
        document.getElementById('LHCP').innerHTML = "(" + model.hcpNorth + ")";
        document.getElementById('RHCP').innerHTML = "(" + model.hcpSouth + ")";
        document.getElementById('BHCP').innerHTML = "(" + model.hcpWest + ")";
    }
}