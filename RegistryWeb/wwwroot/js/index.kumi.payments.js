﻿var searchModal = function () {
    $("#FilterOptions_Sum").val($("#FilterOptions_Sum").val().replace(",", "."));

    if ($("#filterModal").data("idOrder") === undefined) {
        $('input[name="FilterOptions.CommonFilter"]').val("");

        if ($("form.filterForm").valid()) {
            $("form.filterForm").submit();
            $("#filterModal").modal("hide");
        }
    } else {
        $("#searchModalBtn").prop("disabled", "disabled");
        $("#filterModal .rr-search-mo-payment-result, #filterModal .rr-search-mo-payment-error").html("").addClass("d-none");
        $("#filterModal #bindModalBtn").addClass("d-none");
        var urlData = $("#filterModal").closest("form").serialize();
        $.ajax({
            type: 'GET',
            url: window.location.origin + '/KumiPayments/SearchPaymentsForBindOrder?' + urlData,
            dataType: 'json',
            success: function (data) {
                if (data.state === "Success") {
                    $("#filterModal .rr-search-mo-payment-result").removeClass("d-none");
                    if (data.payments.length > 0) {
                        var html = "<table class='table table-bordered'><thead><th></th><th>Реквизиты</th><th class='d-none d-lg-table-cell'>Назначение</th></thead><tbody>";
                        for (var i = 0; i < data.payments.length; i++) {
                            html += "<tr>";
                            html += "<td style='vertical-align: middle'><input type='radio' name='paymentId' value='" + data.payments[i].idPayment + "'></td>";
                            var requisits = "";
                            if (data.payments[i].numDocument !== null) {
                                requisits += "№ " + data.payments[i].numDocument;
                            }
                            if (data.payments[i].dateDocument !== null) {
                                if (requisits !== "") requisits += " ";

                                var dateDocument = new Date(data.payments[i].dateDocument);
                                year = dateDocument.getFullYear();
                                month = dateDocument.getMonth() + 1;
                                day = dateDocument.getDate();
                                dateDocumentStr = (day < 10 ? "0" + day : day) + "." + (month < 10 ? "0" + month : month) + "." + year;
                                requisits += "от " + dateDocumentStr;
                            }
                            var sum = data.payments[i].sum;
                            if (sum === null || sum === undefined)
                                sum = "0,00";
                            else {
                                var sumParts = sum.toString().replace(".", ",").split(',');
                                if (sumParts.length === 1)
                                    sum = sumParts[0] + ",00";
                                else
                                    sum = sumParts[0] + "," + sumParts[1].padEnd(2, '0');
                            }

                            html += "<td><b>Платежный документ:</b> " + requisits +
                                " <a class='btn oi oi-eye p-0 text-primary rr-payment-list-eye-btn' target='_blank' href='/KumiPayments/Details?idPayment=" + data.payments[i].idPayment+"'></a><br><b>КБК:</b> " + data.payments[i].kbk + "<br><b>Сумма:</b> " + sum + " руб.</td>";
                            html += "<td class='d-none d-lg-table-cell'><div class='rr-payment-purpose rr-bind-mo-payment-purpose'>" + data.payments[i].purpose + "</div></td>";
                            html += "</tr>";
                        }
                        html += "</tbody></table>";
                        $("#filterModal .rr-search-mo-payment-result").html(html);
                        var radios = $("#filterModal .rr-search-mo-payment-result input[type='radio']");
                        if (radios.length === 1) {
                            $(radios[0]).prop("checked", true).change();
                        }
                    } else {
                        $("#filterModal .rr-search-mo-payment-error").removeClass("d-none").html("<i>Платежи не найдены</i>");
                    }
                } else {
                    $("#filterModal .rr-search-mo-payment-error").removeClass("d-none").text(data.error);
                }
                $("#searchModalBtn").prop("disabled", "");
            }
        });
    }
};

