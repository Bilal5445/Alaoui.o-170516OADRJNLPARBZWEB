var poststable;
var selectedArabiziIds = [];    // array of arabizi entries ids that have been selected by the user each he clicks on a row
var vars = {};
var Comments = "";
var TimeintervalforFBMethods = 1000 * 60 * 2; // 2 minutes : Time interval for method run for get fb posts and comments and translate posts and comments
var fbTabPagesLoaded = false;

// events variables
var ViewInfluencerIsClicked = false;
var TranslateContentIsClicked = false;
var GetCommentsIsClicked = false;
var GetTranslateCommentIsClicked = false;
var TranslateCommentIsClicked = false;
var AddInfluencerIsClicked = false;
var RetrieveFBPostIsClicked = false;
var AddTextEntityClicked = false;

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
                'copyHtml5', 'excel', 'csv', 'selectAll', 'selectNone'
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

// Clicking on tab Table For FB For Particular influencer (or FB page)
function LoadFacebookPosts(influencerId) {

    // check before
    if (ViewInfluencerIsClicked == true)
        return;

    // mark as clicked to avoid double processing
    ViewInfluencerIsClicked = true;

    // if the table associated with the fb page has no rows yet, load them from DB (via server side controller action)
    var $checkedBoxes = $('.table_' + influencerId + ' tbody tr');
    if ($checkedBoxes.length == 0) {
        InitializeFBPostsDataTables(influencerId);
    }

    //
    ShowFBPage(influencerId);

    // reset to not clicked
    ViewInfluencerIsClicked = false
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
                {
                    "data": null, "className": "details-control",
                    "defaultContent": '<img src="http://i.imgur.com/SD7Dz.png" class="imagetag" onclick="' + "ShowFBComments(this)" + '">'
                },
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
            "ajax": {
                "url": "/Train/DataTablesNet_ServerSide_FB_Posts_GetList?fluencerid=" + fluencerid,
                "dataSrc": function (json) {
                    return json.data;
                }
            }
        });
    });
}

function ShowFBPage(id) {

    //
    if (id.length > 0) {

        // show associated datatables.net table with the FB page
        $('.table_' + id).show();

        // disable any other active tab content / activate current fb page tab content
        $('.tab-pane').each(function () {
            $(this).removeClass('active');
        });
        $('#second_' + id).addClass('active')

        // disable any other active tab header / activate current fb page tab header
        $('.mainUL').each(function () {
            $(this).removeClass('active');
        })
        $('#' + id).addClass('active');
    }
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
function ShowFBComments(img) {
    // img = img.imagetag

    // check before
    if (GetCommentsIsClicked == true)
        return;

    // mark as clicked to avoid double processing
    GetCommentsIsClicked = true;

    //
    var tr = $(img).closest('tr');
    var tds = $(img).parents("tr").find("td");
    var tdPostId = $(tds[1]);
    var idCol = tdPostId.html().toString().split('_');
    var postId = idCol[1];
    var influencerId = idCol[0];
    var table = vars[influencerId];
    var row = table.row(tr);

    //
    ToggleFBCommentsTable(row, tr, img, postId);

    //
    GetCommentsIsClicked = false;
}

function ToggleFBCommentsTable(row, tr, img, postId) {

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

        if (!$('#tabledetails_' + postId).length) {

            // create on the fly a sub table (html table + theader) for the comments associated with a post in a page
            var tablecontent = CommentTable(postId);
            row.child(tablecontent).show();

            // get using server size ajax to datatables.net the actual comments
            InitializeFBCommentsForPostDataTables(postId);

            // add a global bulk translate button for comments
            $('#tabledetails_' + postId + '_length').append('<a class="btn btn-info btn-xs" style="margin-left:5px" onclick="GetTranslateComment(' + postId + ')">Bulk Translate</a>')

            // add a global bulk check all button for comments
            $('#tabledetails_' + postId + '_length').append('<a class="btn btn-info btn-xs" style="margin-left:5px" onclick="CheckUnCheckAllComments(' + postId + ')">Check/Uncheck All</a><h3>Comments</h3>')

        } else {
            row.child($('#tabledetails_' + postId).html()).show();
        }

        //
        tr.addClass('shown');
    }
}

// method used by ShowFBComments above to get table up to theader
function CommentTable(id) {

    // build the html table up to the header (the content is brought vis server side controller)
    var html = '<table id="tabledetails_' + id + '" class="table table-striped table-hover table-bordered"><thead class="header"><tr><th></th><th class="center top col50px">ID</th><th class="center top col50prc">Message</th><th class="center top col50prc">Translated Message</th><th class="center top col130px">Created Time</th><th class="center top col75px">Action</th></tr></thead></table>'

    return html;
}

