// initial load fct
$(document).ready(function () {

    InitializeSocialSearchDataTables();
});

// table for FB post table
function InitializeSocialSearchDataTables() {

    // Initialize DataTables
    var table = $('.datatables-table-fbs-all').DataTable({
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
        dom: "<'row'<'col-sm-5'B><'col-sm-3'l><'col-sm-4'f>>" +
            "<'row'<'col-sm-12'tr>>" +
            "<'row'<'col-sm-5'i><'col-sm-7'p>>",
        buttons: [
            'copyHtml5', 'excel', 'csv'
        ],
        //
        "columns": [
            {
                "data": null, "className": "details-control center top",
                "defaultContent": '<img src="http://i.imgur.com/SD7Dz.png" class="imagetag" onclick="' + "ShowFBComments(this)" + '">'
            },
            { "data": "id", "className": "hide_column" },
            /*{ "data": "fk_i", "className": "arabizi-text top collapsed" },*/
            { "data": "pt", "className": "arabizi-text top" },
            { "data": "tt", "className": "arabic-text top" },
            { "data": "lc", "className": "center top" },
            { "data": "cc", "className": "center top" },
            { "data": "dp", "className": "arabizi-text top" },
            {
                "data": function (data) {
                    var str = '';
                    str = str + '<a class="btn btn-warning btn-xs" onclick="' + "JsTranslateFBPost(this)" + '">Translate</a>';
                    return str;
                },
                "className": "controls center top"
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
            "url": "/Train/DataTablesNet_ServerSide_SocialSearch_GetList",
            "dataSrc": function (json) {
                return json.data;
            },
            "data": function (d) {
                // d.myKey = "myValue";
                // d.custom = $('#myInput').val();
                // etc
                var min = $('#min').datepicker("getDate");
                if (min != null) {
                    var minjson = min.toJSON();
                    d.min = minjson;
                }
                var max = $('#max').datepicker("getDate");
                if (max != null) {
                    var maxjson = max.toJSON();
                    d.max = maxjson;
                }
            }
        }
    });

    /*$.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            var min = $('#min').datepicker("getDate");
            var max = $('#max').datepicker("getDate");
            var startDate = new Date(data[6]);
            if (min == null && max == null) { return true; }
            if (min == null && startDate <= max) { return true; }
            if (max == null && startDate >= min) { return true; }
            if (startDate <= max && startDate >= min) { return true; }
            return false;
        }
        );*/

    $("#min").datepicker({ onSelect: function () { table.draw(); }, changeMonth: true, changeYear: true });
    $("#max").datepicker({ onSelect: function () { table.draw(); }, changeMonth: true, changeYear: true });
    // var table = $('#example').DataTable();

    // Event listener to the two range filtering inputs to redraw on input
    $('#min, #max').change(function () {
        table.draw();
    });
}
