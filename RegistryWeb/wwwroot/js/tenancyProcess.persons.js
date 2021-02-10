﻿function getTenancyPersons() {
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

$(function () {

    function tenancyPersonFillModal(tenancyPersonElem, action) {
        var modal = $("#personModal");
        var fields = tenancyPersonElem.find("input, select, textarea");
        var modalFields = modal.find("input, select, textarea");
        modalFields.prop("disabled", "");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            modal.find("[name='Person." + name + "']").val($(elem).val());
        });
        if (action === "Details" || action === "Delete")
            modalFields.prop("disabled", "disabled");
        else
            modalFields.prop("disabled", "");
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

    $("#personModal").on("click", "#savePersonModalBtn", function (e) {
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

                        if ($("#TenancyProcessPersons").find('.list-group-item').length - 1 > 0)
                        {
                            $(".TenancyProcessPersonsbadge").text($("#TenancyProcessPersons").find('.list-group-item').length + 1);
                            $(".TenancyProcessPersonsbadge").css("display", "inline-block");
                        }

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

                            if ($("#TenancyProcessPersons").find('.list-group-item').length-1 > 0)
                                $(".TenancyProcessPersonsbadge").text($("#TenancyProcessPersons").find('.list-group-item').length - 1);
                            else 
                                $(".TenancyProcessPersonsbadge").css("display", "none");
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

        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyProcesses/AddTenancyPerson',
            data: { action },
            success: function (elem) {
                addingTenancyPersonElem = elem;
                tenancyPersonFillModal($(elem), action);
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
        tenancyPersonFillModal(tenancyPersonElem, action);
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

    if ($("#TenancyProcessPersons").find('.list-group-item').length-1 == 0)
        $(".TenancyProcessPersonsbadge").css("display", "none");
});