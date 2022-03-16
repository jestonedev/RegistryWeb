$(function () {

    function paymentUfFillModal(paymentUfElem, canEdit) {
        var modal = $("#paymentUfsModal");
        var fields = paymentUfElem.find("input, select, textarea");
        var modalFields = modal.find("input, select, textarea");
        modalFields.prop("disabled", "");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            modal.find("[name='KumiPaymentUf." + name + "']").val($(elem).val());
        });
        if (canEdit) {
            modalFields.prop("disabled", "");
        } else {
            modalFields.prop("disabled", "disabled");
        }
    }

    function paymentUfFillElem(paymentUfElem) {
        var modal = $("#paymentUfsModal");
        var fields = paymentUfElem.find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            var value = modal.find("[name='KumiPaymentUf." + name + "']").val();
            $(elem).val(value);
            if (name === "IdPaymentUf") {
                var downloadBtn = paymentUfElem.find(".payment-uf-download-btn");
                downloadBtn.attr("data-id-payment-uf", value);
            }
        });
    }

    function getPaymentUf(form) {
        var data = {};
        var fields = form.find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name");
            data[name] = $(elem).val();
        });
        data["KumiPaymentUf.IdPayment"] = $("#paymentsForm #IdPayment").val();
        return data;
    }

    function paymentUfToFormData(paymentUf) {
        var formData = new FormData();
        for (var field in paymentUf) {
            formData.append(field, paymentUf[field]);
        }
        return formData;
    }

    $("#paymentUfsModal").on("show.bs.modal", function () {
        $(this).find("select").selectpicker("refresh");
    });

    $("#paymentUfsModal").on("hide.bs.modal", function () {

        $(this).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
        $(this).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");

        var paymentUfElem = $('#PaymentsUfs .list-group-item[data-processing]');
        paymentUfElem.removeAttr("data-processing");
        addingPaymentUfElem = undefined;
    });

    function updateInsertPaymentUfElem() {
        var modal = $("#paymentUfsModal");
        let paymentUfElem = $('#PaymentsUfs .list-group-item[data-processing]');
        if (paymentUfElem.length > 0) {
            paymentUfFillElem(paymentUfElem);
            modal.modal('hide');
        } else {
            let list = $('#PaymentsUfs');
            list.find(".rr-list-group-item-empty").hide();
            let paymentUfsToggle = $('#PaymentsUfsForm .payment-ufs-toggler');
            if (!isExpandElemntArrow(paymentUfsToggle)) // развернуть при добавлении, если было свернуто 
                paymentUfsToggle.click();
            list.append(addingPaymentUfElem);
            let paymentUfElem = $('#PaymentsUfs .list-group-item').last();
            paymentUfFillElem(paymentUfElem);
            modal.modal('hide');
            $([document.documentElement, document.body]).animate({
                scrollTop: $(paymentUfElem).offset().top
            }, 1000);
        }
    }

    function updateCountPaymentUfsBadge() {
        var form = $('#PaymentsUfsForm');
        var badge = form.find(".rr-count-badge");
        var count = $('#PaymentsUfsForm').find('.list-group-item').length - 1;
        if (count > 0) {
            badge.text(count);
            badge.css("display", "inline-block");
        }
        else {
            badge.find(".rr-count-badge").text('');
            badge.css("display", "none");
        }
    }

    $("#paymentUfsModal").on("click", "#savePaymentUfModalBtn", function (e) {
        $(".input-decimal").each(function (idx, elem) {
            $(elem).val($(elem).val().replace(".", ","));
        });

        var form = $("#PaymentUfsModalForm");
        var isValid = form.valid();

        if (isValid) {
            let paymentUf = paymentUfToFormData(getPaymentUf(form));
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/KumiPaymentUfs/SavePaymentUf',
                data: paymentUf,
                processData: false,
                contentType: false,
                success: function (paymentUfReturn) {
                    if (paymentUfReturn.idPaymentUf > 0) {
                        form.find("[name='KumiPaymentUf.IdPaymentUf']").val(paymentUfReturn.idPaymentUf);
                        updateInsertPaymentUfElem();
                        updateCountPaymentUfsBadge();

                    } else {
                        alert('Произошла ошибка при сохранении');
                    }
                }
            });

        } else {
            form.find("select").each(function (idx, elem) {
                var id = $(elem).prop("id");
                var name = $(elem).prop("name");
                var errorSpan = $("span[data-valmsg-for='" + name + "']");
                if (errorSpan.hasClass("field-validation-error")) {
                    $("button[data-id='" + id + "']").addClass("input-validation-error");
                }
            });
        }
    });

    function deletePaymentUf(e) {
        let isOk = confirm("Вы уверены что хотите удалить уведомление?");
        if (isOk) {
            let paymentUfElem = $(this).closest(".list-group-item");
            let idPaymentUf = paymentUfElem.find("input[name^='IdPaymentUf']").val();
            $.ajax({
                async: false,
                type: 'POST',
                url: window.location.origin + '/KumiPaymentUfs/DeletePaymentUf',
                data: { idPaymentUf: idPaymentUf },
                success: function (ind) {
                    if (ind === 1) {
                        paymentUfElem.remove();
                        if ($("#PaymentsUfs .list-group-item").length === 1) {
                            $("#PaymentsUfs .rr-list-group-item-empty").show();
                        }

                        updateCountPaymentUfsBadge();
                    }
                    else {
                        alert("Ошибка удаления!");
                    }
                }
            });
        }
        e.preventDefault();
    }

    var addingPaymentUfElem = undefined;

    function addPaymentUf(e) {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/KumiPayments/AddPaymentUf',
            data: { idPayment: $("#paymentsForm #IdPayment").val() },
            success: function (elem) {
                let canEdit = $("#PaymentsUfs").data("can-edit");
                addingPaymentUfElem = elem;
                paymentUfFillModal($(elem), canEdit);
                var modal = $("#paymentUfsModal");
                modal.modal('show');
            }
        });
        e.preventDefault();
    }

    $('body').on('click', '.payment-uf-open-btn, .payment-uf-edit-btn', function (e) {
        var paymentUfElem = $(this).closest('.list-group-item');
        if (paymentUfElem.find("input[name^='IdPaymentUf']").val() === "0") {
            paymentUfElem.attr("data-processing", "create");
        } else {
            paymentUfElem.attr("data-processing", "edit");
        }
        var canEdit = $("#PaymentsUfs").data("can-edit") === "True";
        paymentUfFillModal(paymentUfElem, canEdit);
        var modal = $("#paymentUfsModal");
        modal.modal('show');
        e.preventDefault();
    });

    $('#PaymentsUfsForm').on('click', '.payment-uf-delete-btn', deletePaymentUf);
    $("#PaymentsUfsForm").on("click", "#paymentUfAdd", addPaymentUf);

    $("body").on("click", ".payment-uf-download-btn", function (e) {
        var modal = $("#PaymentUfDownloadModal");
        var modalFields = modal.find("input, select, textarea");
        modalFields.prop("disabled", "");
        modal.find("#PaymentUfDownload_IdPayemntUf").val($(this).attr("data-id-payment-uf"));
        modal.modal('show');
        e.preventDefault();
    });

    $("#PaymentUfDownloadModal .rr-report-submit").on("click", function (e) {
        e.preventDefault();
        var modal = $("#PaymentUfDownloadModal");
        var form = modal.closest("form");
        var isValid = form.valid();
        if (!isValid) {
            fixBootstrapSelectHighlight(form);
            return;
        }
        var idSigner = modal.find("#PaymentUfDownload_IdSigner").val();
        var signDate = modal.find("#PaymentUfDownload_SignDate").val();
        var idPaymentUf = modal.find("#PaymentUfDownload_IdPayemntUf").val();
        var url = "/KumiPayments/DownloadPaymentUf?IdPaymentUf=" + idPaymentUf + "&IdSigner=" + idSigner + "&signDate=" + signDate;
        downloadFile(url);
        modal.modal("hide");
    });

    $("#PaymentUfDownloadModal").on("change", "select", function () {
        fixBootstrapSelectHighlightOnChange($(this));
    });
});