// datatables jquery table for social serach
var socialsearchtable;

// events variables
var btnExtractEntitiesClicked = false;
var btnExtractPostsEntitiesClicked = false;
var btnShowSocialSearchFBPostCommentsClicked = false;

// names of cols (so we can be indpendendant from index)
var columnNamesPosts = [];
$('.datatables-table-fbs-all').find('th').each(function () {
    columnNamesPosts.push($(this).text().trim());
});

// initial load fct
$(document).ready(function () {

    InitializeSocialSearchPostsDataTables();
});

// table for FB post table
function InitializeSocialSearchPostsDataTables() {

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
            { "data": "dp", "className": "arabizi-text top" },  // date post
            { "data": "pt", "className": "top" },               // post text
            { "data": "fbPageName", "className": "arabizi-text top" },
            { "data": "FormattedEntities", "className": "arabic-text top entities" },
            { "data": "tt", "className": "arabic-text top" },   // translation text
            { "data": "lc", "className": "center top" },
            { "data": "cc", "className": "center top" },
            {
                "data": function (data) {
                    var str = '';
                    str = str + '<a class="btn btn-info btn-xs small" onclick="' + "JsExtractPostsEntities(this)" + '"><span class="glyphicon glyphicon-refresh small" aria-hidden="true" title="Reprocess text"></span></a>';
                    return str;
                },
                "className": "controls center top"
            },
        ],
        "columnDefs": [{
            "defaultContent": "-",
            "targets": "_all"
        }],
        "rowCallback": function (row, data) {
            if (IsRandALCat(data.pt.charCodeAt(0))) {
                $('td:eq(' + columnNamesPosts.indexOf("Post") + ')', row).css('direction', 'rtl'); // '2' being the column 'pt' = Post
            }
            if (IsRandALCat(data.fbPageName.charCodeAt(0))) {
                $('td:eq(' + columnNamesPosts.indexOf("Page") + ')', row).css('direction', 'rtl'); // '3' being the column 'fbPageName' = Page
            }
        },
        // server side
        "processing": true,
        "serverSide": true,
        "ajax": {
            "url": "/Train/DataTablesNet_ServerSide_SocialSearch_GetList",
            // search by date & with whole word
            "data": function (d) {

                // custome search by date range
                var min = $('#min').datepicker("getDate");
                if (min !== null) {
                    var minjson = min.toJSON();
                    d.min = minjson;
                }
                var max = $('#max').datepicker("getDate");
                if (max !== null) {
                    var maxjson = max.toJSON();
                    d.max = maxjson;
                }

                // pass filtered ners to server
                var ners = $('.selectpicker').val();
                if (ners !== null && ners.length > 0) {
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
            { "data": "created_time", "className": "arabizi-text top" },
            { "data": "message", "className": "top" },
            { "data": "translated_message", "className": "top" },
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
        "rowCallback": function (row, data) {
            if (IsRandALCat(data.message.charCodeAt(0))) {
                $('td:eq(' + columnNamesComments.indexOf("Comment") + ')', row).css('direction', 'rtl');
            }
            if (data.translated_message !== null && IsRandALCat(data.translated_message.charCodeAt(0))) {
                $('td:eq(' + columnNamesComments.indexOf("Translated Comment") + ')', row).css('direction', 'rtl');
            }
        },
        // server side
        "processing": true,
        "serverSide": true,
        "ajax": "/Train/DataTablesNet_ServerSide_FB_Comments_GetList?id=" + id
    });

    // names of comments cols (filled at fly)
    var columnNamesComments = [];
    $('table[id^=socialsearchtabledetails_]').find('th').each(function () {
        columnNamesComments.push($(this).text().trim());
    });
}

