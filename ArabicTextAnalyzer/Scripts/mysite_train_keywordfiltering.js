function InitializeDataTables() {
    $(function () {

        // Initialize DataTables
        $('.datatables-table').DataTable({
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
            $(this).toggleClass('selected');

            // change a href
            alert($(this).find("td:eq(6)").find("a").attr("href"));
        });
    });
}