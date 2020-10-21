$(document).ready(function () {
    var table = $('#ttt').DataTable({
        fixedHeader: true,
        fixedColumns: true,
        scrollX: true,
        scrollY: "60vh",
        scrollCollapse: true,
        paging: false,
        info: false,
        searching: false
    });
    $("#configModalShow").on("click", function (e) {
        e.preventDefault();
        $("#configModal").modal("show");
    });
    $("#configModalForm .r-config-apply").on("click", function (e) {
        e.preventDefault();
        $("[id^='Column_']").each(function () {
            var id = $(this).attr('data-column');
            var isEnable = $(this).is(':checked');
            var col = table.column(id);
            if ( !col.visible() && isEnable || col.visible() && !isEnable) {
                col.visible(isEnable);
            }
        });
        $("#configModal").modal("hide");
    });
});