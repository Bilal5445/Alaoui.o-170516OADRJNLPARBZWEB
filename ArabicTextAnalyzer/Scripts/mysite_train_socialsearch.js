var socialsearchtable;

// initial load fct
$(document).ready(function () {

    InitializeSocialSearchDataTables();
});

// table for FB post table
function InitializeSocialSearchDataTables() {

    // Initialize DataTables
    socialsearchtable = $('.datatables-table-fbs-all').DataTable({
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
                "defaultContent": '<img src="http://i.imgur.com/SD7Dz.png" class="imagetag" onclick="' + "JsShowSocialSearchFBPostComments(this)" + '">'
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

                // pass filtered ners to server
                var ners = $('.selectpicker').val();
                if (ners != null && ners.length > 0) {
                    d.ners = JSON.stringify(ners);
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
    $("#min").datepicker({ onSelect: function () { socialsearchtable.draw(); }, changeMonth: true, changeYear: true });
    $("#max").datepicker({ onSelect: function () { socialsearchtable.draw(); }, changeMonth: true, changeYear: true });
    $('#min, #max').change(function () {
        socialsearchtable.draw();
    });

    // search by NER select option trigger
    $('.selectpicker').change(function () {
        poststable.draw();
    });

    // custom search whole word : to trigger change when we check the cehckbox whole word
    $('div.toolbar > input[type="checkbox"]').change(function () {
        socialsearchtable.draw();
    });
}

// method used by ShowFBComments above to get table actual comments
function InitializeSocialSearchCommentsForPostDataTables(id) {

    $('#socialsearchtabledetails_' + id).DataTable({
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
        //
        "columns": [
            {
                "data": function (data) {
                    var str = '';
                    str = str + '<input type="checkbox" class="cbxComment_' + id + '" value="' + data.Id + '"/>';
                    return str;
                },
                "className": "details-control center top"
            },
            { "data": "Id", "className": "hide_column" },
            { "data": "message", "className": "arabizi-text top" },
            { "data": "translated_message", "className": "arabizi-text top" },
            { "data": "created_time", "className": "arabizi-text top" },
            {
                "data": function (data) {
                    var str = '';
                    str = str + '<a class="btn btn-warning btn-xs disabled" onclick="' + "TranslateComment(this)" + '">Translate</a>';
                    return str;
                }
            },
        ],
        "columnDefs": [{
            "defaultContent": "-",
            "targets": "_all"
        }],
        // server side
        "processing": true,
        "serverSide": true,
        "ajax": "/Train/DataTablesNet_ServerSide_FB_Comments_GetList?id=" + id
    });
}

// events variables
var btnExtractEntitiesClicked = false;
var btnShowSocialSearchFBPostCommentsClicked = false;

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

// method for get the comments table under the row of each post table
function JsShowSocialSearchFBPostComments(thisimg) {

    // image => parent td => next (hidden) td => td content = post id
    var fullpostId = $(thisimg).parent().next().html();

    // image => parent td => parent tr
    var tr = $(thisimg).parent().parent();

    // get dataTables.net row for the tr
    var dataTablesNetRow = socialsearchtable.row(tr);

    //
    var shortPostId = fullpostId.split('_')[1];

    // check before
    if (btnShowSocialSearchFBPostCommentsClicked == true)
        return;

    // mark as clicked to avoid double processing
    btnShowSocialSearchFBPostCommentsClicked = true;

    //
    ToggleSocialSearchFBPostCommentsTable(dataTablesNetRow, tr, thisimg, shortPostId);

    //
    btnShowSocialSearchFBPostCommentsClicked = false;
}

function ToggleSocialSearchFBPostCommentsTable(row, tr, img, postId) {

    if (row.child.isShown()) {

        // This row is already open - close it
        row.child.hide();
        tr.removeClass('shown');

        // show icon as a green +
        $(img).prop('src', "http://i.imgur.com/SD7Dz.png")

    } else {

        // This row is closed - show it

        // show icon as a red -
        $(img).prop('src', "http://i.imgur.com/d4ICC.png")

        if (!$('#socialsearchtabledetails_' + postId).length) {

            // create on the fly a sub table (html table + theader) for the comments associated with a post in a page
            var tablecontent = SocialSearchCommentTable(postId);
            row.child(tablecontent).show();

            // get using server size ajax to datatables.net the actual comments
            InitializeSocialSearchCommentsForPostDataTables(postId);

            // add a global bulk translate button for comments
            $('#socialsearchtabledetails_' + postId + '_length').append('<a class="btn btn-info btn-xs disabled" style="margin-left:5px" onclick="GetTranslateComment(' + postId + ')">Bulk Translate</a>')

            // add a global bulk check all button for comments
            var commentshtml = `
                <a class ="btn btn-info btn-xs" style="margin-left:5px" onclick="CheckUnCheckAllComments(${postId})">Check/Uncheck All</a>
            `;
            $('#socialsearchtabledetails_' + postId + '_length').append(commentshtml);

        } else {
            row.child($('#socialsearchtabledetails_' + postId).html()).show();
        }

        //
        tr.addClass('shown');
    }
}

// method used by ShowFBComments above to get table up to theader
function SocialSearchCommentTable(postId) {

    // build the html table up to the header (the content is brought vis server side controller)
    var html = `
        <table id="socialsearchtabledetails_${postId}" class ="posts table table-striped table-hover table-bordered">
            <thead class="header">
                <tr>
                    <th class ="center top col50px"></th>
                    <th class="center top col50px">ID</th>
                    <th class="center top col50prc">Comment</th>
                    <th class ="center top col50prc">Translated Comment</th>
                    <th class="center top col95px">Created Time</th>
                    <th class="center top col75px">Action</th>
                </tr>
            </thead>
        </table>
    `;

    return html;
}