function IsRandALCat(c) {
    var hasRandALCat = 0;
    if (c >= 0x5BE && c <= 0x10B7F) {
        if (c <= 0x85E) {
            if (c === 0x5BE) hasRandALCat = 1;
            else if (c === 0x5C0) hasRandALCat = 1;
            else if (c === 0x5C3) hasRandALCat = 1;
            else if (c === 0x5C6) hasRandALCat = 1;
            else if (0x5D0 <= c && c <= 0x5EA) hasRandALCat = 1;
            else if (0x5F0 <= c && c <= 0x5F4) hasRandALCat = 1;
            else if (c === 0x608) hasRandALCat = 1;
            else if (c === 0x60B) hasRandALCat = 1;
            else if (c === 0x60D) hasRandALCat = 1;
            else if (c === 0x61B) hasRandALCat = 1;
            else if (0x61E <= c && c <= 0x64A) hasRandALCat = 1;
            else if (0x66D <= c && c <= 0x66F) hasRandALCat = 1;
            else if (0x671 <= c && c <= 0x6D5) hasRandALCat = 1;
            else if (0x6E5 <= c && c <= 0x6E6) hasRandALCat = 1;
            else if (0x6EE <= c && c <= 0x6EF) hasRandALCat = 1;
            else if (0x6FA <= c && c <= 0x70D) hasRandALCat = 1;
            else if (c === 0x710) hasRandALCat = 1;
            else if (0x712 <= c && c <= 0x72F) hasRandALCat = 1;
            else if (0x74D <= c && c <= 0x7A5) hasRandALCat = 1;
            else if (c === 0x7B1) hasRandALCat = 1;
            else if (0x7C0 <= c && c <= 0x7EA) hasRandALCat = 1;
            else if (0x7F4 <= c && c <= 0x7F5) hasRandALCat = 1;
            else if (c === 0x7FA) hasRandALCat = 1;
            else if (0x800 <= c && c <= 0x815) hasRandALCat = 1;
            else if (c === 0x81A) hasRandALCat = 1;
            else if (c === 0x824) hasRandALCat = 1;
            else if (c === 0x828) hasRandALCat = 1;
            else if (0x830 <= c && c <= 0x83E) hasRandALCat = 1;
            else if (0x840 <= c && c <= 0x858) hasRandALCat = 1;
            else if (c === 0x85E) hasRandALCat = 1;
        }
        else if (c === 0x200F) hasRandALCat = 1;
        else if (c >= 0xFB1D) {
            if (c === 0xFB1D) hasRandALCat = 1;
            else if (0xFB1F <= c && c <= 0xFB28) hasRandALCat = 1;
            else if (0xFB2A <= c && c <= 0xFB36) hasRandALCat = 1;
            else if (0xFB38 <= c && c <= 0xFB3C) hasRandALCat = 1;
            else if (c === 0xFB3E) hasRandALCat = 1;
            else if (0xFB40 <= c && c <= 0xFB41) hasRandALCat = 1;
            else if (0xFB43 <= c && c <= 0xFB44) hasRandALCat = 1;
            else if (0xFB46 <= c && c <= 0xFBC1) hasRandALCat = 1;
            else if (0xFBD3 <= c && c <= 0xFD3D) hasRandALCat = 1;
            else if (0xFD50 <= c && c <= 0xFD8F) hasRandALCat = 1;
            else if (0xFD92 <= c && c <= 0xFDC7) hasRandALCat = 1;
            else if (0xFDF0 <= c && c <= 0xFDFC) hasRandALCat = 1;
            else if (0xFE70 <= c && c <= 0xFE74) hasRandALCat = 1;
            else if (0xFE76 <= c && c <= 0xFEFC) hasRandALCat = 1;
            else if (0x10800 <= c && c <= 0x10805) hasRandALCat = 1;
            else if (c === 0x10808) hasRandALCat = 1;
            else if (0x1080A <= c && c <= 0x10835) hasRandALCat = 1;
            else if (0x10837 <= c && c <= 0x10838) hasRandALCat = 1;
            else if (c === 0x1083C) hasRandALCat = 1;
            else if (0x1083F <= c && c <= 0x10855) hasRandALCat = 1;
            else if (0x10857 <= c && c <= 0x1085F) hasRandALCat = 1;
            else if (0x10900 <= c && c <= 0x1091B) hasRandALCat = 1;
            else if (0x10920 <= c && c <= 0x10939) hasRandALCat = 1;
            else if (c === 0x1093F) hasRandALCat = 1;
            else if (c === 0x10A00) hasRandALCat = 1;
            else if (0x10A10 <= c && c <= 0x10A13) hasRandALCat = 1;
            else if (0x10A15 <= c && c <= 0x10A17) hasRandALCat = 1;
            else if (0x10A19 <= c && c <= 0x10A33) hasRandALCat = 1;
            else if (0x10A40 <= c && c <= 0x10A47) hasRandALCat = 1;
            else if (0x10A50 <= c && c <= 0x10A58) hasRandALCat = 1;
            else if (0x10A60 <= c && c <= 0x10A7F) hasRandALCat = 1;
            else if (0x10B00 <= c && c <= 0x10B35) hasRandALCat = 1;
            else if (0x10B40 <= c && c <= 0x10B55) hasRandALCat = 1;
            else if (0x10B58 <= c && c <= 0x10B72) hasRandALCat = 1;
            else if (0x10B78 <= c && c <= 0x10B7F) hasRandALCat = 1;
        }
    }

    return hasRandALCat === 1 ? true : false;
}

