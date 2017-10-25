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
            dom: "<'row'<'col-sm-3'B><'col-sm-3'l><'col-sm-6'f>>" +
                "<'row'<'col-sm-12'tr>>" +
                "<'row'<'col-sm-5'i><'col-sm-7'p>>",
            buttons: [
                'copyHtml5',
            ],
        });

        $('.datatables-table tbody').on('click', 'tr', function () {
            // select the row
            $(this).toggleClass('selected');

            // find the guid of arabiz entry from the href
            var hrefInnerId = $(this).find("td:eq(6)").find("a").attr("href").substring("/Train/Train_DeleteEntry?arabiziWordGuid=".length);
            // console.log(hrefInnerId);

            // add/remove guid to array
            if ($(this).hasClass('selected')) {
                selectedArabiziIds.push(hrefInnerId);
            } else {
                var index = selectedArabiziIds.indexOf(hrefInnerId);
                if (index > -1) {
                    selectedArabiziIds.splice(index, 1);
                }
            }

            // the guids
            // console.log(selectedArabiziIds);

            // hide buttons remove/apply_tags ?
            // $('tr.selected td:last-child').not(':first').remove();
            // $('tr.selected:first td:last-child').attr('rowspan', '2');

            // change hr to gather list of ids
            /*console.log($('tr.selected td:last-child').each(function (index) {
                // console.log(index + ": " + $(this).find("a").attr("href"));
                console.log(index + ": " + $(this).find("> a").attr("href"));
            }));*/
            // .find("a").attr("href"));
            $('tr.selected td:last-child').each(function (index) {
                // console.log(index + ": " + $(this).find("a").attr("href"));
                // console.log(index + ": " + $(this).find("> a").attr("href"));

                // new value href
                // var newhref = "/Train/Train_DeleteEntries?arabiziWordGuid=" + encodeURIComponent(selectedArabiziIds.join());
                var newhref = "/Train/Train_DeleteEntries?arabiziWordGuids=" + selectedArabiziIds.join();
                // console.log(newhref);

                // set new value
                $(this).find("> a").attr("href", newhref);
            })
        });
    });
}