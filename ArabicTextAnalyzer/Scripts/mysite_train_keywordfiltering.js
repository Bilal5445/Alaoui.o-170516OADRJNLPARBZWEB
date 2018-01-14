var poststable;
var selectedArabiziIds = [];

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
            dom: "<'row'<'col-sm-3'B><'col-sm-3'l><'col-sm-6'f>>" +
                "<'row'<'col-sm-12'tr>>" +
                "<'row'<'col-sm-5'i><'col-sm-7'p>>",
            buttons: [
                'copyHtml5', 'excel', 'csv'
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
            // "ajax": "/Train/DataTablesNet_ServerSide_GetList/0",
            "ajax": {
                "url": "/Train/DataTablesNet_ServerSide_GetList",
                "type": "POST",
                "data": { "adminModeShowAll": adminModeShowAll }
            },
        });

        // event click to select row
        $('.datatables-table tbody').on('click', 'tr', function (e) {

            // if we click on the last column, do not select/unselect
            if ($(e.target).closest("td").attr('class').includes("controls"))
                return;

            // select the row
            $(this).toggleClass('selected');

            // add/remove guid to array
            if ($(this).hasClass('selected')) {

                // we select

                // find the guid of arabiz entry from the href in delete button
                var hrefInnerId = $(this).find("td:eq(6)").find("> a").eq(0).attr("href").substring("/Train/Train_DeleteEntry/?arabiziWordGuid=".length);

                // add it to global array
                selectedArabiziIds.push(hrefInnerId);

                // console.log(hrefInnerId);

                // save old id in backup in delete button
                $(this).find("td:eq(6)").find("> a").eq(0).attr('data-backhref', hrefInnerId);

            } else {

                // we deselect

                // find the guid of arabiz entry from the backup href in delete button
                var hrefBackInnerId = $(this).find("td:eq(6)").find("> a").eq(0).attr("data-backhref");

                // drop it from global (we know it is there)
                var index = selectedArabiziIds.indexOf(hrefBackInnerId);
                selectedArabiziIds.splice(index, 1);

                // set new value href (from backup) in delete button
                var newhref = "/Train/Train_DeleteEntry/?arabiziWordGuid=" + hrefBackInnerId;
                $(this).find("td:eq(6)").find("> a").eq(0).attr("href", newhref);
            }

            // loop over selected to concatenate the arabizi entries ids
            $('tr.selected td:last-child').each(function (index) {
                // new value href
                var newhref = "/Train/Train_DeleteEntries/?arabiziWordGuids=" + selectedArabiziIds.join();

                // set new value in delete button
                $(this).find("> a").eq(0).attr("href", newhref);
            });
        });
    });
}

// Table For FB For Particular influencer
var ViewInfluencerIsClicked = false;
function LoadFacebookPosts(fluencerid) {
    if (ViewInfluencerIsClicked == false) {
        ViewInfluencerIsClicked = true;
        var $checkedBoxes = $('.table_' + fluencerid + ' tbody tr');
        if ($checkedBoxes.length == 0) {
            InitializeFBDataTables(fluencerid)
        }
        fnCallback(fluencerid)
    }
}

var vars = {};
// table for FB post table
function InitializeFBDataTables(fluencerid) {
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
            "ajax": "/Train/DataTablesNet_ServerSide_FB_GetList?fluencerid=" + fluencerid
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
var TranslateContentIsClicked = false;

function TranslateContent(obj) {

    // check before
    if (TranslateContentIsClicked == true)
        return;

    // mark as clicked
    TranslateContentIsClicked = true;

    //
    var tds = $(obj).parents("tr").find("td");
    var id = ($(tds[1])).html().toString();
    var postText = $(tds[/*3*/2]).html().trim().replace('-', '');
    // var _tag = ($(tds[/*3*/2])).html().toString().replace('-', '');
    var translatedTextTd = tds[/*4*/3];
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

    $.ajax({
        "dataType": 'json',
        "type": "GET",
        "url": "/Train/TranslateFbPost",
        "data": {
            "content": /*_tag*/postText,
            "id": id
        },
        "success": function (msg) {
            // console.log(msg);
            TranslateContentIsClicked = false;
            if (msg.status) {

                if (translatedText.length == 0) {
                    // if succesfully transalted, remplace translatedText column by result text
                    $(translatedTextTd).html(msg.recordsFiltered)
                }
            }
            else {
                alert("Error " + msg.message);
            }
        },
        "error": function () {
            alert("Error")
            TranslateContentIsClicked = false;
        }
    });
}
// end of method.

// method for get the comments table under the row of each post table
var GetCommentsIsClicked = false;

function GetComments(obj) {

    // check before
    if (GetCommentsIsClicked == true)
        return;

    // mark as clicked
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

        $(obj).prop('src', "http://i.imgur.com/d4ICC.png")

        if (!$('#tabledetails_' + id).length) {

            var tablecontent = CommentTable(id);
            row.child(tablecontent).show();
            GetCommentsForPost(id);
            $('#tabledetails_' + id + '_length').append('<a class="btn btn-info" style="margin-left:5%" onclick="GetTranslateComment(' + id + ')">Bulk Translate</a><h3>Comments</h3>')
            GetCommentsIsClicked = false;

        } else {

            row.child($('#tabledetails_' + id).html()).show();
            GetCommentsIsClicked = false;
        }

        //
        tr.addClass('shown');
    }
}

