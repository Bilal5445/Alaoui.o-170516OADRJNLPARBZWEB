$(document).ready(function () {
    // bring new data
    $.ajax({
        type: 'POST',
        url: "/Train/ArabicDarijaEntryPartialView",
        data: null,
        success: (function (result) {
            $('#partialPlaceHolder').html(result);
        }),
        failure: function (response) {
            console.log(response);
        },
        error: function (response) {
            console.log(response);
        }
    });
});