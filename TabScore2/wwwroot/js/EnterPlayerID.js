// TabScore2, a wireless bridge scoring program.Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

var playerID = "";
var isSubmitted = false;

function addNumber(e) {
    if (playerID == stringUnknown) {
        playerID = "";
    }
    playerID = playerID + e;
    document.getElementById('playerNumberBox').value = playerID;
    document.getElementById("OKButton").disabled = false;
}

function unknown() {
    playerID = stringUnknown;
    document.getElementById('playerNumberBox').value = playerID;
    document.getElementById("OKButton").disabled = false;
}

function clearplayerNumber() {
    playerID = ""
    document.getElementById('playerNumberBox').value = "";
    document.getElementById("OKButton").disabled = true;
}

function clearLastEntry() {
    if (playerID == stringUnknown) {
        playerID = "";
        document.getElementById("OKButton").disabled = true;
    }
    else {
        if (playerID.length > 0) {
            playerID = playerID.slice(0, -1);
            if (playerID == "") document.getElementById("OKButton").disabled = true;
        }
    }
    document.getElementById('playerNumberBox').value = playerID;
}

function OKButtonClick() {
    if (document.getElementById("OKButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlOKButtonClick + '&playerID=' + playerID;
    }
}