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
        if (this.files.length === 0) return;
        if (this.files.length > 1 || !this.files[0].name.endsWith(".xlsx")) {
            $(this).closest("form").submit();
        } else {
            var modal = $("#UploadPaymentsDateEnrollUfkModal");
            modal.modal('show');
        }
    });

    $("#UploadPaymentsDateEnrollUfkModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        if (!form.valid()) return;
        var dateEnrollUfk = form.find("#UploadPayments_DateEnrollUfk").val();
        var submitForm = $("form[name='UploadPaymentsForm']");
        submitForm.find("input[name='DateEnrollUfk']").val(dateEnrollUfk);
        submitForm.submit();
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

    $("#BksSetDateEnrollUfk").on("click", function (e) {
        e.preventDefault();
        var modal = $("#UpdateBksPaymentsDateEnrollUfkModal");
        modal.modal('show');
    });

    $("#UpdateBksPaymentsDateEnrollUfkModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        if (!form.valid()) return;
        $(this).prop("disabled", "disabled");

        var dateDoc = form.find("#UpdateBksPaymentsDateEnrollUfk_DateDoc").val();
        var dateEnrollUfk = form.find("#UpdateBksPaymentsDateEnrollUfk_DateEnrollUfk").val();

        $.ajax({
            type: 'POST',
            url: window.location.origin + '/KumiPayments/UpdateBksPaymentsDateEnrollUfkForm',
            data: { dateDoc, dateEnrollUfk  },
            dataType: 'json',
            success: function (data) {
                if (data.state === "Success") {
                    $("#UpdateBksPaymentsDateEnrollUfkModal").modal('hide');
                } else {
                    form.find(".rr-update-date-enroll-ufk-error").removeClass("d-none").text(data.error);
                }
                $("#UpdateBksPaymentsDateEnrollUfkModal .rr-report-submit").prop("disabled", "");
            }
        });
    });
});