var filterClearModal = function () {
    $("#filterModal input[type='text'], #filterModal input[type='date'], #filterModal input[type='hidden'], #filterModal select").val("");
    $('#filterModal').find("select").selectpicker('render');
    $("#filterModal input[type='checkbox']").prop("checked", false);
    $("#filterModal .rr-search-mo-payment-result, #filterModal .rr-search-mo-payment-error").html("").addClass("d-none");
    $("#filterModal #bindModalBtn").addClass("d-none");
    $("form.filterForm, form[name='DistributeMoSearchPaymentForm']").valid();
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
        var onlyBksFiles = true;
        for (var i = 0; i < this.files.length; i++) {
            if (!this.files[i].name.endsWith(".xlsx") && !this.files[i].name.endsWith(".txt")) {
                onlyBksFiles = false;
                break;
            }
        }
        if (!onlyBksFiles) {
            $(this).closest("form").submit();
        } else {
            var modal = $("#UploadPaymentsDateEnrollUfkModal");
            if (window.FileReader && window.Promise) {
                var promises = [];
                for (var j = 0; j < this.files.length; j++) {
                    var filePromise = new Promise(resolve => {
                        if (!this.files[j].name.endsWith(".txt")) {
                            resolve(true);
                            return;
                        }
                        var reader = new FileReader();
                        reader.readAsText(this.files[j], "UTF-8");
                        reader.onload = function (evt) {
                            var showDateEnroll = false;
                            var lines = evt.target.result.split('\r\n');
                            for (var i = 0; i < lines.length; i++) {
                                if (lines[i].startsWith("- - -;")) {
                                    showDateEnroll = true;
                                    break;
                                }
                            }
                            resolve(showDateEnroll);
                        };
                        reader.onerror = function () {
                            resolve(true);
                        };
                    });
                    promises.push(filePromise);
                }
                Promise.all(promises).then(function (showDateEnrollAll) {
                    var showDateEnroll = false;
                    for (var i = 0; i < showDateEnrollAll.length; i++) {
                        if (showDateEnrollAll[i]) {
                            showDateEnroll = true;
                            break;
                        }
                    }
                    if (!showDateEnroll) {
                        modal.find("#UploadPayments_DateEnrollUfk").closest(".form-row").hide();
                    } else
                        modal.find("#UploadPayments_DateEnrollUfk").closest(".form-row").show();
                    modal.modal('show');
                });
            } else {
                modal.modal('show');
            }


        }
    });

    $("#UploadPaymentsDateEnrollUfkModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        if (!form.valid()) return;
        var dateEnrollUfk = form.find("#UploadPayments_DateEnrollUfk").val();
        var idParentpayment = form.find("[name='UploadPayments_IdParentPayment']").val();
        var submitForm = $("form[name='UploadPaymentsForm']");
        submitForm.find("input[name='DateEnrollUfk']").val(dateEnrollUfk);
        submitForm.find("input[name='IdParentPayment']").val(idParentpayment);
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

    $('input#UploadPayments_ParentPayment').autocomplete({
        source: function (request, response) {
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/KumiPayments/SearchPaymentRaw',
                dataType: 'json',
                data: { text: request.term },
                success: function (data) {
                    if (data !== "0" && data !== undefined) {
                        response($.map(data, function (payment) {
                            return {
                                label: payment.description,
                                value: payment.description,
                                idPayment: payment.idPayment,
                                dateEnrollUfk: payment.dateEnrollUfk
                            };
                        }));
                    }
                }
            });
        },
        select: function (event, ui) {
            $("[name='UploadPayments_IdParentPayment']").val(ui.item.idPayment);
            $("input#UploadPayments_DateEnrollUfk").val(ui.item.dateEnrollUfk);
        },
        delay: 300,
        minLength: 3
    });

    $('input#UploadPayments_ParentPayment').focusout(function () {
        if ($("[name='UploadPayments_IdParentPayment']").val() === "") {
            $('input#UploadPayments_ParentPayment').val("");
        }
    });

    $("#UploadPayments_ParentPayment").on("input", function () {
        $("[name='UploadPayments_IdParentPayment']").val("");
    });
});