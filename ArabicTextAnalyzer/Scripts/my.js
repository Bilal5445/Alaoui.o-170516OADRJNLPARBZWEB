function getPlainLoadTime() {
    var pgloadTime = window.performance.timing.domContentLoadedEventEnd - window.performance.timing.navigationStart;

    // format
    var ms0 = pgloadTime,
        min0 = (ms0 / 1000 / 60) << 0,
        sec0 = (ms0 / 1000) % 60;

    var elapsed = Math.round(sec0 * 100) / 100 + 's';
    document.getElementById("loadTime").innerHTML = elapsed;

    //
    logToServer("loadTime : " + elapsed);
}

function refreshPlainLoadTime() {
    var timeInMs = Date.now();
    var pgloadTime = timeInMs - window.performance.timing.navigationStart;

    // format
    var ms0 = pgloadTime,
        min0 = (ms0 / 1000 / 60) << 0,
        sec0 = (ms0 / 1000) % 60;

    var elapsed = Math.round(sec0 * 100) / 100 + 's';
    document.getElementById("loadTime").innerHTML = elapsed;

    //
    logToServer("loadTime : refresh : " + elapsed);
}

function logToServer(elapsed) {
    $.ajax({
        type: 'POST',
        url: "/Train/Log",
        data: { "message": elapsed },
        failure: function (response) {
            console.log(response);
        },
        error: function (response) {
            console.log(response);
        }
    });
}