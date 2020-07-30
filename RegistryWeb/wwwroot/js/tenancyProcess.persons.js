﻿$(function () {

    function tenancyPersonFillModal(tenancyPersonElem, action) {
        var modal = $("#personModal");
        var fields = tenancyPersonElem.find("input, select, textarea");
        var modalFields = modal.find("input, select, textarea");
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
    });

    $("#personModal").on("click", "#savePersonModalBtn", function (e) {
        var modal = $("#personModal");
        var form = $("#TenancyProcessPersonsModalForm");
        var isValid = form.valid();
        if (isValid) {
            tenancyPersonCorrectSnp(form);
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

    function deleteTenancyPerson(e) {
        let isOk = confirm("Вы уверены что хотите удалить участника найма?");
        if (isOk) {
            let tenancyPersonElem = $(this).closest(".list-group-item");
            let idPerson = tenancyPersonElem.find("input[name^='IdPerson']").val();
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
                    }
                    else {
                        alert("Ошибка удаления!");
                    }
                }
            });
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

    $('#TenancyProcessPersonsForm').on('click', '.tenancy-person-delete-btn', deleteTenancyPerson);
    $("#TenancyProcessPersonsForm").on("click", "#tenancyPersonAdd", addTenancyPerson);
});