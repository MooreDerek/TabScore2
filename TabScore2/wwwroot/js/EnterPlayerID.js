// TabScore2, a wireless bridge scoring program.Copyright(C) 2025 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

var playerId = "";
var isSubmitted = false;

function addNumber(e) {
    if (playerId == stringUnknown) {
        playerId = "";
    }
    playerId = playerId + e;
    document.getElementById('playerNumberBox').value = playerId;
    document.getElementById("OKButton").disabled = false;
}

function unknown() {
    playerId = stringUnknown;
    document.getElementById('playerNumberBox').value = playerId;
    document.getElementById("OKButton").disabled = false;
}

function clearplayerNumber() {
    playerId = ""
    document.getElementById('playerNumberBox').value = "";
    document.getElementById("OKButton").disabled = true;
}

function clearLastEntry() {
    if (playerId == stringUnknown) {
        playerId = "";
        document.getElementById("OKButton").disabled = true;
    }
    else {
        if (playerId.length > 0) {
            playerId = playerId.slice(0, -1);
            if (playerId == "") document.getElementById("OKButton").disabled = true;
        }
    }
    document.getElementById('playerNumberBox').value = playerId;
}

function OKButtonClick() {
    if (document.getElementById("OKButton").disabled) return;
    if (!isSubmitted) {
        isSubmitted = true;
        location.href = urlOKButtonClick + '&playerId=' + playerId;
    }
}