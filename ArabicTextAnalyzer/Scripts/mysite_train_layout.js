var AddTheme = false;
var RenameTheme = false;

function JsAddTheme() {

    // check before
    if (AddTheme == true)
        return;

    // check on fields
    var themename = $('#themenameforadd').val();
    console.log("themename : " + themename);
    if (themename.length == 0) {
        alert("this field is required.");
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

            // reset
            AddTheme = false;

            //
            if (response.success) {

                $('#addthmiscareaerror').css('display', 'none');
                $('#addthmiscareasuccess').css('display', 'block');
                $('#addthmiscareasuccess p').html(response.responseText);

                //
                window.location = '/Train/Index';

            } else {

                // show misc area error msg
                $('#addthmiscareasuccess').css('display', 'none');
                $('#addthmiscareaerror').css('display', 'block');
                $('#addthmiscareaerror p').html(response.responseText);

            }
        },
        error: function (jqXHR, exception) {

            // reset
            AddTheme = false;

            //
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

// event on close add new theme : reset
// $('#myModalAddNewTheme').on('hidden', function () {
$('#myModalAddNewTheme').on('hidden.bs.modal', function () {
    
    // hide misc area error msg
    console.log("myModalAddNewTheme : close");
    $('#addthmiscareasuccess').css('display', 'none');
    $('#addthmiscareaerror').css('display', 'none');
})

function JsRenameTheme() {

    // check before
    if (RenameTheme == true)
        return;

    // check on fields
    var themename = $('#themeNewName').val();
    console.log("themename : " + themename);
    if (themename.length == 0) {
        alert("this field is required.");
        return;
    }
    var idXtrctThemeToRename = $('#idXtrctThemeToRename').val();

    // mark as clicked to avoid double processing
    RenameTheme = true;
    console.log("themename 2 : " + themename);
    $.ajax({
        "dataType": 'json',
        "contentType": "application/json; charset=utf-8",
        "type": 'GET',
        "url": "/Train/XtrctTheme_EditName",
        "data": { "idXtrctTheme": idXtrctThemeToRename, "themeNewName": themename },
        success: function (response) {

            // reset
            RenameTheme = false;

            //
            if (response.success) {

                $('#addrnthmiscareaerror').css('display', 'none');
                $('#addrnthmiscareasuccess').css('display', 'block');
                $('#addrnthmiscareasuccess p').html(response.responseText);

                //
                window.location = '/Train/Index';

            } else {

                // show misc area error msg
                $('#addrnthmiscareasuccess').css('display', 'none');
                $('#addrnthmiscareaerror').css('display', 'block');
                $('#addrnthmiscareaerror p').html(response.responseText);
            }
        },
        error: function (jqXHR, exception) {

            // reset
            RenameTheme = false;

            //
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
            $('#addrnthmiscareasuccess').css('display', 'none');
            $('#addrnthmiscareaerror').css('display', 'block');
            $('#addrnthmiscareaerror p').html(msg);
        }
    });
}

// event on close rename theme : reset
// $('#myModalRenameTheme').on('hidden', function () {
$('#myModalRenameTheme').on('hidden.bs.modal', function () {

    // hide misc area error msg
    console.log("myModalRenameTheme : close");
    $('#addrnthmiscareasuccess').css('display', 'none');
    $('#addrnthmiscareaerror').css('display', 'none');
})
