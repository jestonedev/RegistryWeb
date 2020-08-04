function getTenancyAgreements() {
    var items = $("#TenancyProcessAgreements .list-group-item").filter(function (idx, elem) {
        return !$(elem).hasClass("rr-list-group-item-empty");
    });
    return items.map(function (idx, elem) {
        var data = {};
        var fields = $(elem).find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            data[name] = $(elem).val();
        });
        return data;
    });
}

$(function () {

    function tenancyAgreementFillModal(tenancyAgreementElem, action) {
        var modal = $("#agreementModal");
        var fields = tenancyAgreementElem.find("input, select, textarea");
        var modalFields = modal.find("input, select, textarea");
        modalFields.prop("disabled", "");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            modal.find("[name='Agreement." + name + "']").val($(elem).val());
        });
        if (action === "Details" || action === "Delete")
            modalFields.prop("disabled", "disabled");
        else
            modalFields.prop("disabled", "");
    }

    function tenancyAgreementFillElem(tenancyAgreementElem) {
        var modal = $("#agreementModal");
        var fields = tenancyAgreementElem.find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            var value = modal.find("[name='Agreement." + name + "']").val();
            $(elem).val(value);
        });
    }

    function getTenancyAgreement(form) {
        var data = {};
        var fields = form.find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name");
            data[name] = $(elem).val();
        });
        data["Agreement.IdProcess"] = $("#TenancyProcessForm #TenancyProcess_IdProcess").val();
        return data;
    }

    function tenancyAgreementToFormData(agreement) {
        var formData = new FormData();
        for (var field in agreement) {
            formData.append(field, agreement[field]);
        }
        return formData;
    }

    $("#agreementModal").on("show.bs.modal", function () {
        $(this).find("select").selectpicker("refresh");
    });

    $("#agreementModal").on("hide.bs.modal", function () {

        $(this).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
        $(this).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");

        var tenancyAgreementElem = $('#TenancyProcessAgreements .list-group-item[data-processing]');
        tenancyAgreementElem.removeAttr("data-processing");
        addingTenancyAgreementElem = undefined;
    });

    function updateInsertTenancyAgreementElem() {
        var modal = $("#agreementModal");
        let tenancyAgreementElem = $('#TenancyProcessAgreements .list-group-item[data-processing]');
        if (tenancyAgreementElem.length > 0) {
            tenancyAgreementFillElem(tenancyAgreementElem);
            modal.modal('hide');
        } else {
            let list = $('#TenancyProcessAgreements');
            list.find(".rr-list-group-item-empty").hide();
            let tenancyAgreementToggle = $('#TenancyProcessAgreementsForm .tenancy-process-toggler');
            if (!isExpandElemntArrow(tenancyAgreementToggle)) // развернуть при добавлении, если было свернуто 
                tenancyAgreementToggle.click();
            list.append(addingTenancyAgreementElem);
            let tenancyAgreementElem = $('#TenancyProcessAgreements .list-group-item').last();
            tenancyAgreementFillElem(tenancyAgreementElem);
            modal.modal('hide');
            $([document.documentElement, document.body]).animate({
                scrollTop: $(tenancyAgreementElem).offset().top
            }, 1000);
        }
    }

    $("#agreementModal").on("click", "#saveAgreementModalBtn", function (e) {
        let action = $('#TenancyProcessAgreements').data('action');
        var form = $("#TenancyProcessAgreementsModalForm");
        var isValid = form.valid();

        if (isValid) {
            if (action === "Create") {
                updateInsertTenancyAgreementElem();
                return;
            }
            let tenancyAgreement = tenancyAgreementToFormData(getTenancyAgreement(form));
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/TenancyAgreements/SaveAgreement',
                data: tenancyAgreement,
                processData: false,
                contentType: false,
                success: function (tenancyAgreementReturn) {
                    if (tenancyAgreementReturn.idAgreement > 0) {
                        form.find("[name='Agreement.IdAgreement']").val(tenancyAgreementReturn.idAgreement);
                        updateInsertTenancyAgreementElem();
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
            $([document.documentElement, document.body]).animate({
                scrollTop: form.find(".input-validation-error").first().offset().top - 35
            }, 1000);
        }
    });

    function deleteTenancyAgreement(e) {
        let isOk = confirm("Вы уверены что хотите удалить дополнительное соглашение?");
        if (isOk) {
            let tenancyAgreementElem = $(this).closest(".list-group-item");
            let idAgreement = tenancyAgreementElem.find("input[name^='IdAgreement']").val();
            if (idAgreement === "0") {
                tenancyAgreementElem.remove();
                if ($("#TenancyProcessAgreements .list-group-item").length === 1) {
                    $("#TenancyProcessAgreements .rr-list-group-item-empty").show();
                }
            } else {
                $.ajax({
                    async: false,
                    type: 'POST',
                    url: window.location.origin + '/TenancyAgreements/DeleteAgreement',
                    data: { idAgreement: idAgreement },
                    success: function (ind) {
                        if (ind === 1) {
                            tenancyAgreementElem.remove();
                            if ($("#TenancyProcessAgreements .list-group-item").length === 1) {
                                $("#TenancyProcessAgreements .rr-list-group-item-empty").show();
                            }
                        }
                        else {
                            alert("Ошибка удаления!");
                        }
                    }
                });
            }
        }
        e.preventDefault();
    }

    var addingTenancyAgreementElem = undefined;

    function addTenancyAgreement(e) {
        let action = $('#TenancyProcessAgreements').data('action');

        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyProcesses/AddTenancyAgreement',
            data: { action },
            success: function (elem) {
                addingTenancyAgreementElem = elem;
                tenancyAgreementFillModal($(elem), action);
                var modal = $("#agreementModal");
                modal.modal('show');
            }
        });
        e.preventDefault();
    }

    $('body').on('click', '.tenancy-agreement-open-btn, .tenancy-agreement-edit-btn', function (e) {
        var tenancyAgreementElem = $(this).closest('.list-group-item');
        if (tenancyAgreementElem.find("input[name^='IdAgreement']").val() === "0") {
            tenancyAgreementElem.attr("data-processing", "create");
        } else {
            tenancyAgreementElem.attr("data-processing", "edit");
        }
        var action = $("#TenancyProcessAgreements").data("action");
        tenancyAgreementFillModal(tenancyAgreementElem, action);
        var modal = $("#agreementModal");
        modal.modal('show');
        e.preventDefault();
    });

    $('#TenancyProcessAgreementsForm').on('click', '.tenancy-agreement-delete-btn', deleteTenancyAgreement);
    $("#TenancyProcessAgreementsForm").on("click", "#tenancyAgreementAdd", addTenancyAgreement);
});