// method used by ShowFBComments above to get table actual comments
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
function GetTranslateComment(postid) {

    // check before
    if (GetTranslateCommentIsClicked == true)
        return;

    // mark as clicked to avoid double processing
    GetTranslateCommentIsClicked = true;

    //
    var translatedCommentId = '';

    // loop on all comments row and for checked ones save the ids
    $('.cbxComment_' + postid).each(function () {
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
        postOnCommentsTranslate(translatedCommentId, postid)
    } else {
        alert("Error: Please check at least one cheackbox.")
        GetTranslateCommentIsClicked = false;
    }
}

// method for check/uncheck all comments for the post with id
function CheckUnCheckAllComments(postid) {

    // loop on all comments row and check (or uncheck) each one
    var firstIsChecked = false;
    $('.cbxComment_' + postid).each(function (index, value) {
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

function JsTestFBFilter(influencerid) {

    //
    $.ajax({
        "dataType": 'json',
        "type": "GET",
        "url": "/Train/TranslateAndExtractNERFBPostsAndComments",
        "data": {
            "influencerid": influencerid
        },
        "success": function (msg) {
            console.log(msg);
        },
        "error": function () {
            console.log("error in TranslateFBPostsAndComments");
        }
    });
}

// Js for Retrieve fb post or refresh button
function JsRetrieveFBPosts(influencerurl_name, influencerid) {

    //
    var model = new FBDataVM();
    model.RetrieveFBPostIsClicked = false;

    // call function
    model.JsRetrieveFBPosts(influencerurl_name, influencerid);
}

// Method for schedule a task for retrieve the fb posts and comments in a time interval
var FBDataVM = function () {

    // init
    this.CallMethod = false;
    this.CallTranslateMethod = false;
    this.RetrieveFBPostIsClicked = false;
    this.isAutoRetrieveFBPostAndComments = false;

    // wrap function to call original function JsRetrieveFBPosts
    this.GetFBPostsAndComments = function (influencerUrl, influencerid) {

        // DBG
        console.log("GetFBPostsAndComments - begin");

        var currentInstance = this;
        var intervalFlag = true;
        if (currentInstance.CallMethod == false) {

            //
            currentInstance.CallMethod = true;

            //
            currentInstance.JsRetrieveFBPosts(influencerUrl, influencerid, intervalFlag);
        }
    };

    // function to translate posts/comments and extract NERs from them and filter over negative NER
    this.TranslateFBPostsAndComments = function (influencerid) {

        // DBG
        console.log("TranslateFBPostsAndComments - begin");

        var currentInstance = this;
        // alert(influencerid);
        if (currentInstance.CallTranslateMethod == false) {
            currentInstance.CallTranslateMethod = true;

            //
            $.ajax({
                "dataType": 'json',
                "type": "GET",
                "url": "/Train/TranslateAndExtractNERFBPostsAndComments",
                "data": {
                    "influencerid": influencerid
                },
                "success": function (msg) {
                    console.log(msg);
                    currentInstance.CallTranslateMethod = false;
                    if (msg.status) {
                        if ($('#' + influencerid).hasClass('active')) {
                            ResetDataTable(influencerid);
                        }
                    }
                },
                "error": function () {
                    console.log("error in TranslateFBPostsAndComments");
                    currentInstance.CallTranslateMethod = false;
                }
            });
        }
    };

    // original function to retrieve fb posts (and comments as well) 
    this.JsRetrieveFBPosts = function (influencerurl_name, influencerid, intervalFlag) {

        // DBG
        console.log("JsRetrieveFBPosts - begin");

        var currentInstance = this;
        if ((currentInstance.RetrieveFBPostIsClicked == false && currentInstance.CallMethod == false) || intervalFlag == true) {

            // mark as clicked to avoid double processing
            currentInstance.RetrieveFBPostIsClicked = true;
            currentInstance.CallMethod = true;

            // real work : call on controller Train action Retrieve FB Posts
            $.ajax({
                "dataType": 'json',
                "type": "GET",
                "url": "/Train/RetrieveFBPosts",
                "data": {
                    "influencerurl_name": influencerurl_name
                },
                "success": function (msg) {

                    console.log("JsRetrieveFBPosts - msg : " + msg);
                    console.log("JsRetrieveFBPosts - msg.status : " + msg.status);
                    console.log("retrievedPostsCount : " + msg.retrievedPostsCount);   // DBG
                    console.log("retrievedCommentsCount : " + msg.retrievedCommentsCount);   // DBG

                    //
                    currentInstance.RetrieveFBPostIsClicked = false;
                    currentInstance.CallMethod = false;

                    if (intervalFlag == true) {

                        if ($('#' + influencerid).hasClass('active')) {

                            // refresh
                            ResetDataTable(influencerid);
                        }

                    } else if (msg.status) {

                        // refresh
                        ResetDataTable(influencerid);

                    } else {

                        console.log("Success Msg Status Error : " + msg.message);
                        alert("Success Msg Status Error : " + msg.message);
                    }
                },
                "error": function (jqXHR, exception) {

                    currentInstance.RetrieveFBPostIsClicked = false;

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
                    // $('#post').html(msg);
                    console.log("JsRetrieveFBPosts - Error : " + msg);
                    alert("Error : " + msg);
                }
            });
        }
    }

    // function to start retrieving posts and posts from FB and translating them
    this.init = function (influencerUrl, influencerid, isAutoRetrieveFBPostAndComments) {

        console.log("init - begin");

        //
        var currentInstance = this;

        //    
        console.log("setinterval - GetFBPostsAndComments");
        setInterval(function () {

            if ($('#cbxAutoRetrieveFBPostAndComments_' + influencerid).is(":checked")) {
                currentInstance.isAutoRetrieveFBPostAndComments = true;
            } else {
                currentInstance.isAutoRetrieveFBPostAndComments = false;
            }

            if (currentInstance.isAutoRetrieveFBPostAndComments == true) {
                currentInstance.GetFBPostsAndComments(influencerUrl, influencerid);
            }

        }, TimeintervalforFBMethods);

        //
        console.log("setinterval - TranslateFBPostsAndComments");
        setInterval(function () {

            if ($('#cbxAutoRetrieveFBPostAndComments_' + influencerid).is(":checked")) {
                currentInstance.isAutoRetrieveFBPostAndComments = true;
            } else {
                currentInstance.isAutoRetrieveFBPostAndComments = false;
            }

            if (currentInstance.isAutoRetrieveFBPostAndComments == true) {
                currentInstance.TranslateFBPostsAndComments(influencerid);
            }

        }, TimeintervalforFBMethods);
    };
};

// Method for get the fb posts and comments of all pages.
function RefreshFBPostsAndComments() {

    var totalFbpages = $('#hdnTotalInfluencer').val();
    var noOfFbPages = parseInt(totalFbpages);

    // alert(noOfFbPages);
    if (noOfFbPages > 0) {
        fbTabPagesLoaded = true; // this flag will maintain the loading of the tabs to appear the threading.
        for (var i = 1; i <= noOfFbPages; i++) {

            // create a model per FB page
            var model = new FBDataVM();

            // get fb page url & id
            var influencerUrl = $('#hdnURLName_' + i).val();
            var influencerid = $('#hdnId_' + i).val();

            // get fb page auto retrieve y/n
            var isAutoRetrieveFBPostAndComments = false;
            if ($('#cbxAutoRetrieveFBPostAndComments_' + influencerid).is(":checked")) {
                isAutoRetrieveFBPostAndComments = true;
            }

            console.log(influencerUrl + "\n" + influencerid);
            if (influencerUrl != null && influencerUrl != undefined && influencerid != null && influencerid != undefined) {
                model.init(influencerUrl, influencerid, isAutoRetrieveFBPostAndComments);
            }
        }
    }
}

// Js fct to add per user influencer the target entities that are used to cross-match against with the Negative/Explative NER in the FB filter module
function AddTextEntity(influencerid) {

    if (AddTextEntityClicked == false) {

        var targetText = $('#txtTxetEntity_' + influencerid).val();
        var isAutoRetrieveFBPostandComments = false;

        if ($('#cbxAutoRetrieveFBPostAndComments_' + influencerid).is(":checked")) {

            $('#cbxAutoRetrieveFBPostAndComments_' + influencerid).prop("checked", true)
            isAutoRetrieveFBPostandComments = true;

        } else {

            $('#cbxAutoRetrieveFBPostAndComments_' + influencerid).prop("checked", false)
        }

        if (targetText.length > 0) {

            AddTextEntityClicked = true;

            $.ajax({
                "dataType": 'json',
                "type": "GET",
                "url": "/Train/AddTextEntity",
                "data": {
                    "influencerid": influencerid, "targetText": targetText, "isAutoRetrieveFBPostandComments": isAutoRetrieveFBPostandComments
                },
                "success": function (msg) {

                    console.log(msg);

                    // reset to not clicked
                    AddTextEntityClicked = false;

                    if (msg.status) {

                        $('#myModal_' + influencerid).modal('hide');

                    } else {

                        alert("Error " + msg.message);
                    }
                },
                "error": function () {

                    AddTextEntityClicked = false;
                    alert("Error");
                }
            });
        } else {

            alert("Please enter target text.");
        }
    }
}

// Method for reset data table of influence fb.
function ResetDataTable(influencerid) {

    // clean table
    var oTable = $('.table_' + influencerid).dataTable();
    oTable.fnClearTable();
    oTable.fnDestroy();

    // Re-load FB posts table
    LoadFacebookPosts(influencerid)
}

// method for reset the comments table
function ResetDataTableComments(influencerid) {
    var oTable = $('#tabledetails_' + influencerid).dataTable();
    oTable.fnClearTable();
    oTable.fnDestroy();
    InitializeFBCommentsForPostDataTables(influencerid)
}

// starting the timer to collect FB data
$(document).ready(function () {

    // every 2 secs, refresh posts and comments from FB
    var intervalFB = setInterval(function () {
        if (fbTabPagesLoaded == false) {
            console.log("Before RefreshFBPostsAndComments()");
            RefreshFBPostsAndComments();
        } else {
            console.log("Found the tabs");
            clearInterval(intervalFB);
        }
    }, 2000);
});
