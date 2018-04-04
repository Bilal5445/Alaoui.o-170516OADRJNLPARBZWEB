// initial load fct
/*$(document).ready(function () {

    // bring new data into the partial view (table of posts)
    BringNewDataIntoPartialView();
});*/

function BringNewDataIntoPartialView(adminModeShowAll, partialViewType) {

    // default value for optional parameter adminModeShowAll
    adminModeShowAll = adminModeShowAll || false;

    // default for partialViewType = 0 = all
    partialViewType = partialViewType || 0;

    // bring new data into the partial view (table of posts)
    $.ajax({
        type: 'POST',
        url: "/Train/ArabicDarijaEntryPartialView",
        "data": { "adminModeShowAll": adminModeShowAll, "partialViewType": partialViewType },
        success: (function (result) {

            // replace partial view with the returned data
            $('#partialPlaceHolder').html(result);

            // if not FB, hide the plus button
            var partialViewType_all = 0;
            var partialViewType_FBPagesOnly = 2;
            if (partialViewType != partialViewType_all && partialViewType != partialViewType_FBPagesOnly) {
                $('li.mainUL#addFBPage').hide();
            }

            // refresh load time
            refreshPlainLoadTime();

            // fct to attach working data table with search highlight keywords
            // but only if not FB
            if (partialViewType != partialViewType_FBPagesOnly)
                InitializeDataTables(adminModeShowAll);
        }),
        failure: function (response) {
            console.log(response);
        },
        error: function (response) {
            console.log(response);
        }
    });
}

// event when we switch between twingly accounts radio buttons
$("input#id").change(function () {

    console.log($(this).parent().text().split(/\ +/)[1]);

    $.ajax({
        type: 'POST',
        url: "/Train/TwinglySetup_changeActiveAccount",
        data: { "newlyActive_id_twinglyaccount_api_key": $(this).parent().text().split(/\ +/)[1] },
        failure: function (response) {
            console.log(response);
        },
        error: function (response) {
            console.log(response);
        }
    });
});

// event when we enter a new twingly account
$('#txtNewTwinglyAccount').keydown(function (e) {
    var key = e.which;
    if (key == 13)  // the enter key code
    {
        console.log($(this).val());
        $.ajax({
            type: 'POST',
            url: "/Train/TwinglySetup_AddNewActiveAccount",
            data: { "newlyActive_id_twinglyaccount_api_key": $(this).val() },
            success: function () {
                location.reload();
            },
            failure: function (response) {
                console.log(response);
            },
            error: function (response) {
                console.log(response);
            }
        });
    }
});