$(document).ready(function () {

    // bring new data into the partial view (table of posts)
    BringNewDataIntoPartialView();
});

function BringNewDataIntoPartialView(adminModeShowAll) {

    // default value for optional parameter adminModeShowAll
    adminModeShowAll = adminModeShowAll || false;

    // bring new data into the partial view (table of posts)
    $.ajax({
        type: 'POST',
        url: "/Train/ArabicDarijaEntryPartialView",
        "data": { "adminModeShowAll": adminModeShowAll },
        success: (function (result) {

            $('#partialPlaceHolder').html(result);

            // klipfolio : here because the klipfolio is available only after loading partial view
            KF.embed.embedKlip({
                profile: "34bfa2079bd0a2236c048c2cb631f938",
                settings: { "width": 606, "theme": "light", "borderStyle": "round", "borderColor": "#cccccc" },
                title: "NERs Count"
            });

            // refresh load time
            refreshPlainLoadTime();

            // fct to attach table with search highlight keywords
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
// $("#myButtons :input").change(function () {
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
            success: function() {
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