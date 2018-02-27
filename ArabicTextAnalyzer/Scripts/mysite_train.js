﻿// initial load fct
/*$(document).ready(function () {

    // bring new data into the partial view (table of posts)
    BringNewDataIntoPartialView();
});*/

function BringNewDataIntoPartialView(adminModeShowAll, partialViewType) {

    // default value for optional parameter adminModeShowAll
    adminModeShowAll = adminModeShowAll || false;

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

            // KF is used by std index with tables & klip reports
            if (typeof KF != 'undefined') {

                // klipfolio 1 : here because the klipfolio is available only after loading partial view
                KF.embed.embedKlip({
                    profile: "d3066a5aec3f32032e5493c6e1d7f6ac",
                    settings: { "width": 450, "theme": "light", "borderStyle": "round", "borderColor": "#cccccc" },
                    title: "NERs Count"
                });

                // klipfolio 2 : here because the klipfolio is available only after loading partial view
                KF.embed.embedKlip({
                    profile: "45a5705de26ee482f3b79609f9676ecc",
                    settings: { "width": 450, "theme": "light", "borderStyle": "round", "borderColor": "#cccccc" },
                    title: "NERs Type Count"
                });

                // k3
                KF.embed.embedKlip({
                    profile: "c359a92c257f3fa726356b23b4415bfa",
                    settings: { "width": 450, "theme": "light", "borderStyle": "round", "borderColor": "#cccccc" },
                    title: "Sentiment Analysis"
                });

                // k4 : part de voix
                KF.embed.embedKlip({
                    profile: "4e81c894b814346e50f7361aef991cf5",
                    settings: { "width": 450, "theme": "light", "borderStyle": "round", "borderColor": "#cccccc" },
                    title: "Parts de Voix"
                });
            }

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