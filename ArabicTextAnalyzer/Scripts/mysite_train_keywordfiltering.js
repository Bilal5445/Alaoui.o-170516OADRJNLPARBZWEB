function InitializeDataTables() {
    $(function () {

        // Initialize DataTables
        $('.datatables-table').DataTable({
            // Enable mark.js search term highlighting
            // mark: true,
            mark: {
                element: 'span',
                className: 'highlight'
            }
        });
    });
}