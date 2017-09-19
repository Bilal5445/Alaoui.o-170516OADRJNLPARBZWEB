window.onload = function () {
    $(function () {

        // Initialize DataTables
        $('.datatables-table').DataTable({
            // Enable mark.js search term highlighting
            mark: true,
            "drawCallback": function( settings ) {
                alert( 'DataTables has redrawn the table' );
            }
        });

    });
}