var Comments = "";
function CommentTable(id) {
    var html = '<table id="tabledetails_' + id + '" class="table table-striped table-hover table-bordered"><thead  class="header"><tr><th></th><th class="center top col50px">ID</th><th class="center top col50prc">Message</th><th class="center top col50prc">Translated Message</th><th class="center top col130px">Created Time</th><th class="center top col75px">Action</th></tr></thead></table>'
    return html;
}

function GetCommentsForPost(id) {

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
        "ajax": "/Train/GetFBPostComment?id=" + id
    });
}
// end of method.

// method for translate the comments either on bulk translate or on translate button of comment
var GetTranslateCommentIsClicked = false;
function GetTranslateComment(id) {
    if (GetTranslateCommentIsClicked == false) {
        GetTranslateCommentIsClicked = true;
        var TranlatedCommentId = '';
        $('.cbxComment_' + id).each(function () {
            if ($(this).is(':checked')) {
                if (TranlatedCommentId.length > 0) {
                    TranlatedCommentId = TranlatedCommentId + ",'" + $(this).val() + "'";
                }
                else {
                    TranlatedCommentId = "'" + $(this).val() + "'"
                }
            }
        });
        if (TranlatedCommentId.length > 0) {
            postOnCommentsTranslate(TranlatedCommentId, id)
        }
        else {
            alert("Error: Please check atleast one cheackbox.")
            GetTranslateCommentIsClicked = false;
        }
    }
}

var TranslateCommentIsClicked = false;
function TranslateComment(obj) {
    if (TranslateCommentIsClicked == false) {

        //
        TranslateCommentIsClicked = true;

        //
        if ($($(obj).parent().parent().find("td")[3]).html().trim().replace('-', '').length == 0) {

            if ($($(obj).parent().parent().find("td")[2]).html().trim().length > 0) {

                var mainId = ($($(obj).parent().parent().find("td")[1])).html().toString();
                var id = mainId.split('_')[0];
                var TranlatedCommentId = "'" + mainId + "'";
                postOnCommentsTranslate(TranlatedCommentId, id)
            }
            else {
                alert("There is no comment text for translate.")
                TranslateCommentIsClicked = false;
            }
        }
        else {
            alert("The comment is already translated.");
            TranslateCommentIsClicked = false;
        }
    }
}

function postOnCommentsTranslate(TranlatedCommentId, id) {
    $.ajax({
        "dataType": 'json',
        // "contentType": "application/json; charset=utf-8",
        "type": "GET",
        "url": "/Train/TranslateFbComments",
        "data": {
            "ids": TranlatedCommentId
        },
        "success": function (msg) {
            console.log(msg);
            GetTranslateCommentIsClicked = false;
            TranslateCommentIsClicked = false;
            if (msg.status) {
                ResetDataTableComments(id)
            }
            else {
                alert("Error " + msg.message);
            }
        },
        "error": function () {
            GetTranslateCommentIsClicked = false;
            TranslateCommentIsClicked = false;
            alert("Error:")
        }
    });
}
// end of method.

// Js for add  influencer
var AddInfluencerIsClicked = false;
function AddInfluencer() {

    // check before
    if (AddInfluencerIsClicked == true) {
        return;
    }

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
// End of js of add influencer

// Js for Retreive fb post or refersh button
var RetrieveFBPostIsClicked = false;
function RetrieveFBPost(influencerurl_name, influencerid) {
    if (RetrieveFBPostIsClicked == false) {
        RetrieveFBPostIsClicked = true;
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
}
// end of method of refresh fb post

// Method for reset data table of influence fb.
function ResetDataTable(influencerid) {
    var oTable = $('.table_' + influencerid).dataTable();
    oTable.fnClearTable();
    oTable.fnDestroy();
    LoadFacebookPosts(influencerid)
}
// end of method

// method for reset the comments table
function ResetDataTableComments(influencerid) {
    var oTable = $('#tabledetails_' + influencerid).dataTable();
    oTable.fnClearTable();
    oTable.fnDestroy();
    GetCommentsForPost(influencerid)
}
// end of method