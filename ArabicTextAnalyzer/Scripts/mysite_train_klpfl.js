function JsShowStatsPage() {

    // disable any other active tab content / activate current fb page tab content
    $('.tab-pane').each(function () {
        $(this).removeClass('active');
    });
    $('#statscontent').addClass('active')

    // disable any other active tab header / activate current fb page tab header
    $('.mainUL').each(function () {
        $(this).removeClass('active');
    })
    $('#statsheader').addClass('active');
}