$(function () {
    $("#analyzeText").click(function () {
        $.ajax({
            type: "POST",
            url: "/Home/AjaxMethod",
            data: '{name: "' + $("#txtName").val() + '" }',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                alert("Hello: " + response.Name + " .\nCurrent Date and Time: " + response.DateTime);
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