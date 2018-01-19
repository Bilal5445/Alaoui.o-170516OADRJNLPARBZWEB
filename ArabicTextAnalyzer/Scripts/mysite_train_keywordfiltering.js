var poststable;
var selectedArabiziIds = [];    // array of arabizi entries ids that have been selected by the user each he clicks on a row
var ViewInfluencerIsClicked = false;
var vars = {};
var TranslateContentIsClicked = false;
var GetCommentsIsClicked = false;
var Comments = "";
var GetTranslateCommentIsClicked = false;
var TranslateCommentIsClicked = false;
var AddInfluencerIsClicked = false;
var RetrieveFBPostIsClicked = false;

function InitializeDataTables(adminModeShowAll) {

    // default value for optional parameter adminModeShowAll
    adminModeShowAll = adminModeShowAll || false;

    //
    $(function () {

        // Initialize DataTables
        poststable = $('.datatables-table').DataTable({
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
            dom: "<'row'<'col-sm-6'B><'col-sm-3'l><'col-sm-3'f>>" +
                "<'row'<'col-sm-12'tr>>" +
                "<'row'<'col-sm-5'i><'col-sm-7'p>>",
            buttons: [
                'copyHtml5', 'excel', 'csv',
                'selectAll',
                /*{
                    text: 'Select All',
                    action: function () {
                        this.rows().deselect();
                        // this.rows().select();
                        this.rows().click();
                    }
                },*/
                'selectNone'
            ],
            //
            "columns": [
                    { "data": "PositionHash", "className": "center top" },
                    { "data": "FormattedArabiziEntryDate", "className": "arabizi-text top" },
                    { "data": "ArabiziText", "className": "arabizi-text top" },
                    { "data": "FormattedArabicDarijaText", "className": "arabic-text top" },
                    { "data": "FormattedEntitiesTypes", "className": "arabic-text top entitiestype" },
                    { "data": "FormattedEntities", "className": "arabic-text top entities" },
                    { "data": "FormattedRemoveAndApplyTagCol", "className": "controls center top" } // class 'controls' just there to identify the td to prevent click on 'remove' (or any other button in this column) to be considered as selection. MC080118 plus now also for keeping buttons in same line
            ],
            "columnDefs": [{
                "defaultContent": "-",
                "targets": "_all"
            }],
            // server side
            "processing": true,
            "serverSide": true,
            "ajax": {
                "url": "/Train/DataTablesNet_ServerSide_GetList",
                "type": "POST",
                "data": { "adminModeShowAll": adminModeShowAll }
            },
            select: true    // warning : before this, previous multi-select was just clicking on many rows, 
            // now muli-select needs ctrl+click (separate multiclick) or shift + click (range multiclick)
        });

        // user event click on select row controls column
        poststable.on('user-select', function (e, dt, type, cell, originalEvent) {
            if ($(cell.node()).attr('class').includes("controls")) {
                e.preventDefault();
            }
        });

        // event select
        poststable.on('select', function (e, dt, type, indexes) {

            if (type === 'row') {

                // we select
                dt.rows(indexes).every(function (rowIdx, tableLoop, rowLoop) {

                    // find the guid of arabiz entry from the href in delete button
                    var thisTr = this.node();
                    var controlsTd = $(thisTr).find("td:eq(6)");
                    var deleteButton = controlsTd.find("> a").eq(0);
                    var hrefInnerId = deleteButton.attr("href").substring("/Train/Train_DeleteEntry/?arabiziWordGuid=".length);

                    // add it to global array if not already there (shift select add the first row even if already there)
                    if (selectedArabiziIds.indexOf(hrefInnerId) === -1)
                        selectedArabiziIds.push(hrefInnerId);

                    // save old id in backup in delete button
                    deleteButton.attr('data-backhref', hrefInnerId);
                });

                // loop over selected to concatenate the arabizi entries ids but only if more than one
                var thisTbody = dt.table().body();
                var selectedControlsTds = $(thisTbody).find('tr.selected td:last-child');
                BuildMulipleIdsForDeleteAndRefreshButton(selectedControlsTds);
            }
        }).on('deselect', function (e, dt, type, indexes) {

            if (type === 'row') {

                // we deselect
                dt.rows(indexes).every(function (rowIdx, tableLoop, rowLoop) {

                    // find the guid of arabiz entry from the backup href in delete button
                    var thisTr = this.node();
                    var controlsTd = $(thisTr).find("td:eq(6)");
                    var deleteButton = controlsTd.find("> a").eq(0);
                    var hrefBackInnerId = deleteButton.attr("data-backhref");

                    // drop it from global (we know it is there)
                    var index = selectedArabiziIds.indexOf(hrefBackInnerId);
                    selectedArabiziIds.splice(index, 1);

                    // set new value href (from backup) in delete button
                    var newhref = "/Train/Train_DeleteEntry/?arabiziWordGuid=" + hrefBackInnerId;
                    deleteButton.attr("href", newhref);

                    // same for refresh button (2nd button)
                    var refreshButton = controlsTd.find("> a").eq(1);
                    var newrefreshhref = "/Train/Train_RefreshEntry/?arabiziWordGuid=" + hrefBackInnerId;
                    refreshButton.attr("href", newrefreshhref);

                    // remove backup
                    deleteButton.removeAttr('data-backhref');
                });

                // loop over selected to concatenate the arabizi entries ids but only if more than one
                var thisTbody = dt.table().body();
                var selectedControlsTds = $(thisTbody).find('tr.selected td:last-child');
                BuildMulipleIdsForDeleteAndRefreshButton(selectedControlsTds);
            }
        });

        // capturing the click on a-href because href can not take too many args (when multi-deletes or multu-refresh)
        /*$(".plyshr").click(function() {
            var id = $(this).attr('id');
            var dataString = 'id='+ id ;
            var parent = $(this);
            //alert (data);
            $.ajax({
                type: "POST",
                url: "playlist.php",
                success: function(html)
                    data:  dataString,
                    cache: false,
                    success: function(html)
                    }).done(function( msg ) {
                        parent.html(html);
                    });
                });*/
    });
}

