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
                    { "data": "likes_count", "className": "arabic-text top" },
                    { "data": "comments_count", "className": "arabic-text top entitiestype" },
                    { "data": "date_publishing", "className": "arabic-text top entities" },
                    {
                        "data": function (data)
                        {
                            var str = '';
                            str = str + '<a class="controls center top" onclick="'+"TranslateContent(this)"+'">Translate</a>';
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
    InitializeFBDataTables(fluencerid)
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

function AddInfluencer()
{
    var urlname = $('#txtUrlName').val();
    var pro_or_anti = $('#ddlPro_or_anti').val();
    
    var urlToPost = "http://localhost:49835/api/AccountPanel/AddFBInfluencer"
    $.ajax({
        "dataType": 'json',
        "contentType": "application/json; charset=utf-8",
        "type": "POST",
        "url": urlToPost,
        "data": {
            "name": "",
            "url_name": urlname,
            "pro_or_anti": pro_or_anti,
            "id": 1
        },
        "success": function (msg) {
            var json = JSON.parse(msg.d);
            //customAlertMessages.alerts.success("Lead has been assigned successfully.");
            //$('#divGridLeads').DataTable().ajax.reload();
            //$('#grdLeadAssigned').DataTable().ajax.reload();
        }
    });
}

function TranslateContent(obj)
{
  //  alert($($(obj).parents("tr").find("td")[2]).html())
    if ($($(obj).parents("tr").find("td")[2]).html().trim().length > 0) {
        var _tag = ($($(obj).parents("tr").find("td")[2])).html().toString();
        var urlToPost = "/Train/TranslateFbPost"
        $.ajax({
            "dataType": 'json',
            "contentType": "application/json; charset=utf-8",
            "type": "POST",
            "url": urlToPost,
            "data": {
                
                "content": _tag,
                //"mainEntity": _tag,
               // "ArabiziEntryDate": new Date(),
               // "id": 1
            },
            "success": function (msg) {
                var json = JSON.parse(msg.d);
                if (json.status)
                {
                    alert(json.recordsFiltered)
                }
                //customAlertMessages.alerts.success("Lead has been assigned successfully.");
                //$('#divGridLeads').DataTable().ajax.reload();
                //$('#grdLeadAssigned').DataTable().ajax.reload();
            }
        });
       
    }
}