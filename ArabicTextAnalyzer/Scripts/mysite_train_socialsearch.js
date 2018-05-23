// initial load fct
$(document).ready(function () {

    InitializeSocialSearchDataTables();
});

// table for FB post table
function InitializeSocialSearchDataTables() {

    // Initialize DataTables
    var table = $('.datatables-table-fbs-all').DataTable({
        /*"language": {
            "url": "https://cdn.datatables.net/plug-ins/1.10.16/i18n/French.json",
            buttons: {
                copyTitle: 'Ajouté au presse-papiers',
                copySuccess: {
                    _: '%d lignes copiées',
                    1: '1 ligne copiée'
                }
            }
        },*/
        // Enable mark.js search term highlighting
        mark: {
            element: 'span',
            className: 'highlight'
        },
        aaSorting: [],   // just use the sorting from the server
        "pageLength": 100,
        lengthMenu: [
            [10, 25, 50, 100, 500, 1000, -1],
            ['10', '25', '50', '100', '500', '1000', 'all']
        ],
        // dom just to display the design of the fields : search , page , ...
        // dom: "<'row'<'col-sm-5'B><'col-sm-3'l><'col-sm-4'f>>" +
        dom: "<'row'<'col-sm-3'B><'col-sm-3'l><'col-sm-3'f><'toolbar'>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-5'i><'col-sm-7'p>>",
        buttons: [
            'copyHtml5', 'excel', 'csv'
        ],
        //
        "columns": [
            {
                "data": null, "className": "details-control center top",
                "defaultContent": '<img src="http://i.imgur.com/SD7Dz.png" class="imagetag" onclick="' + "ShowFBComments(this)" + '">'
            },
            { "data": "id", "className": "hide_column" },
            /*{ "data": "fk_i", "className": "arabizi-text top collapsed" },*/
            { "data": "pt", "className": "arabizi-text top" },
            { "data": "fbPageName", "className": "arabizi-text top" },
            { "data": "tt", "className": "arabic-text top" },
            { "data": "lc", "className": "center top" },
            { "data": "cc", "className": "center top" },
            { "data": "dp", "className": "arabizi-text top" },
            {
                "data": function (data) {
                    var str = '';
                    str = str + '<a class="btn btn-warning btn-xs disabled" onclick="' + "JsTranslateFBPost(this)" + '">Traduire</a>';
                    return str;
                },
                "className": "controls center top"
            },
        ],
        "columnDefs": [{
            "defaultContent": "-",
            "targets": "_all"
        }],
        // server side
        "processing": true,
        "serverSide": true,
        "ajax": {
            "url": "/Train/DataTablesNet_ServerSide_SocialSearch_GetList",
            // search by date & with whole word
            "data": function (d) {
                // custome search by date range
                var min = $('#min').datepicker("getDate");
                if (min != null) {
                    var minjson = min.toJSON();
                    d.min = minjson;
                }
                var max = $('#max').datepicker("getDate");
                if (max != null) {
                    var maxjson = max.toJSON();
                    d.max = maxjson;
                }
                // custom search whole word
                if ($('div.toolbar > input[type="checkbox"]').is(":checked")) {
                    d.wholeWord = true;
                } else {
                    d.wholeWord = false;
                }
            }
        }
    });

    // add checkbox for wholeword search
    $("div.toolbar").html('<input type="checkbox" name="vehicle" value="Bike"> par mot complet');

    // Search by date & Event listener to the two range filtering inputs to redraw on input
    $("#min").datepicker({ onSelect: function () { table.draw(); }, changeMonth: true, changeYear: true });
    $("#max").datepicker({ onSelect: function () { table.draw(); }, changeMonth: true, changeYear: true });
    $('#min, #max').change(function () {
        table.draw();
    });

    // custom search whole word : to trigger change when we check the cehckbox whole word
    $('div.toolbar > input[type="checkbox"]').change(function () {
        table.draw();
    });
}

// events variables
var btnExtractEntitiesClicked = false;

function JsExtractEntities() {

    // check before
    if (btnExtractEntitiesClicked == true)
        return;

    // mark as clicked to avoid double processing
    btnExtractEntitiesClicked = true;

    //
    // var commentsIds = "'2195511010686419_2195728087331378','2195511010686419_2195718037332383'";
    var commentsIds = "'2195511010686419_2195713473999506'";

    $.ajax({
        "dataType": 'json',
        "type": "GET",
        "url": "/Train/Train_FB_Comments_woBingGoogleRosette",
        "data": {
            "commentsIds": commentsIds
        },
        "success": function (msg) {
            console.log(msg);

            // reset to not clicked
            btnExtractEntitiesClicked = false;

            if (msg.status) {
                alert("Success");
            } else {
                alert(msg.message);
            }
        },
        "error": function () {
            
            // reset to not clicked
            btnExtractEntitiesClicked = false;

            //
            alert("Error");
        }
    });
}
