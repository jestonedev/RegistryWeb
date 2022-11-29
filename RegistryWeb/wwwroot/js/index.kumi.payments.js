var searchModal = function () {
    $('input[name="FilterOptions.CommonFilter"]').val("");

    $("#FilterOptions_Sum").val($("#FilterOptions_Sum").val().replace(",", "."));

    if ($("form.filterForm").valid()) {
        $("form.filterForm").submit();
        $("#filterModal").modal("hide");
    }
};

var filterClearModal = function () {
    $("#filterModal input[type='text'], #filterModal input[type='date'], #filterModal input[type='hidden'], #filterModal select").val("");
    $('#filterModal').find("select").selectpicker('render');
    $("#filterModal input[type='checkbox']").prop("checked", false);
    $("form.filterForm").valid();
};
var filterClear = function () {
    filterClearModal();
    $("form.filterForm").submit();
};

$(function () {
    $('#PayerFiltersToggler').on("click", $("#PayerFilters"), elementToogleHide);
    $('#RecipientFiltersToggler').on("click", $("#RecipientFilters"), elementToogleHide);

    $('#searchModalBtn').click(searchModal);
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);

    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        modal.modal('show');
    });

    $("#FilterOptions_CommonFilter").keyup(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            filterClear();
        }
    });
    $("#paymentFilterClearBtn").click(function (event) {
        event.preventDefault();
        $("#FilterOptions_CommonFilter").val("");
        $("form.filterForm").submit();
    });

    $("#UploadPaymentsFiles").on("change", function (event) {
        event.preventDefault();
        $(this).closest("form").submit();
    });


    $("body").on("click", ".payment-ufs-download-btn", function (e) {
        var modal = $("#PaymentUfsDownloadModal");
        var modalFields = modal.find("input, select, textarea");
        modalFields.prop("disabled", "");
        modal.modal('show');
        e.preventDefault();
    });

    $("#PaymentUfsDownloadModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var modal = $("#PaymentUfsDownloadModal");
        var form = modal.closest("form");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return;
        }
        var idSigner = modal.find("#PaymentUfsDownload_IdSigner").val();
        var signDate = modal.find("#PaymentUfsDownload_SignDate").val();
        var dateUf = modal.find("#PaymentUfsDownload_DateUf").val();
        var url = "/KumiPayments/DownloadPaymentUfs?IdSigner=" + idSigner + "&signDate=" + signDate + "&dateUf=" + dateUf;
        downloadFile(url);
        modal.modal("hide");
    });

    $("#PaymentUfsDownloadModal").on("change", "select", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });

});