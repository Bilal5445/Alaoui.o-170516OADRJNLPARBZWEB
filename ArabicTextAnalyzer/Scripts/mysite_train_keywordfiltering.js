var poststable;
var selectedArabiziIds = [];

function InitializeDataTables() {
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
                    { "data": "FormattedRemoveAndApplyTagCol", "className": "controls center top" } // class 'controls' just there to identify the td to prevent click on 'remove' (or any other button in this column) to be considered as selection
            ],
            "columnDefs": [{
                "defaultContent": "-",
                "targets": "_all"
            }],
            // server side
            "processing": true,
            "serverSide": true,
            "ajax": "/Train/DataTablesNet_ServerSide_GetList/0"
        });

        $('.datatables-table tbody').on('click', 'tr', function (e) {

            // if we click on the last column, do not select/unselect
            if ($(e.target).closest("td").attr('class').includes("controls"))
                return;

            // select the row
            $(this).toggleClass('selected');

            // add/remove guid to array
            if ($(this).hasClass('selected')) {

                // we select

                // find the guid of arabiz entry from the href
                var hrefInnerId = $(this).find("td:eq(6)").find("> a").attr("href").substring("/Train/Train_DeleteEntry/?arabiziWordGuid=".length);

                // add it to global array
                selectedArabiziIds.push(hrefInnerId);

                console.log(hrefInnerId);

                // save old id in backud
                $(this).find("td:eq(6)").find("> a").attr('data-backhref', hrefInnerId);

            } else {

                // we deselect

                // find the guid of arabiz entry from the backup href
                var hrefBackInnerId = $(this).find("td:eq(6)").find("> a").attr("data-backhref");

                // drop it from global (we know it is there)
                var index = selectedArabiziIds.indexOf(hrefBackInnerId);
                selectedArabiziIds.splice(index, 1);

                // set new value href (from backup)
                var newhref = "/Train/Train_DeleteEntry/?arabiziWordGuid=" + hrefBackInnerId;
                $(this).find("td:eq(6)").find("> a").attr("href", newhref);
            }

            // loop over selected to concatenate the arabizi entries ids
            $('tr.selected td:last-child').each(function (index) {
                // new value href
                var newhref = "/Train/Train_DeleteEntries/?arabiziWordGuids=" + selectedArabiziIds.join();

                // set new value
                $(this).find("> a").attr("href", newhref);
            });
        });
    });
}

