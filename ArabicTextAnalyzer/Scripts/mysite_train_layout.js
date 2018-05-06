var AddTheme = false;

function JsAddTheme() {

    // check before
    if (AddTheme == true)
        return;

    // check on fields
    var themename = $('#themenameforadd').val();
    console.log("themename : " + themename);
    if (themename.length == 0) {
        alert("this field is  required.");
        return;
    }

    // mark as clicked to avoid double processing
    AddTheme = true;
    console.log("themename 2 : " + themename);
    $.ajax({
        "dataType": 'json',
        "contentType": "application/json; charset=utf-8",
        "type": 'GET',
        "url": "/Train/XtrctTheme_AddNewAjax",
        "data": { "themename": themename },
        success: function (response) {
            if (response.success) {

                $('#addthmiscareaerror').css('display', 'none');
                $('#addthmiscareasuccess').css('display', 'block');
                $('#addthmiscareasuccess p').html(response.responseText);
                // alert(response.responseText);
                // alert($('#themename').val());
                //
                window.location = '/Train/Index';
                // $('#themename').val('');

            } else {

                // show misc area error msg
                $('#addthmiscareasuccess').css('display', 'none');
                $('#addthmiscareaerror').css('display', 'block');
                $('#addthmiscareaerror p').html(response.responseText);
                // alert(response.responseText);
                // $('#themename').val('');
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