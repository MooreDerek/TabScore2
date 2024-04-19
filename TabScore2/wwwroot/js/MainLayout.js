// TabScore2, a wireless bridge scoring program.Copyright(C) 2024 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

window.addEventListener('load', function () {
    document.body.style.paddingTop = document.getElementById("headerRow").offsetHeight.toString() + "px";
    document.body.style.paddingBottom = (document.getElementById("footerRow").offsetHeight + 10).toString() + "px";
    if ("getBattery" in navigator) {
        navigator.getBattery().then(function (battery) {
            document.getElementById("battery").style.display = "block";
            document.getElementById("header").className = "col-11 my-auto px-2";
            var batteryLevel = battery.level * 100;
            if (batteryLevel > 87.5) {
                document.getElementById("bl").className = "icon-battery-full float-end";
            }
            else if (batteryLevel > 62.5) {
                document.getElementById("bl").className = "icon-battery-three-quarters float-end";
            }
            else if (batteryLevel > 37.5) {
                document.getElementById("bl").className = "icon-battery-half float-end";
            }
            else if (batteryLevel > 12.5) {
                document.getElementById("bl").className = "icon-battery-quarter float-end";
            }
            else {
                document.getElementById("bl").className = "icon-battery-empty float-end";
            }
        });
    }
    if (timerSeconds >= 0) {
        setTimerValue(timerSeconds);
        startTimer(timerSeconds);
    }
    if (typeof onFullPageLoad == 'function') {
        onFullPageLoad();
    }
}, false);

function startTimer(t) {
    var timerInterval = setInterval(function () {
        setTimerValue(--t);
        if (t <= 0) clearInterval(timerInterval);
    }, 1000);
}

function setTimerValue(t) {
    if (t <= 0) {
        document.getElementById("timerValue").innerText = " 00:00";
        document.getElementById("timerButton").className = "btn btn-danger my-2 px-2";
    }
    else {
        minutes = parseInt(t / 60, 10);
        seconds = parseInt(t % 60, 10);
        minutes = minutes < 10 ? "0" + minutes : minutes;
        seconds = seconds < 10 ? "0" + seconds : seconds;
        document.getElementById("timerValue").innerText = " " + minutes + ":" + seconds;
        if (t <= 60) {
            document.getElementById("timerButton").className = "btn btn-warning my-2 px-2";
        }
    }
}