// table for FB
function InitializeFBDataTables(fluencerid) {
    $(function () {

        // Initialize DataTables
        $('.table_' + fluencerid).DataTable({
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
                    { "data": "id", "className": "center top" },
                    { "data": "fk_influencer", "className": "arabizi-text top" },
                    { "data": "post_text", "className": "arabizi-text top" },
                    {"data": "translated_text", "className": "arabizi-text top"},
                    { "data": "likes_count", "className": "arabic-text top" },
                    { "data": "comments_count", "className": "arabic-text top entitiestype" },
                    { "data": "date_publishing", "className": "arabic-text top entities" },
                    {
                        "data": function (data)
                        {
                            var str = '';
                            str = str + '<a class="btn btn-warning btn-xs" onclick="'+"TranslateContent(this)"+'">Translate</a>';
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


//Table For FB For Particular influencer
function LoadFacebookPosts(fluencerid) {
    var $checkedBoxes = $('.table_' + fluencerid + ' tbody tr');
    if ($checkedBoxes.length == 0) {
        InitializeFBDataTables(fluencerid)
    }
    fnCallback(fluencerid)
}
function fnCallback(id)
{
    $('.table_' + id).show();
    if (id.length > 0)
    {
        $('.tab-pane').each(function () {
            $(this).removeClass('active');
        });
        $('#second_' + id).addClass('active')
        $('.mainUL').each(function () {
            $(this).removeClass('active');
        })
        $('#' + id).addClass('active');
    }
}
var AddInfluencerIsClicked = false;
function AddInfluencer()
{
    if (AddInfluencerIsClicked == false)
    {
        var urlname = $('#txtUrlName').val();
        var pro_or_anti = $('#ddlPro_or_anti').val();
        AddInfluencerIsClicked = true;

        //var urlToPost = AddFBInfluencer
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
                    window.location = '/Train';
                   
                }
                else {
                    alert("Error " + msg.message);
                }
               
            },
             "error": function () {
                 AddInfluencerIsClicked = false;
                 alert("Error");
            }
        });
    }
  
}

function TranslateContent(obj)
{
    
    //var token = $('#hdnToken').val();
    //alert(token)
  //  alert($($(obj).parents("tr").find("td")[2]).html())
    if ($($(obj).parents("tr").find("td")[3]).html().trim().length == 0)
    {
        if ($($(obj).parents("tr").find("td")[2]).html().trim().length > 0) {
            var _tag = ($($(obj).parents("tr").find("td")[2])).html().toString();
            var id = ($($(obj).parents("tr").find("td")[0])).html().toString();
            //var urlToPost = "/Train/TranslateFbPost"// 'http://localhost:50037/api/Arabizi/GetArabicDarijaEntry/' + _tag + '?token=' + token// 
            // alert(_tag)
            $.ajax({
                "dataType": 'json',
                // "contentType": "application/json; charset=utf-8",
                "type": "GET",
                "url": "/Train/TranslateFbPost",
                "data": {
                    "content": _tag,
                    "id":id
                    //"mainEntity": _tag,
                    // "ArabiziEntryDate": new Date(),
                    // "id": 1
                },
                "success": function (msg) {
                    console.log(msg);
                    //var json = JSON.parse(msg);
                    if (msg.status) {
                        //alert(msg.recordsFiltered)

                        if ($($(obj).parents("tr").find("td")[3]).html().trim().length == 0) {
                            $($(obj).parents("tr").find("td")[3]).html(msg.recordsFiltered)
                        }
                    }
                    //customAlertMessages.alerts.success("Lead has been assigned successfully.");
                    //$('#divGridLeads').DataTable().ajax.reload();
                    //$('#grdLeadAssigned').DataTable().ajax.reload();
                },
                "error": function () {
                    alert("Error")
                }
            });

        }
        else
        {
            alert("There is no post for translate");
        }
    }
    else {
        alert("This post is already translated");
    }
   
}

function RetrieveFBPost(influencerurl_name,influencerid)
{
    $.ajax({
        "dataType": 'json',       
        "type": "GET",
        "url": "/Train/RetrieveFBPost",
        "data": {
            "influencerurl_name": influencerurl_name
            //"mainEntity": _tag,
            // "ArabiziEntryDate": new Date(),
            // "id": 1
        },
        "success": function (msg) {
            console.log(msg);
            //var json = JSON.parse(msg);
            if (msg.status) {
                ResetDataTable(influencerid);
                //alert(msg.recordsFiltered)
                //$('.table_' + influencerid).DataTable().ajax.reload();           
                //fnCallback(influencerid)
            }
            //customAlertMessages.alerts.success("Lead has been assigned successfully.");
            //$('#divGridLeads').DataTable().ajax.reload();
            //$('#grdLeadAssigned').DataTable().ajax.reload();
        },
        "error": function () {
            alert("error")
        }
    });
}
function ResetDataTable(influencerid) {
    var oTable = $('.table_' + influencerid).dataTable();
    oTable.fnClearTable();
    oTable.fnDestroy();
    LoadFacebookPosts(influencerid)
}







function fnFormatDetails(table_id, html) {
    var sOut = "<table id=\"exampleTable_" + table_id + "\">";
    sOut += html;
    sOut += "</table>";
    return sOut;
}

//////////////////////////////////////////////////////////// EXTERNAL DATA - Array of Objects 

var terranImage = "https://i.imgur.com/HhCfFSb.jpg";
var jaedongImage = "https://i.imgur.com/s3OMQ09.png";
var grubbyImage = "https://i.imgur.com/wnEiUxt.png";
var stephanoImage = "https://i.imgur.com/vYJHVSQ.jpg";
var scarlettImage = "https://i.imgur.com/zKamh3P.jpg";

// DETAILS ROW A 
var detailsRowAPlayer1 = { pic: jaedongImage, name: "Jaedong", team: "evil geniuses", server: "NA" };
var detailsRowAPlayer2 = { pic: scarlettImage, name: "Scarlett", team: "acer", server: "Europe" };
var detailsRowAPlayer3 = { pic: stephanoImage, name: "Stephano", team: "evil geniuses", server: "Europe" };

var detailsRowA = [detailsRowAPlayer1, detailsRowAPlayer2, detailsRowAPlayer3];

// DETAILS ROW B 
var detailsRowBPlayer1 = { pic: grubbyImage, name: "Grubby", team: "independent", server: "Europe" };

var detailsRowB = [detailsRowBPlayer1];

// DETAILS ROW C 
var detailsRowCPlayer1 = { pic: terranImage, name: "Bomber", team: "independent", server: "NA" };

var detailsRowC = [detailsRowCPlayer1];

var rowA = { race: "Zerg", year: "2014", total: "3", details: detailsRowA };
var rowB = { race: "Protoss", year: "2014", total: "1", details: detailsRowB };
var rowC = { race: "Terran", year: "2014", total: "1", details: detailsRowC };

var newRowData = [rowA, rowB, rowC];

////////////////////////////////////////////////////////////

var iTableCounter = 1;
var oTable;
var oInnerTable;
var detailsTableHtml;

//Run On HTML Build
$(document).ready(function () {

    // you would probably be using templates here
    detailsTableHtml = $("#detailsTable").html();

    //Insert a 'details' column to the table
    var nCloneTh = document.createElement('th');
    var nCloneTd = document.createElement('td');
    nCloneTd.innerHTML = '<img src="http://i.imgur.com/SD7Dz.png">';
    nCloneTd.className = "center";

    $('#exampleTable thead tr').each(function () {
        this.insertBefore(nCloneTh, this.childNodes[0]);
    });

    $('#exampleTable tbody tr').each(function () {
        this.insertBefore(nCloneTd.cloneNode(true), this.childNodes[0]);
    });


    //Initialse DataTables, with no sorting on the 'details' column
    var oTable = $('#exampleTable').dataTable({
        "bJQueryUI": true,
        "aaData": newRowData,
        "bPaginate": false,
        "aoColumns": [
            {
                "mDataProp": null,
                "sClass": "control center",
                "sDefaultContent": '<img src="http://i.imgur.com/SD7Dz.png">'
            },
            { "mDataProp": "race" },
            { "mDataProp": "year" },
            { "mDataProp": "total" }
        ],
        "oLanguage": {
            "sInfo": "_TOTAL_ entries"
        },
        "aaSorting": [[1, 'asc']]
    });

    /* Add event listener for opening and closing details
    * Note that the indicator for showing which row is open is not controlled by DataTables,
    * rather it is done here
    */
    $('#exampleTable tbody td img').live('click', function () {
        var nTr = $(this).parents('tr')[0];
        var nTds = this;

        if (oTable.fnIsOpen(nTr)) {
            /* This row is already open - close it */
            this.src = "http://i.imgur.com/SD7Dz.png";
            oTable.fnClose(nTr);
        }
        else {
            /* Open this row */
            var rowIndex = oTable.fnGetPosition($(nTds).closest('tr')[0]);
            var detailsRowData = newRowData[rowIndex].details;

            this.src = "http://i.imgur.com/d4ICC.png";
            oTable.fnOpen(nTr, fnFormatDetails(iTableCounter, detailsTableHtml), 'details');
            oInnerTable = $("#exampleTable_" + iTableCounter).dataTable({
                "bJQueryUI": true,
                "bFilter": false,
                "aaData": detailsRowData,
                "bSort": true, // disables sorting
                "aoColumns": [
                    { "mDataProp": "pic" },
                    { "mDataProp": "name" },
                    { "mDataProp": "team" },
                    { "mDataProp": "server" }
                ],
                "bPaginate": false,
                "oLanguage": {
                    "sInfo": "_TOTAL_ entries"
                },
                "fnRowCallback": function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
                    var imgLink = aData['pic'];
                    var imgTag = '<img width="100px" src="' + imgLink + '"/>';
                    $('td:eq(0)', nRow).html(imgTag);
                    return nRow;
                }
            });
            iTableCounter = iTableCounter + 1;
        }
    });


});