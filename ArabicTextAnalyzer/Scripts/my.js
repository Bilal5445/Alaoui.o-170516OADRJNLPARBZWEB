function getPlainLoadTime() {
    var pgloadTime = window.performance.timing.domContentLoadedEventEnd - window.performance.timing.navigationStart;

    // format
    var ms0 = pgloadTime,
        min0 = (ms0 / 1000 / 60) << 0,
        sec0 = (ms0 / 1000) % 60;

    var elapsed = Math.round(sec0 * 100) / 100 + 's';
    document.getElementById("loadTime").innerHTML = elapsed;
}