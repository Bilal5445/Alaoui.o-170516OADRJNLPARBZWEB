$(function () {
    $("#analyzeText").click(function () {

        var arabicText = $("#arabic-text").val();

        alert(arabicText);

        // change to POST for large text

        $.ajax({
            type: "GET",
            url: "/Home/ProcessText?text=" + arabicText,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
               console.log(response);
            },
            failure: function (response) {
                alert(response.responseText);
            },
            error: function (response) {
                alert(response.responseText);
            }
        });
    });
});