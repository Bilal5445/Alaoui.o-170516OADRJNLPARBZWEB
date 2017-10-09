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
            dom: 'Bfrtip',
            buttons: [
                'copyHtml5',
            ],
        });
    });
}