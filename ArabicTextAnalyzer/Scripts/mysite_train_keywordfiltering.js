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