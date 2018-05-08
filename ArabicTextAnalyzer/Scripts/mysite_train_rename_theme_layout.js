
var RenameTheme = false;

function JsRenameTheme() {

    // check before
    if (RenameTheme == true)
        return;

    // check on fields
    var themeNewName = $('#themeNewName').val();
    var idXtrctTheme = $('#idXtrctTheme').val();
    console.log("idXtrctTheme : " + idXtrctTheme);
    console.log("themeNewName : " + themeNewName);
    if (themeNewName.length == 0) {
        alert("this field is  required.");
        return;
    }

    // mark as clicked to avoid double processing
    RenameTheme = true;
    console.log("themeNewName 2 : " + themeNewName);
    $.ajax({
        "dataType": 'json',
        "contentType": "application/json; charset=utf-8",
        "type": 'GET',
        "url": "/Train/XtrctTheme_EditNameAjax",
        "data": {
            "themeNewName": themeNewName,
            "idXtrctTheme": idXtrctTheme,
        },
        success: function (response) {
            if (response.success) {

                $('#renamethmiscareaerror').css('display', 'none');
                $('#renamethmiscareasuccess').css('display', 'block');
                $('#renamethmiscareasuccess p').html(response.responseText);
                // alert(response.responseText);
                // alert($('#themeNewName').val());
                //
                window.location = '/Train/Index';
                // $('#themeNewName').val('');
                console.log("themeNewName 2 : " + themeNewName);
                console.log("idXtrctTheme   : " + idXtrctTheme);

            } else {

                // show misc area error msg
                $('#renamethmiscareasuccess').css('display', 'none');
                $('#renamethmiscareaerror').css('display', 'block');
                $('#renamethmiscareaerror p').html(response.responseText);
                // alert(response.responseText);
                // $('#themeNewName').val('');
            }
        },
        error: function (jqXHR, exception) {
            var msg = '';
            if (jqXHR.status === 0) {
                msg = 'Not connect.\n Verify Network.';
            } else if (jqXHR.status == 404) {
                msg = 'Requested page not found. [404]';
            } else if (jqXHR.status == 500) {
                msg = 'Internal Server Error [500].';
            } else if (exception === 'parsererror') {
                msg = 'Requested JSON parse failed.';
            } else if (exception === 'timeout') {
                msg = 'Time out error.';
            } else if (exception === 'abort') {
                msg = 'Ajax request aborted.';
            } else {
                msg = 'Uncaught Error.\n' + jqXHR.responseText;
            }
            console.log("Js Retrieve Add ThemeName - Error : " + msg);

            // show misc area error msg
            $('#addthmiscareasuccess').css('display', 'none');
            $('#addthmiscareaerror').css('display', 'block');
            $('#addthmiscareaerror p').html(msg);
        }
    });
}