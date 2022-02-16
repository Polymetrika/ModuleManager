var tables = [];
$(document).ready(function () {
    tables = $('table.table').DataTable({
        paging: false,
        scrollY: 400
    } );
});