function BuildMulipleIdsForDeleteAndRefreshButton(selectedControlsTds) {
    // loop over selected to concatenate the arabizi entries ids but only if more than one
    if (selectedControlsTds.length > 1) {
        selectedControlsTds.each(function (index) {
            // new value href
            var arabiziWordGuids = selectedArabiziIds.join();
            var newhref = "/Train/Train_DeleteEntries/?arabiziWordGuids=" + arabiziWordGuids;

            // set new value in delete button
            var deleteButton = $(this).find("> a").eq(0);
            deleteButton.attr("href", newhref);

            // same for refresh button (2nd button)
            var refreshButton = $(this).find("> a").eq(1);
            var newrefreshhref = "/Train/Train_RefreshEntries/?arabiziWordGuids=" + arabiziWordGuids;
            refreshButton.attr("href", newrefreshhref);
        });
    } else if (selectedControlsTds.length == 1) {
        // new value href
        var arabiziWordGuid = selectedArabiziIds.join();
        var newhref = "/Train/Train_DeleteEntry/?arabiziWordGuid=" + arabiziWordGuid;

        // set new value in delete button
        var deleteButton = selectedControlsTds.find("> a").eq(0);
        deleteButton.attr("href", newhref);

        // same for refresh button (2nd button)
        var refreshButton = selectedControlsTds.find("> a").eq(1);
        var newrefreshhref = "/Train/Train_RefreshEntry/?arabiziWordGuid=" + arabiziWordGuid;
        refreshButton.attr("href", newrefreshhref);
    }
}

// Table For FB For Particular influencer
function LoadFacebookPosts(fluencerid) {
    if (ViewInfluencerIsClicked == false) {
        ViewInfluencerIsClicked = true;
        var $checkedBoxes = $('.table_' + fluencerid + ' tbody tr');
        if ($checkedBoxes.length == 0) {
            InitializeFBPostsDataTables(fluencerid)
        }
        fnCallback(fluencerid)
    }
}