function JsExtractEntities() {

    // check before
    if (btnExtractEntitiesClicked === true)
        return;

    // mark as clicked to avoid double processing
    btnExtractEntitiesClicked = true;

    //
    // var commentsIds = "'2195511010686419_2195728087331378','2195511010686419_2195718037332383'";
    // var commentsIds = "'2195511010686419_2195713473999506'";
    var postsIds = "'1425749764329218_2195511010686419'"

    $.ajax({
        "dataType": 'json',
        "type": "GET",
        /*"url": "/Train/Train_FB_Comments_woBingGoogleRosette",
        "data": {
            "commentsIds": commentsIds
        },*/
        "url": "/Train/Train_FB_Posts_woBingGoogleRosette",
        "data": {
            "postsIds": postsIds
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

function JsExtractPostsEntities(thisa) {

    // a => parent td => next (hidden) td => td content = post id
    var parentTd = $(thisa).parent();
    var parentTr = parentTd.parent();
    var fullpostId = parentTr.children().eq(columnNamesPosts.indexOf("PostId")).html();

    // check before
    if (btnExtractPostsEntitiesClicked === true)
        return;

    // mark as clicked to avoid double processing
    btnExtractPostsEntitiesClicked = true;

    //
    // var postsIds = "'1425749764329218_2195511010686419'"
    var postsIds = "'" + fullpostId + "'";

    $.ajax({
        "dataType": 'json',
        "type": "GET",
        "url": "/Train/Train_FB_Posts_woBingGoogleRosette",
        "data": {
            "postsIds": postsIds
        },
        "success": function (msg) {
            console.log(msg);

            // reset to not clicked
            btnExtractPostsEntitiesClicked = false;

            if (msg.status) {
                // alert("Success");
                // reload
                window.location = '/Train/IndexSocialSearch';
            } else {
                alert(msg.message);
            }
        },
        "error": function () {

            // reset to not clicked
            btnExtractPostsEntitiesClicked = false;

            //
            alert("Error");
        }
    });
}

// method for get the comments table under the row of each post table
function JsShowSocialSearchFBPostComments(thisimg) {

    // image => parent td => next (hidden) td => td content = post id
    var parentTd = $(thisimg).parent();

    // image => parent td => parent tr
    var parentTr = parentTd.parent();

    //
    var fullpostId = parentTr.children().eq(columnNamesPosts.indexOf("PostId")).html();

    // get dataTables.net row for the tr
    var dataTablesNetRow = socialsearchtable.row(parentTr);

    // full post id retrived from FB contains 2 parts : first being the fb page id, and second being the specific id of this post
    var shortPostId = fullpostId.split('_')[1];

    // check before
    if (btnShowSocialSearchFBPostCommentsClicked === true)
        return;

    // mark as clicked to avoid double processing
    btnShowSocialSearchFBPostCommentsClicked = true;

    //
    ToggleSocialSearchFBPostCommentsTable(dataTablesNetRow, parentTr, thisimg, shortPostId);

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
                    <th class="center top col50px"></th>
                    <th class="center top col50px">ID</th>
                    <th class="center top col95px">Created Time</th>
                    <th class="center top col50prc">Comment</th>
                    <th class="center top col50prc">Translated Comment</th>
                    <th class="center top col75px"><i class="fa fa-cogs" aria-hidden="true"></i></th>
                </tr>
            </thead>
        </table>
    `;

    return html;
}
