function getTenancyPersons() {
    var items = $("#TenancyProcessPersons .list-group-item").filter(function (idx, elem) {
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

function fillPaymentAccount(tenancyPersonElem, modal) {
    var idAccountElem = tenancyPersonElem.find("[name^='PaymentAccount_']")[0];
    var accountElem = modal.find(".rr-payment-account").find("[name|='Person.PaymentAccountVisible']")[0];
    var modalIdAccount = modal.find(".rr-payment-account").find("[name|='Person.PaymentAccount']")[0];
    modalIdAccount.value = idAccountElem.value;
    accountElem.value = "";
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/Claims/GetAccountInfo',
        dataType: 'json',
        data: { idAccount: parseInt(idAccountElem.value), type: "BKS" },
        success: function (data) {
            if (data.paymentAccount) {
                accountElem.value = data.paymentAccount;
            }
            return data;
        }
    });
}

$(function () {

    var currentTenancyPerson;

    function initAutocompletePaymentAccount(modal) {
        
        var accountElem = modal.find(".rr-payment-account").find("[name|='Person.PaymentAccountVisible']");
        var idAccountFormElem = modal.find(".rr-payment-account").find("[name|='Person.PaymentAccount']");
            
        accountElem.on("input", function () {
            idAccountFormElem.val(null);
        });

        accountElem.autocomplete({
            appendTo: "#paymentAccountContainer",
            source: function (request, response) {
                $.ajax({
                    type: 'POST',
                    url: window.location.origin + '/Claims/GetAccounts',
                    dataType: 'json',
                    data: { text: request.term, type: "BKS" },
                    success: function (data) {
                        response($.map(data, function (item) {
                            let account = { label: item.account, value: item.account, idAccount: item.idAccount };
                            if (data.length === 1) {
                                idAccountFormElem.val(account.idAccount);
                                accountElem.val(account.value);
                            }
                            return account;
                        }));
                    }
                });
            },            
            select: function (event, ui) {
                idAccountFormElem.val(ui.item.idAccount);
                accountElem.val(ui.item.value);
            },            
            minLength: 3
        });
    }

    function tenancyPersonFillModal(tenancyPersonElem, action, canEditAll, canEditEmailsOnly) {
        currentTenancyPerson = tenancyPersonElem;
        var modal = $("#personModal");
        var fields = tenancyPersonElem.find("input, select, textarea");
        var modalFields = modal.find("input, select, textarea");
        modalFields.prop("disabled", "");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            modal.find("[name='Person." + name + "']").val($(elem).val());
        });  
        if (action === "Details" || action === "Delete") {
            modalFields.prop("disabled", "disabled");
        } else
        if (canEditAll === "True") {
            modalFields.prop("disabled", "");
        } else if (canEditEmailsOnly === "True") {
            modalFields.prop("disabled", "disabled");
            modalFields.filter(function (idx, elem) { return $(elem).prop("name") === "Person.Email" || $(elem).prop("name") === "Person.PaymentAccountVisible"; }).prop("disabled", "");
        } else {
            modalFields.prop("disabled", "disabled");
        }        
        fillPaymentAccount(tenancyPersonElem, modal);
        initAutocompletePaymentAccount(modal);
    }

    function tenancyPersonFillElem(tenancyPersonElem) {
        var modal = $("#personModal");
        var fields = tenancyPersonElem.find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            var value = modal.find("[name='Person." + name + "']").val();
            $(elem).val(value);
            if (name === "ExcludeDate") {
                var markingElems = $(tenancyPersonElem).find("[id^='Surname'], [id^='Name'], [id^='Patronymic']");
                if (value !== "") {
                    markingElems.addClass("text-danger");
                } else {
                    markingElems.removeClass("text-danger");
                }
            }
            if (elem.tagName === "SELECT") {
                $(elem).selectpicker("refresh");
            }
        });
    }

    function getTenancyPerson(form) {
        var data = {};
        var fields = form.find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name");
            data[name] = $(elem).val();
        });
        data["Person.IdProcess"] = $("#TenancyProcessForm #TenancyProcess_IdProcess").val();
        return data;
    }

    function tenancyPersonToFormData(person) {
        var formData = new FormData();
        for (var field in person) {
            formData.append(field, person[field]);
        }
        return formData;
    }

    function tenancyPersonCorrectSnp(form) {
        $(form).find("#Person_Surname, #Person_Name, #Person_Patronymic").each(function (idx, elem) {
            var value = $(elem).val();
            if (value.length > 0) {
                value = value[0].toUpperCase() + value.substring(1);
                $(elem).val(value);
            }
        });
    }

    $("#personModal").on("show.bs.modal", function () {
        $(this).find("select").selectpicker("refresh");
    });

    $("#personModal").on("hide.bs.modal", function () {

        $(this).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
        $(this).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");

        var tenancyPersonElem = $('#TenancyProcessPersons .list-group-item[data-processing]');
        tenancyPersonElem.removeAttr("data-processing");
        addingTenancyPersonElem = undefined;

        $("#cancelDocumentIssudeByBtn").click();
    });

    function updateInsertTenancyPersonElem() {
        var modal = $("#personModal");
        let tenancyPersonElem = $('#TenancyProcessPersons .list-group-item[data-processing]');
        if (tenancyPersonElem.length > 0) {
            tenancyPersonFillElem(tenancyPersonElem);
            modal.modal('hide');
        } else {
            let list = $('#TenancyProcessPersons');
            list.find(".rr-list-group-item-empty").hide();
            let tenancyPersonToggle = $('#TenancyProcessPersonsForm .tenancy-process-toggler');
            if (!isExpandElemntArrow(tenancyPersonToggle)) // развернуть при добавлении, если было свернуто 
                tenancyPersonToggle.click();
            list.append(addingTenancyPersonElem);
            let tenancyPersonElem = $('#TenancyProcessPersons .list-group-item').last();
            tenancyPersonElem.find("select").selectpicker("render");
            tenancyPersonFillElem(tenancyPersonElem);
            modal.modal('hide');
            $([document.documentElement, document.body]).animate({
                scrollTop: $(tenancyPersonElem).offset().top
            }, 1000);
        }
    }

    function addCustomDocumentIssuedBy() {
        let personDocumentIssuedByElem = $("#Person_IdDocumentIssuedBy");
        let customDocumentIssuedBy = $("#CustomDocumentIssuedBy").val();
        if (!$("#CustomDocumentIssuedBy").valid()) return false;
        let code = 0;
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyPersons/AddDocumentIssuedBy',
            data: { documentIssuedByName: customDocumentIssuedBy },
            async: false,
            success: function (id) {
                code = id;
                if (id > 0) {
                    personDocumentIssuedByElem.append("<option value='" + id + "'>" + customDocumentIssuedBy + "</option>");
                    $("#cancelDocumentIssudeByBtn").click();
                    personDocumentIssuedByElem.val(id).selectpicker("refresh");
                } else
                    if (id === -3) {
                        $("#cancelDocumentIssudeByBtn").click();
                        var duplicateOption = personDocumentIssuedByElem.find("option").filter(function (idx, elem) {
                            return $(elem).text() === customDocumentIssuedBy;
                        });
                        var optionId = 0;
                        if (duplicateOption.length > 0) {
                            optionId = duplicateOption.prop("value");
                        } else {
                            alert('Произошла ошибка при сохранении органа, выдающего документы, удостоверяющие личность');
                        }
                        personDocumentIssuedByElem.val(optionId).selectpicker("refresh");
                        code = optionId;
                    } else {
                        alert('Произошла ошибка при сохранении органа, выдающего документы, удостоверяющие личность');
                        return false;
                    }
            }
        });
        return code > 0;
    }

    function tenancyPersonsCustomValidations(form, validator) {
        var isValid = true;

        var idKinshipElem = form.find("#Person_IdKinship");
        var excludeDateElem = form.find("#Person_ExcludeDate");
        if (idKinshipElem.val() === "1" && excludeDateElem.val() === "") {
            var tenantAlreadyExists = false;
            var persons = $('#TenancyProcessPersons .list-group-item');
            persons.each(function (idx, elem) {
                if ($(elem).attr("data-processing") === "edit") return;
                var personIdKinship = $(elem).find("[id^='IdKinship']").val();
                var personExcludeDate = $(elem).find("[id^='ExcludeDate']").val();
                if (personIdKinship === "1" && personExcludeDate === "") {
                    tenantAlreadyExists = true;
                }
            });
            if (tenantAlreadyExists) {
                error = {};
                error[idKinshipElem.attr("name")] = "Для добавления нового нанимателя необходимо исключить или удалить предыдущего";
                validator.showErrors(error);
                isValid = false;
            } else {
                clearValidationError(idKinshipElem);
                removeErrorFromValidator(validator, idKinshipElem);
            }
        }

        return isValid;
    }

    function updateCountPersonsBadge() {
        var form = $('#TenancyProcessPersonsForm');
        var badge = form.find(".rr-count-badge");
        var count = $('#TenancyProcessPersonsForm').find('.list-group-item').length - 1;
        var activeCount = $('#TenancyProcessPersonsForm').find('.list-group-item').filter(function (idx, elem) {
            return $(elem).find('input[id^="ExcludeDate_"]').val() === "";
        }).length;
        if (count > 0) {
            badge.text(activeCount + " / " + count);
            badge.css("display", "inline-block");
        }
        else {
            badge.text('');
            badge.css("display", "none");
        }
    }

    $("#personModal").on("click", "#savePersonModalBtn", function (e) {
        var idAccountElem = currentTenancyPerson.find("[name^='PaymentAccount_']");
        var idAccountFormElem = $("#personModal").find(".rr-payment-account").find("[name^='PaymentAccountHidden']");
        idAccountElem.val(idAccountFormElem.val());
        let action = $('#TenancyProcessPersons').data('action');
        var form = $("#TenancyProcessPersonsModalForm");
        var validator = form.validate();
        var isValid = form.valid();
        if (!tenancyPersonsCustomValidations(form, validator)) {
            isValid = false;
        }

        if (isValid) {
            if (isCustomDocumentIssuedBy) {
                if (!addCustomDocumentIssuedBy())
                    return;
            }
            tenancyPersonCorrectSnp(form);
            if (action === "Create") {
                updateInsertTenancyPersonElem();
                return;
            }
            let tenancyPerson = tenancyPersonToFormData(getTenancyPerson(form));
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/TenancyPersons/SavePerson',
                data: tenancyPerson,
                processData: false,
                contentType: false,
                success: function (tenancyPersonReturn) {
                    if (tenancyPersonReturn.idPerson > 0) {
                        form.find("[name='Person.IdPerson']").val(tenancyPersonReturn.idPerson);
                        updateInsertTenancyPersonElem();
                        updateCountPersonsBadge();

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

    function deleteTenancyPerson(e) {
        let isOk = confirm("Вы уверены что хотите удалить участника найма?");
        if (isOk) {
            let tenancyPersonElem = $(this).closest(".list-group-item");
            let idPerson = tenancyPersonElem.find("input[name^='IdPerson']").val();
            if (idPerson === "0") {
                tenancyPersonElem.remove();
                if ($("#TenancyProcessPersons .list-group-item").length === 1) {
                    $("#TenancyProcessPersons .rr-list-group-item-empty").show();
                }
            } else {
                $.ajax({
                    async: false,
                    type: 'POST',
                    url: window.location.origin + '/TenancyPersons/DeletePerson',
                    data: { idPerson: idPerson },
                    success: function (ind) {
                        if (ind === 1) {
                            tenancyPersonElem.remove();
                            if ($("#TenancyProcessPersons .list-group-item").length === 1) {
                                $("#TenancyProcessPersons .rr-list-group-item-empty").show();
                            }
                            
                            updateCountPersonsBadge();
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

    var addingTenancyPersonElem = undefined;

    function addTenancyPerson(e) {
        let action = $('#TenancyProcessPersons').data('action');
        let canEditAll = $("#TenancyProcessPersons").data("can-edit-all");
        let canEditEmailsOnly = $("#TenancyProcessPersons").data("can-edit-emails-only");

        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyProcesses/AddTenancyPerson',
            data: { action },
            success: function (elem) {
                addingTenancyPersonElem = elem;
                tenancyPersonFillModal($(elem), action, canEditAll, canEditEmailsOnly);
                var modal = $("#personModal");
                modal.modal('show');
            }
        });
        e.preventDefault();
    }

    $('body').on('click', '.tenancy-person-open-btn, .tenancy-person-edit-btn', function (e) {
        var tenancyPersonElem = $(this).closest('.list-group-item');
        if (tenancyPersonElem.find("input[name^='IdPerson']").val() === "0") {
            tenancyPersonElem.attr("data-processing", "create");
        } else {
            tenancyPersonElem.attr("data-processing", "edit");
        }
        var action = $("#TenancyProcessPersons").data("action");
        var canEditAll = $("#TenancyProcessPersons").data("can-edit-all");
        var canEditEmailsOnly = $("#TenancyProcessPersons").data("can-edit-emails-only");
        tenancyPersonFillModal(tenancyPersonElem, action, canEditAll, canEditEmailsOnly);
        var modal = $("#personModal");
        modal.modal('show');
        e.preventDefault();
    });

    var isCustomDocumentIssuedBy = false;

    $("#addDocumentIssudeByBtn").on('click', function (e) {
        $("#addDocumentIssudeByBtn").hide();
        $("#cancelDocumentIssudeByBtn").show();
        $("#CustomDocumentIssuedBy").show();
        $("#Person_IdDocumentIssuedBy").closest(".bootstrap-select").hide();
        isCustomDocumentIssuedBy = true;
        e.preventDefault();
    });

    $("#cancelDocumentIssudeByBtn").on('click', function (e) {
        $("#addDocumentIssudeByBtn").show();
        $("#cancelDocumentIssudeByBtn").hide();
        $("#CustomDocumentIssuedBy").val("").hide();
        $("#Person_IdDocumentIssuedBy").closest(".bootstrap-select").show();
        isCustomDocumentIssuedBy = false;
        var customDocumentIssuedBy = $("#CustomDocumentIssuedBy").closest(".form-group");
        customDocumentIssuedBy.find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
        customDocumentIssuedBy.find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
        e.preventDefault();
    });

    $('#TenancyProcessPersonsForm').on('click', '.tenancy-person-delete-btn', deleteTenancyPerson);
    $("#TenancyProcessPersonsForm").on("click", "#tenancyPersonAdd", addTenancyPerson);
});