// table for FB post table
function InitializeFBPostsDataTables(fluencerid) {
    $(function () {

        // Initialize DataTables
        vars[fluencerid] = $('.table_' + fluencerid).DataTable({
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
            dom: "<'row'<'col-sm-3'B><'col-sm-3'l><'col-sm-6'f>>" +
                "<'row'<'col-sm-12'tr>>" +
                "<'row'<'col-sm-5'i><'col-sm-7'p>>",
            buttons: [
                'copyHtml5', 'excel', 'csv'
            ],
            //
            "columns": [
                { "data": null, "className": "details-control", "defaultContent": '<img src="http://i.imgur.com/SD7Dz.png" class="imagetag" onclick="' + "GetComments(this)" + '">' },
                    { "data": "id", "className": "center top" },
                    /*{ "data": "fk_i", "className": "arabizi-text top collapsed" },*/
                    { "data": "pt", "className": "arabizi-text top" },
                    { "data": "tt", "className": "arabizi-text top" },
                    { "data": "lc", "className": "arabic-text top" },
                    { "data": "cc", "className": "arabic-text top entitiestype" },
                    { "data": "dp", "className": "arabic-text top entities" },
                    {
                        "data": function (data) {
                            var str = '';
                            str = str + '<a class="btn btn-warning btn-xs" onclick="' + "TranslateContent(this)" + '">Translate</a>';
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
            "ajax": "/Train/DataTablesNet_ServerSide_FB_Posts_GetList?fluencerid=" + fluencerid
        });
    });
}

function fnCallback(id) {
    $('.table_' + id).show();
    if (id.length > 0) {
        $('.tab-pane').each(function () {
            $(this).removeClass('active');
        });
        $('#second_' + id).addClass('active')
        $('.mainUL').each(function () {
            $(this).removeClass('active');
        })
        $('#' + id).addClass('active');
    }
    ViewInfluencerIsClicked = false
}

// method for translate the fb post 
function TranslateContent(obj) {

    // check before
    if (TranslateContentIsClicked == true)
        return;

    // mark as clicked to avoid double processing
    TranslateContentIsClicked = true;

    //
    var tds = $(obj).parents("tr").find("td");
    var id = ($(tds[1])).html().toString();
    var postText = $(tds[2]).html().trim().replace('-', '');
    var translatedTextTd = tds[3];
    var translatedText = $(translatedTextTd).html().trim().replace('-', '');

    //
    /*if (translatedText.length != 0) {
        alert("This post is already translated");
        TranslateContentIsClicked = false;
        return;
    }*/

    if (postText.length <= 0) {
        alert("There is no post for translate");
        TranslateContentIsClicked = false;
        return;
    }

    //
    $.ajax({
        "dataType": 'json',
        "type": "GET",
        "url": "/Train/TranslateFbPost",
        "data": {
            "content": postText,
            // "id": id
        },
        "success": function (msg) {

            // reset to not clicked
            TranslateContentIsClicked = false;

            if (msg.status) {

                if (translatedText.length == 0) {
                    // if succesfully translated, remplace translatedText column by result text
                    $(translatedTextTd).html(msg.recordsFiltered)
                }
            } else {
                alert("Error " + msg.message);
            }
        },
        "error": function () {
            TranslateContentIsClicked = false;
            alert("Error");
        }
    });
}

// method for get the comments table under the row of each post table
function GetComments(obj) {

    // check before
    if (GetCommentsIsClicked == true)
        return;

    // mark as clicked to avoid double processing
    GetCommentsIsClicked = true;

    //
    var tr = $(obj).closest('tr');
    var tds = $(obj).parents("tr").find("td");
    var idCol = ($(tds[1])).html().toString().split('_');
    var id = idCol[1];
    var influenceridFromIdCol = idCol[0];
    // var influencerid = ($(tds[2])).html().toString();
    var influencerid = influenceridFromIdCol;
    var table = vars[influencerid];
    var row = table.row(tr);

    //
    if (row.child.isShown()) {

        // This row is already open - close it

        $(obj).prop('src', "http://i.imgur.com/SD7Dz.png")
        row.child.hide();
        tr.removeClass('shown');
        GetCommentsIsClicked = false;

    } else {

        // This row is closed - show it

        $(obj).prop('src', "http://i.imgur.com/d4ICC.png")

        if (!$('#tabledetails_' + id).length) {

            // create on the fly a sub table (html table + theader) for the comments associated with a post in a page
            var tablecontent = CommentTable(id);
            row.child(tablecontent).show();

            // get using server size ajax to datatables.net the actual comments
            InitializeFBCommentsForPostDataTables(id);

            // add a global bulk translate button
            // $('#tabledetails_' + id + '_length').append('<a class="btn btn-info" style="margin-left:5%" onclick="GetTranslateComment(' + id + ')">Bulk Translate</a><h3>Comments</h3>')
            $('#tabledetails_' + id + '_length').append('<a class="btn btn-info" style="margin-left:5%" onclick="GetTranslateComment(' + id + ')">Bulk Translate</a>')

            // add a global bulk check all button
            $('#tabledetails_' + id + '_length').append('<a class="btn btn-info" style="margin-left:5%" onclick="CheckUnCheckAllComments(' + id + ')">Check/Uncheck All</a><h3>Comments</h3>')

            //
            GetCommentsIsClicked = false;

        } else {

            row.child($('#tabledetails_' + id).html()).show();

            //
            GetCommentsIsClicked = false;
        }

        //
        tr.addClass('shown');
    }
}

// method used by GetComments above to get table up to theader
function CommentTable(id) {

    // build the html table up to the header (the content is brought vis server side controller)
    var html = '<table id="tabledetails_' + id + '" class="table table-striped table-hover table-bordered"><thead class="header"><tr><th></th><th class="center top col50px">ID</th><th class="center top col50prc">Message</th><th class="center top col50prc">Translated Message</th><th class="center top col130px">Created Time</th><th class="center top col75px">Action</th></tr></thead></table>'

    return html;
}

// method used by GetComments above to get table actual comments
function InitializeFBCommentsForPostDataTables(id) {

    $('#tabledetails_' + id).DataTable({
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
        /*
        // dom just to display the design of the fields : search , page , ...
        dom: "<'row'<'col-sm-3'B><'col-sm-3'l><'col-sm-6'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-5'i><'col-sm-7'p>>",
        buttons: [
        'selectAll',
        'selectNone'
        ],
        language: {
            buttons: {
                selectAll: "Select all items",
                selectNone: "Select none"
            }
        },*/
        //
        "columns": [
            {
                "data": function (data) {
                    var str = '';
                    str = str + '<input type="checkbox" class="cbxComment_' + id + '" value="' + data.Id + '"/>';
                    return str;
                }
            },
            { "data": "Id", "className": "center top" },
            { "data": "message", "className": "arabizi-text top" },
            { "data": "translated_message", "className": "arabizi-text top" },
            { "data": "created_time", "className": "arabizi-text top" },
            {
                "data": function (data) {
                    var str = '';
                    str = str + '<a class="btn btn-warning btn-xs" onclick="' + "TranslateComment(this)" + '">Translate</a>';
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

// method for translate the comments when clicking on bulk translate of all comments under a post
function GetTranslateComment(id) {

    // check before
    if (GetTranslateCommentIsClicked == true)
        return;

    // mark as clicked to avoid double processing
    GetTranslateCommentIsClicked = true;

    //
    var translatedCommentId = '';

    // loop on all comments row and for checked ones save the ids
    $('.cbxComment_' + id).each(function () {
        if ($(this).is(':checked')) {
            if (translatedCommentId.length > 0) {
                translatedCommentId = translatedCommentId + ",'" + $(this).val() + "'";
            } else {
                translatedCommentId = "'" + $(this).val() + "'"
            }
        }
    });

    // bulk translate comments
    if (translatedCommentId.length > 0) {
        postOnCommentsTranslate(translatedCommentId, id)
    } else {
        alert("Error: Please check at least one cheackbox.")
        GetTranslateCommentIsClicked = false;
    }
}

// method for check/uncheck all
function CheckUnCheckAllComments(id) {

    // loop on all comments row and check (or uncheck) each one
    var firstIsChecked = false;
    $('.cbxComment_' + id).each(function (index, value) {
        if (index == 0 && $(this).is(':checked'))
            firstIsChecked = true;

        if (firstIsChecked == true)
            $(this).prop('checked', false);
        else
            $(this).prop('checked', true);
    });
}

// method for translate the comments when clicking on translate button of comment
function TranslateComment(obj) {
    if (TranslateCommentIsClicked == false) {

        //
        TranslateCommentIsClicked = true;

        //
        var tds = $(obj).parent().parent().find("td");
        var mainId = ($(tds[1])).html().toString();
        var id = mainId.split('_')[0];
        var translatedCommentId = "'" + mainId + "'";
        var commentText = $(tds[2]).html().trim();
        var translatedComment = $(tds[3]).html().trim().replace('-', '');

        //
        /*if (translatedComment.length != 0) {
            alert("The comment is already translated");
            TranslateCommentIsClicked = false;
            return;
        }*/

        //

        if (commentText.length <= 0) {
            alert("There is no comment text for translate.")
            TranslateCommentIsClicked = false;
            return;
        }

        //
        postOnCommentsTranslate(translatedCommentId, id)
    }
}

function postOnCommentsTranslate(translatedCommentId, id) {

    $.ajax({
        "dataType": 'json',
        "type": "GET",
        "url": "/Train/TranslateFbComments",
        "data": {
            "ids": translatedCommentId
        },
        "success": function (msg) {
            console.log(msg);

            // reset to not clicked
            GetTranslateCommentIsClicked = false;
            TranslateCommentIsClicked = false;

            if (msg.status) {
                ResetDataTableComments(id)
            } else {
                alert("Error " + msg.message);
            }
        },
        "error": function () {
            GetTranslateCommentIsClicked = false;
            TranslateCommentIsClicked = false;
            alert("Error");
        }
    });
}

// Js for add  influencer
function AddInfluencer() {

    // check before
    if (AddInfluencerIsClicked == true)
        return;

    // check on fields
    var urlname = $('#txtUrlName').val();
    var pro_or_anti = $('#ddlPro_or_anti').val();
    if (urlname.length == 0 || pro_or_anti.length == 0) {
        alert("All the fields are required.");
        return;
    }

    // mark as clicked to avoid double processing
    AddInfluencerIsClicked = true;

    // real work : call on controller Train action AddFBInfluencer
    $.ajax({
        "dataType": 'json',
        "contentType": "application/json; charset=utf-8",
        "type": "GET",
        "url": "/Train/AddFBInfluencer",
        "data": {
            "name": "",
            "url_name": urlname,
            "pro_or_anti": pro_or_anti,
        },
        "success": function (msg) {
            AddInfluencerIsClicked = false;
            if (msg.status) {
                alert("Success " + msg.message);
                window.location = '/Train';
            } else {
                alert("Error " + msg.message);
            }
        },
        "error": function () {
            AddInfluencerIsClicked = false;
            alert("Error");
        }
    });
}

// Js for Retrieve fb post or refresh button
function RetrieveFBPost(influencerurl_name, influencerid) {

    //
    if (RetrieveFBPostIsClicked == true)
        return;

    //
    RetrieveFBPostIsClicked = true;

    //
    $.ajax({
        "dataType": 'json',
        "type": "GET",
        "url": "/Train/RetrieveFBPost",
        "data": {
            "influencerurl_name": influencerurl_name
        },
        "success": function (msg) {
            console.log(msg);
            RetrieveFBPostIsClicked = false;

            if (msg.status) {
                ResetDataTable(influencerid);
            }
            else {
                alert("Error " + msg.message);
            }
        },
        "error": function () {
            RetrieveFBPostIsClicked = false;
            alert("error")
        }
    });
}

// Method for reset data table of influence fb.
function ResetDataTable(influencerid) {
    var oTable = $('.table_' + influencerid).dataTable();
    oTable.fnClearTable();
    oTable.fnDestroy();
    LoadFacebookPosts(influencerid)
}

// method for reset the comments table
function ResetDataTableComments(influencerid) {
    var oTable = $('#tabledetails_' + influencerid).dataTable();
    oTable.fnClearTable();
    oTable.fnDestroy();
    InitializeFBCommentsForPostDataTables(influencerid)
}