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
        else {
            modalFields.prop("disabled", "");
            if ($("#Agreement_Type_ProlongUntilDismissal").is(":checked")) {
                $("#Agreement_Type_ProlongEndDate").prop("disabled", "disabled");
            }
            if ($("#Agreement_Type_TenantExclude").is(":checked")) {
                $("#Agreement_Type_TenantNewIdKinship").prop("disabled", "disabled").selectpicker("refresh");
                $("#Agreement_Type_TenantDeath").prop("disabled", "disabled");
            }
            if ($("#Agreement_Type_TenantDeath").is(":checked")) {
                $("#Agreement_Type_TenantNewIdKinship").prop("disabled", "disabled").selectpicker("refresh");
                $("#Agreement_Type_TenantExclude").prop("disabled", "disabled");
            }
            $("#agreementModal #Agreement_Type_Point").val("");
            $("#agreementModal #Agreement_Type_SubPoint").val("");
            $("#agreementModal #Agreement_Type_OldKinship").val("");
            $("#agreementModal #Agreement_Type_NewKinship").val("");
            $("#Agreement_Type_TenancyPersonsChangeKinship option[value]").remove();
            $("#Agreement_Type_TenancyPersonsWithoutTenant option[value]").remove();
            $("#Agreement_Type_TenancyPersons option[value]").remove();
            $("#Agreement_Type_Tenant").val("").attr("data-id", "").attr("data-guid", "").attr("data-birthdate", "");
            $("#Agreement_Type_Tenant").prop("disabled", "disabled");
            var personsElems = $("#TenancyProcessPersons .list-group-item").filter(function (idx, elem) {
                return !$(elem).hasClass("rr-list-group-item-empty");
            });
            personsElems.each(function (idx, elem) {
                var excludeElem = $(elem).find("input[id^='ExcludeDate']");
                if (excludeElem.val() !== "" && excludeElem.val() !== null) return;
                var personOption = createPersonOptionByElem($(elem));
                $("#Agreement_Type_TenancyPersons").append(personOption);
                var kinshipElem = $(elem).find("select[id^='IdKinship']");
                if (kinshipElem.val() === "1") {
                    var surname = $(elem).find("input[id^='Surname']").val();
                    var name = $(elem).find("input[id^='Name']").val();
                    var patronymic = $(elem).find("input[id^='Patronymic']").val();
                    var idPersonElem = $(elem).find("input[id^='IdPerson']");
                    $("#Agreement_Type_Tenant").val(surname + " " + name + (patronymic !== "" ? " " + patronymic : ""));
                    $("#Agreement_Type_Tenant").attr("data-id", idPersonElem.val());
                    $("#Agreement_Type_Tenant").attr("data-guid", idPersonElem.attr("id").split("_")[1]);
                    $("#Agreement_Type_Tenant").attr("data-birthdate", formatDate($(elem).find("input[id^='DateOfBirth']").val()));
                } else {
                    $("#Agreement_Type_TenancyPersonsWithoutTenant").append(personOption);
                    $("#Agreement_Type_TenancyPersonsChangeKinship").append(createPersonOptionByElem($(elem), true));
                }
            });
        }
    }

    function createPersonOptionByElem(personElem, isChangeKinship = false) {
        var idPerson = personElem.find("input[id^='IdPerson']").val();
        var guid = personElem.find("input[id^='IdPerson']").prop("id").split("_")[1];
        var surname = personElem.find("input[id^='Surname']").val();
        var name = personElem.find("input[id^='Name']").val();
        var patronymic = personElem.find("input[id^='Patronymic']").val();
        var birthDate = formatDate(personElem.find("input[id^='DateOfBirth']").val());
        var kinshipElem = personElem.find("select[id^='IdKinship']");
        var kinship = kinshipElem.find("option[value='" + kinshipElem.val() + "']").text();
        return "<option data-surname='" + surname + "' data-name='" + name + "'" +
            " data-patronymic='" + patronymic + "' data-birthdate='" + birthDate + "'" +
            " data-id-kinship='" + kinshipElem.val() + "' data-kinship='" + kinship + "'" +
            (isChangeKinship ? " data-id-new-kinship='' data-new-kinship=''" : "") +
            " data-id='" + idPerson + "' value='" + guid + "'>" +
            surname + " " + name + (patronymic !== "" ? " " + patronymic : "") +
            ((birthDate !== "" && birthDate !== null) ? " (" + birthDate + " г.р.)" : "") +
            "</option>";
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
        $("#saveAgreementModalBtn").prop("disabled", "");
    });

    $("#agreementModal").on("hide.bs.modal", function () {

        $(this).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
        $(this).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");

        var tenancyAgreementElem = $('#TenancyProcessAgreements .list-group-item[data-processing]');
        tenancyAgreementElem.removeAttr("data-processing");
        addingTenancyAgreementElem = undefined;
        modifications = [];
        $("#agreementModal #Agreement_AutomateOperations").empty();
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

    function updateExcludePersonOnClient(currentOperation) {
        var excludeDateElem = $("#TenancyProcessPersons").find("input[id='ExcludeDate_" + currentOperation.Info.Guid + "']");
        var personElem = excludeDateElem.closest(".list-group-item");
        var markingElems = $(personElem).find("[id^='Surname'], [id^='Name'], [id^='Patronymic']");
        markingElems.addClass("text-danger");
        excludeDateElem.val($("#agreementModal #Agreement_AgreementDate").val());
    }

    function executeAutomateOperationExcludePerson(currentOperation, action, operations, onSuccess, onError) {
        if (action === "Create") {
            updateExcludePersonOnClient(currentOperation);
            currentOperation.Checkbox.prop("checked", false);
            executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
            return;
        }
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyPersons/UpdateExcludeDate',
            data: { idPerson: currentOperation.Info.IdPerson, excludeDate: $("#agreementModal #Agreement_AgreementDate").val() },
            async: false,
            success: function (error) {
                if (error.code === 0) {
                    updateExcludePersonOnClient(currentOperation);
                    currentOperation.Checkbox.prop("checked", false);
                    executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
                } else {
                    onError("Во время исключения участника найма произошла ошибка: "+error.text);
                }
            }
        });
    }

    function updateIncludePersonOnClient(elem, currentOperation) {
        let list = $('#TenancyProcessPersons');
        list.find(".rr-list-group-item-empty").hide();
        list.append(elem);
        let tenancyPersonElem = $('#TenancyProcessPersons .list-group-item').last();
        tenancyPersonElem.find("select").selectpicker("render");
        tenancyPersonElem.find("input[id^='IdPerson']").val(currentOperation.Info.IdPerson);
        tenancyPersonElem.find("input[id^='IdDocumentType']").val(1);
        tenancyPersonElem.find("input[id^='Surname']").val(currentOperation.Info.Surname);
        tenancyPersonElem.find("input[id^='Name']").val(currentOperation.Info.Name);
        tenancyPersonElem.find("input[id^='Patronymic']").val(currentOperation.Info.Patronymic);
        tenancyPersonElem.find("input[id^='DateOfBirth']").val(parseDate(currentOperation.Info.BirthDate));
        tenancyPersonElem.find("select[id^='IdKinship']").val(currentOperation.Info.IdKinship).selectpicker("refresh");
    }

    function executeAutomateOperationIncludePerson(currentOperation, action, operations, onSuccess, onError) {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyProcesses/AddTenancyPerson',
            data: { action },
            success: function (elem) {
                if (action === "Create") {
                    currentOperation.Info.IdPerson = 0;
                    updateIncludePersonOnClient(elem, currentOperation);
                    currentOperation.Checkbox.prop("checked", false);
                    executeAutomateOperationsRecursive(action, operations, onSuccess, onError);     
                    return;
                }

                let tenancyPerson = {
                    "Person.IdProcess": $("#TenancyProcessPersons").data("id"),
                    "Person.Surname": currentOperation.Info.Surname,
                    "Person.Name": currentOperation.Info.Name,
                    "Person.Patronymic": currentOperation.Info.Patronymic,
                    "Person.DateOfBirth": parseDate(currentOperation.Info.BirthDate),
                    "Person.IdKinship": currentOperation.Info.IdKinship,
                    "Person.IdDocumentType": 1
                };

                $.ajax({
                    type: 'POST',
                    url: window.location.origin + '/TenancyPersons/SavePerson',
                    data: tenancyPerson,
                    success: function (tenancyPersonReturn) {
                        if (tenancyPersonReturn.idPerson > 0) {
                            currentOperation.Info.IdPerson = tenancyPersonReturn.idPerson;
                            updateIncludePersonOnClient(elem, currentOperation);
                            currentOperation.Checkbox.prop("checked", false);
                            executeAutomateOperationsRecursive(action, operations, onSuccess, onError);                
                        } else {
                            onError("Во время добавления участника найма «" +
                                tenancyPerson["Person.Surname"] + " " + tenancyPerson["Person.Name"] +
                                (tenancyPerson["Person.Patronymic"] !== "" ? " " + tenancyPerson["Person.Patronymic"] : "") + "» произошла непредвиденная ошибка");
                        }
                    }
                });
            }
        });
    }

    function addProlongOnClient(elem, currentOperation) {
        let list = $('#TenancyProcessRentPeriods');
        list.find(".rr-list-group-item-empty").hide();
        list.append(elem);
        let rentPeriodElem = $('#TenancyProcessRentPeriods .list-group-item').last();
        rentPeriodElem.find("input[name^='IdRentPeriod']").val(currentOperation.Info.IdRentPeriod);
        rentPeriodElem.find("input[id^='BeginDate']").val(parseDate(currentOperation.Info.OldStartPeriod));
        rentPeriodElem.find("input[id^='EndDate']").val(parseDate(currentOperation.Info.OldEndPeriod));
        rentPeriodElem.find("input[id^='UntilDismissal']").prop("checked", currentOperation.Info.OldUntilDismissal);
    }

    function updateRentPeriodOnClient(currentOperation) {
        var startDate = parseDate(currentOperation.Info.StartPeriod);
        var endDate = parseDate(currentOperation.Info.EndPeriod);
        var untilDismissal = currentOperation.Info.UntilDismissal;
        $("#TenancyProcess_BeginDate").val(startDate);
        $("#TenancyProcess_UntilDismissal").prop("checked", untilDismissal);
        if (untilDismissal) {
            $("#TenancyProcess_EndDate").val("");
            $("#TenancyProcess_EndDate").prop("disabled", "disabled");
        } else {
            $("#TenancyProcess_EndDate").val(endDate);
            $("#TenancyProcess_EndDate").prop("disabled", "");
        }
    }

    function updateRentPeriod(currentOperation, onSuccess, onError) {
        var startDate = parseDate(currentOperation.Info.StartPeriod);
        var endDate = parseDate(currentOperation.Info.EndPeriod);
        var untilDismissal = currentOperation.Info.UntilDismissal;
        var idProcess = $("#TenancyProcess_IdProcess").val();
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyProcesses/UpdateRentPeriod',
            data: { idProcess: idProcess, beginDate: startDate, endDate: endDate, untilDismissal: untilDismissal },
            success: function (error) {
                if (error.code === 0) {
                    updateRentPeriodOnClient(currentOperation);
                    onSuccess();
                } else {
                    onError("Во время обновления периода найма произошла ошибка: "+error.text);
                }
            }
        });
    }

    function executeAutomateOperationProlong(currentOperation, action, operations, onSuccess, onError) {
        var startDate = parseDate(currentOperation.Info.StartPeriod);
        var endDate = parseDate(currentOperation.Info.EndPeriod);
        var oldStartDate = parseDate(currentOperation.Info.OldStartPeriod);
        var oldEndDate = parseDate(currentOperation.Info.OldEndPeriod);
        if (startDate === "" && endDate === "" && currentOperation.Info.UntilDismissal === false &&
            oldStartDate === "" && oldEndDate === "" && currentOperation.Info.OldUntilDismissal === false) {
            executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
            return;
        }
        
        var equalPeriod = $("#TenancyProcessRentPeriods .list-group-item").filter(function (idx, elem) {
            return !$(elem).hasClass("rr-list-group-item-empty") &&
                $(elem).find("input[id^='BeginDate']").val() === oldStartDate &&
                $(elem).find("input[id^='EndDate']").val() === oldEndDate &&
                $(elem).find("input[id^='UntilDismissal']").is(":checked") === currentOperation.Info.OldUntilDismissal;
        });
        if (equalPeriod.length > 0) {
            updateRentPeriod(currentOperation, function () {
                currentOperation.Checkbox.prop("checked", false);
                executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
            }, function (error) {
                onError(error);
            });
            return;
        }

        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyProcesses/AddRentPeriod',
            data: { action },
            success: function (elem) {
                if (action === "Create") {
                    currentOperation.Info.IdRentPeriod = 0;
                    if (oldStartDate !== "" || oldEndDate !== "" || currentOperation.Info.OldUntilDismissal !== false)
                        addProlongOnClient(elem, currentOperation);

                    updateRentPeriodOnClient(currentOperation);
                    currentOperation.Checkbox.prop("checked", false);
                    executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
                    return;
                }

                let rentPeriod = {
                    "RentPeriod.IdProcess": $("#TenancyProcessRentPeriods").data("id"),
                    "RentPeriod.BeginDate": oldStartDate,
                    "RentPeriod.EndDate": oldEndDate,
                    "RentPeriod.UntilDismissal": currentOperation.Info.OldUntilDismissal
                };

                if (oldStartDate !== "" || oldEndDate !== "" || currentOperation.Info.OldUntilDismissal !== false) {
                    $.ajax({
                        type: 'POST',
                        url: window.location.origin + '/TenancyRentPeriods/SaveRentPeriod',
                        data: rentPeriod,
                        success: function (rentPeriodReturn) {
                            if (rentPeriodReturn.idRentPeriod > 0) {
                                currentOperation.Info.IdRentPeriod = rentPeriodReturn.idRentPeriod;
                                addProlongOnClient(elem, currentOperation);
                                updateRentPeriod(currentOperation, function () {
                                    currentOperation.Checkbox.prop("checked", false);
                                    executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
                                }, function () {
                                    onError();
                                });
                            } else {
                                onError("Во время во время продления найма произошла непредвиденная ошибка");
                            }
                        }
                    });
                } else {
                    updateRentPeriod(currentOperation, function () {
                        currentOperation.Checkbox.prop("checked", false);
                        executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
                    }, function () {
                        onError();
                    });
                }
            }
        });
    }

    function updateIdKinshipPersonOnClient(currentOperation) {
        var idKinshipDateElem = $("#TenancyProcessPersons").find("select[id='IdKinship_" + currentOperation.Info.Guid + "']");
        idKinshipDateElem.val(currentOperation.Info.IdKinship).selectpicker("refresh");
    }

    function executeAutomateOperationChangeKinship(currentOperation, action, operations, onSuccess, onError) {
        if (action === "Create") {
            updateIdKinshipPersonOnClient(currentOperation);
            currentOperation.Checkbox.prop("checked", false);
            executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
            return;
        }
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyPersons/UpdateIdKinship',
            data: { idPerson: currentOperation.Info.IdPerson, idKinship: currentOperation.Info.IdKinship },
            async: false,
            success: function (error) {
                if (error.code === 0) {
                    updateIdKinshipPersonOnClient(currentOperation);
                    currentOperation.Checkbox.prop("checked", false);
                    executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
                } else {
                    onError("Во время исключения участника найма произошла ошибка: " + error.text);
                }                
            }
        });
        executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
    }

    function executeAutomateOperationsRecursive(action, operations, onSuccess, onError) {
        if (operations.length === 0) {
            onSuccess();
            return;
        }
        var operation = operations.pop();
        switch (operation.Operation) {
            case "ExcludePerson":
                executeAutomateOperationExcludePerson(operation, action, operations, onSuccess, onError);
                break;
            case "IncludePerson":
                executeAutomateOperationIncludePerson(operation, action, operations, onSuccess, onError);
                break;
            case "Prolong":
                executeAutomateOperationProlong(operation, action, operations, onSuccess, onError);
                break;
            case "ChangeKinship":
                executeAutomateOperationChangeKinship(operation, action, operations, onSuccess, onError);
                break;
            default:
                onError("Неизвестный тип автоматической операции");
                break;
        }
    }

    function executeAutomateOperations(action, operations, onSuccess, onError) {
        operations = $(operations).filter(function (idx, elem) {
            return $(elem.Checkbox).is(":checked");
        }).toArray().sort(function () { return 1; });
        executeAutomateOperationsRecursive(action, operations, onSuccess, onError);
    }

    $("#agreementModal").on("click", "#saveAgreementModalBtn", function (e) {
        e.preventDefault();
        $("#saveAgreementModalBtn").prop("disabled", "disabled");
        let action = $('#TenancyProcessAgreements').data('action');
        var form = $("#TenancyProcessAgreementsModalForm");
        form.find("#Agreement_Type").val("").selectpicker("refresh").change();
        var isValid = form.valid();

        if (isValid) {
            executeAutomateOperations(action, modifications, function () {
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

                            var flag = $("#TenancyProcessAgreementsForm").find("rr-list-group-item-empty") != null ? true : false;
                            countBadges('#TenancyProcessAgreementsForm', flag);

                        } else {
                            alert('Произошла ошибка при сохранении');
                        }
                    }
                });
            }, function (error) {
                alert(error);
                $("#saveAgreementModalBtn").prop("disabled", "");
            });
        } else {
            refreshSelectpickerValidationBorders(form);
            $([document.documentElement, document.body]).animate({
                scrollTop: form.find(".input-validation-error").first().offset().top - 35
            }, 1000);
            $("#saveAgreementModalBtn").prop("disabled", "");
        }
    });

    function refreshSelectpickerValidationBorders(form) {
        form.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
    }

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

                            var flag = $("#TenancyProcessAgreementsForm").find("rr-list-group-item-empty") != null != 0 ? true : false;
                            countBadges('#TenancyProcessAgreementsForm', flag);

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
                var agreementContent = modal.find("#Agreement_AgreementContent").val();
                modal.find("#Agreement_AgreementContent").val(reformAgreementContent(agreementContent));

                modal.modal('show');
            }
        });
        e.preventDefault();
    }

    function reformAgreementContent(agreementContent) {
        var tenancyBaseInfo = getTenancyBaseInfo();
        var rentObjects = getRentObjects();
        rentObjects = rentObjects.toArray().sort(sortRentObjects);
        var rentAddress = buildFullRentAddress(rentObjects);

        agreementContent = agreementContent.replace("{0}", tenancyBaseInfo.RegistrationNumber);
        agreementContent = agreementContent.replace("{1}", tenancyBaseInfo.RegistrationDate);
        agreementContent = agreementContent.replace("{2}", tenancyBaseInfo.RentTypeGenetive);
        agreementContent = agreementContent.replace("{3}", rentAddress);

        return agreementContent;
    }

    function getTenancyBaseInfo() {
        var idRentType = $('#TenancyProcessForm #TenancyProcess_IdRentType').val();
        var rentTypeGenetive = $('#TenancyProcessForm input[name="RentTypeGenetive"]').filter(function (idx, elem) { return $(elem).val() === idRentType; }).first().data("genetive");
        if (rentTypeGenetive === undefined) {
            rentTypeGenetive = "";
        }
        var regNumber = $("#TenancyProcessForm #TenancyProcess_RegistrationNum").val();
        var regDate = formatDate($("#TenancyProcessForm #TenancyProcess_RegistrationDate").val());
        return {
            IdRentType: idRentType,
            RentTypeGenetive: rentTypeGenetive,
            RegistrationNumber: regNumber,
            RegistrationDate: regDate
        };
    }

    function getRentObjects() {
        var rentObjects = $("#TenancyProcessRentObjects .list-group-item").filter(function (idx, elem) {
            return !$(elem).hasClass("rr-list-group-item-empty");
        }).map(function (idx, elem) {
            var streetElem = $(elem).find("select[name^='IdStreet']");
            var buildingElem = $(elem).find("select[name^='IdBuilding']");
            var premiseElem = $(elem).find("select[name^='IdPremises']");
            var subPremiseElem = $(elem).find("select[name^='IdSubPremises']");
            return {
                Street: streetElem.find("option[value='" + streetElem.val() + "']").text(),
                Building: buildingElem.find("option[value='" + buildingElem.val() + "']").text(),
                Premise: premiseElem.find("option[value='" + premiseElem.val() + "']").text(),
                SubPremise: subPremiseElem.find("option[value='" + subPremiseElem.val() + "']").text()
            };
            });
        return rentObjects;
    }

    function sortRentObjects(a, b) {
        if (a.Street < b.Street) return -1;
        if (a.Street > b.Street) return 1;
        if (a.Building < b.Building) return -1;
        if (a.Building > b.Building) return 1;
        if (a.Premise < b.Premise) return -1;
        if (a.Premise > b.Premise) return 1;
        if (a.SubPremise < b.SubPremise) return -1;
        if (a.SubPremise > b.SubPremise) return 1;
        return 0;
    }

    function groupRentObjects(rentObjects) {
        var currentStreet = undefined;
        var currentBuilding = undefined;
        var currentPremise = undefined;
        var currentSubPremise = undefined;
        var groupedRentObjects = [];
        for (var i = 0; i < rentObjects.length; i++) {
            var rentObject = rentObjects[i];
            var street = rentObject.Street;
            var building = rentObject.Building;
            var premise = rentObject.Premise;
            var subPremise = rentObject.SubPremise;
            if (street === "" || building === "") continue;
            if (street !== currentStreet) {
                currentStreet = street;
                currentBuilding = undefined;
                currentPremise = undefined;
                currentSubPremise = undefined;
                groupedRentObjects.push({ Street: currentStreet, Buildings: [] });
            }
            if (building !== currentBuilding) {
                currentBuilding = building;
                currentPremise = undefined;
                currentSubPremise = undefined;
                for (let sIndex in groupedRentObjects) {
                    if (groupedRentObjects[sIndex].Street === currentStreet) {
                        groupedRentObjects[sIndex].Buildings.push({ Building: currentBuilding, Premises: [] });
                    }
                }
            }
            if (premise !== currentPremise) {
                currentPremise = premise;
                currentSubPremise = undefined;
                if (currentPremise !== "") {
                    for (let sIndex in groupedRentObjects) {
                        if (groupedRentObjects[sIndex].Street === street) {
                            for (let bIndex in groupedRentObjects[sIndex].Buildings) {
                                if (groupedRentObjects[sIndex].Buildings[bIndex].Building === currentBuilding) {
                                    groupedRentObjects[sIndex].Buildings[bIndex].Premises.push({ Premise: currentPremise, SubPremises: [] });
                                }
                            }
                        }
                    }
                }
            }
            if (subPremise !== currentSubPremise) {
                currentSubPremise = subPremise;
                if (currentSubPremise !== "") {
                    for (let sIndex in groupedRentObjects) {
                        if (groupedRentObjects[sIndex].Street === street) {
                            for (let bIndex in groupedRentObjects[sIndex].Buildings) {
                                if (groupedRentObjects[sIndex].Buildings[bIndex].Building === currentBuilding) {
                                    for (let pIndex in groupedRentObjects[sIndex].Buildings[bIndex].Premises) {
                                        if (groupedRentObjects[sIndex].Buildings[bIndex].Premises[pIndex].Premise === currentPremise) {
                                            groupedRentObjects[sIndex].Buildings[bIndex]
                                                .Premises[pIndex].SubPremises.push({ SubPremise: currentSubPremise });
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }
        return groupedRentObjects;
    }

    function buildFullRentAddress(rentObjects) {
        var fullRentAddress = "Российская Федерация, Иркутская область, ";
        var groupedRentObjects = groupRentObjects(rentObjects);
        for (var i = 0; i < groupedRentObjects.length; i++) {
            var street = groupedRentObjects[i].Street;
            if (street.indexOf("Иркутская обл., ") !== -1) {
                street = street.replace("Иркутская обл., ", "");
            }
            street = street.replace("г.", "город").replace("ж.р.", "жилой район").replace("мкр.", "микрорайон").replace("ул.", "улица")
                .replace("пер.", "переулок").replace("б-р.", "бульвар").replace("пр-кт.", "проспект").replace("кв-л.", "квартал")
                .replace("проезд.", "проезд").replace("ст.", "станция").replace("тер.", "территория");
            fullRentAddress += street;
            if (groupedRentObjects[i].Buildings.length > 1) {
                fullRentAddress += ", дома ";
            } else if (groupedRentObjects[i].Buildings.length === 1) {
                fullRentAddress += ", дом ";
            }
            for (var j = 0; j < groupedRentObjects[i].Buildings.length; j++) {
                var building = groupedRentObjects[i].Buildings[j];
                fullRentAddress += building.Building;
                if (building.Premises.length > 1) {
                    fullRentAddress += ", квартиры ";
                } else if (building.Premises.length === 1) {
                    fullRentAddress += ", квартира ";
                }
                for (var k = 0; k < building.Premises.length; k++) {
                    var premise = building.Premises[k];
                    fullRentAddress += premise.Premise;

                    if (premise.SubPremises.length > 1) {
                        fullRentAddress += ", комнаты ";
                    } else if (premise.SubPremises.length === 1) {
                        fullRentAddress += ", комната ";
                    }
                    for (var l = 0; l < premise.SubPremises.length; l++) {
                        fullRentAddress += premise.SubPremises[l].SubPremise;
                        if (l < premise.SubPremises.length - 1) {
                            fullRentAddress += ", ";
                        }
                    }

                    if (k < building.Premises.length - 1) {
                        fullRentAddress += ", ";
                    }
                }

                if (j < groupedRentObjects[i].Buildings.length - 1) {
                    fullRentAddress += ", ";
                }
            }
            if (i < groupedRentObjects.length - 1) {
                fullRentAddress += ", ";
            }
        }
        return fullRentAddress;
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

    $("#agreementModal #Agreement_Type").on("change", function (e) {
        var id = $(this).val();
        if (id === "" || id === null)
            $("#Agreement_Type_Button").prop("disabled", "disabled");
        else
            $("#Agreement_Type_Button").prop("disabled", "");
        $("#agreementModal .rr-agreement-type-field").each(function (idx, elem) {
            $(elem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
            $(elem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
            var agreementTypeIds = $(elem).data("agreement-types").toString().split(",");
            if (agreementTypeIds.indexOf(id) !== -1) {
                $(elem).show();
            } else {
                $(elem).hide();
            }
        });
    });

    $("#agreementModal #Agreement_Type").change();

    $("#agreementModal #Agreement_Type_Button").on("click", function (e) {
        e.preventDefault();
        var isValid = $("#agreementModal .rr-agreement-type-field").find("input, select, textarea").valid();
        if (!isValid) {
            refreshSelectpickerValidationBorders($("#TenancyProcessAgreementsModalForm"));
            return;
        }
        var agreementType = $("#agreementModal #Agreement_Type").val();
        switch (agreementType) {
            case "0":
                var excludeTenantPersonInfo = getExcludeTenantPersonInfo();
                addExcludeTenantPersonInfoToContent(excludeTenantPersonInfo);
                addExcludeTenantPersonInfoToModifications(excludeTenantPersonInfo);
                break;
            case "1":
                var includeTenantPersonInfo = getIncludeTenantPersonInfo();
                addIncludeTenantPersonInfoToContent(includeTenantPersonInfo);
                addIncludeTenantPersonInfoToModifications(includeTenantPersonInfo);
                break;
            case "2":
                var explainPointInfo = getExplainPointInfo();
                addExplainPointInfoToContent(explainPointInfo);
                break;
            case "3":
                var terminateTenancyInfo = getTerminateTenancyInfo();
                addTerminateTenancyInfoToContent(terminateTenancyInfo);
                break;
            case "4":
                var prolongCommercialInfo = getProlongCommercialInfo();
                addProlongCommercialInfoToContent(prolongCommercialInfo);
                addProlongCommercialInfoToModifications(prolongCommercialInfo);
                break;
            case "5":
                var prolongSpecialInfo = getProlongSpecialInfo();
                addProlongSpecialInfoToContent(prolongSpecialInfo);
                addProlongSpecialInfoToModifications(prolongSpecialInfo);
                break;
            case "6":
                var changeTenantInfo = getChangeTenantInfo();
                addChangeTenantInfoToContent(changeTenantInfo);
                addChangeTenantInfoToModifications(changeTenantInfo);
                break;
            case "7":
                var changeKinshipsInfo = getChangeKinshipTenantsInfo();
                addChangeKinshipTenantsInfoToContent(changeKinshipsInfo);
                addChangeKinshipTenantsInfoToModifications(changeKinshipsInfo);
                break;
        }
    });

    var modifications = [];

    function formatTenantPerson(personInfo) {
        var tenant = "";
        tenant += personInfo.Surname + " " + personInfo.Name + (personInfo.Patronymic !== "" ? " " + personInfo.Patronymic : "");
        tenant += " - " + personInfo.Kinship;
        if (personInfo.BirthDate !== "") {
            tenant += ", " + personInfo.BirthDate + " г.р.";
        }
        tenant = tenant;
        return tenant;
    }

    function getExcludeTenantPersonInfo() {
        var personElem = $("#agreementModal #Agreement_Type_TenancyPersons");
        var personGuid = personElem.val();
        var personOption = personElem.find("option[value='" + personGuid + "']");
        return {
            IdPerson: personOption.data("id"),
            Guid: personGuid,
            Surname: personOption.data("surname"),
            Name: personOption.data("name"),
            Patronymic: personOption.data("patronymic"),
            Kinship: personOption.data("kinship"),
            BirthDate: personOption.data("birthdate"),
            Point: $("#agreementModal #Agreement_Type_Point").val(),
            SubPoint: $("#agreementModal #Agreement_Type_SubPoint").val()
        };
    }

    function addExcludeTenantPersonInfoToContent(personInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var content = contentElem.val();
        var contentLines = content.split("\n");
        var headerWildcard = "^\u200B.*?из договора исключить";
        if (personInfo.Point !== "") {
            headerWildcard = "^\u200B.*?из пункта " + personInfo.Point + " договора исключить";
        }
        if (personInfo.SubPoint !== "") {
            headerWildcard += " подпункт " + personInfo.SubPoint + " следующего содержания";
        }
        if (!isHeaderInserted(contentLines, headerWildcard)) {
            var nextPoint = getNextHeaderPoint(contentLines);
            var header = "\u200B" + nextPoint + ") из договора исключить";
            if (personInfo.Point !== "") {
                header = "\u200B" + nextPoint + ") из пункта " + personInfo.Point + " договора исключить";
            }
            if (personInfo.SubPoint !== "") {
                header += " подпункт " + personInfo.SubPoint + " следующего содержания";
            }
            header += ":";
            contentLines.push(header);
        }

        var tenant = "";
        if (personInfo.SubPoint !== "")
            tenant += personInfo.SubPoint + ". ";
        tenant += formatTenantPerson(personInfo);
        tenant = "«" + tenant + "»";
        contentLines = insertPoint(contentLines, tenant, headerWildcard);
        contentElem.val(contentLines.join("\n"));
    }

    function addExcludeTenantPersonInfoToModifications(personInfo) {
        var checkbox = insertAutomateOperationsCheckBox("Исключить участника найма «" + formatTenantPerson(personInfo) + "»");
        modifications.push({ Checkbox: checkbox, Operation: "ExcludePerson", Info: personInfo });
    }

    function getIncludeTenantPersonInfo() {
        var tenancyBaseInfo = getTenancyBaseInfo();
        var kinshipElem = $("#agreementModal #Agreement_Type_TenancyPersonIdKinship");
        var idKinship = kinshipElem.val();
        var kinshipOption = kinshipElem.find("option[value='" + idKinship + "']");
        var tenantElem = $("#agreementModal #Agreement_Type_Tenant");
        return {
            Surname: $("#agreementModal #Agreement_Type_TenancyPersonSurname").val(),
            Name: $("#agreementModal #Agreement_Type_TenancyPersonName").val(),
            Patronymic: $("#agreementModal #Agreement_Type_TenancyPersonPatronymic").val(),
            Kinship: kinshipOption.text(),
            IdKinship: idKinship,
            BirthDate: formatDate($("#agreementModal #Agreement_Type_TenancyPersonBirthDate").val()),
            Point: $("#agreementModal #Agreement_Type_Point").val(),
            SubPoint: $("#agreementModal #Agreement_Type_SubPoint").val(),
            RegistrationNum: tenancyBaseInfo.RegistrationNumber,
            RegistrationDate: tenancyBaseInfo.RegistrationDate,
            CurrentTenantIdPerson: tenantElem.attr("data-id"),
            CurrentTenantGuid: tenantElem.attr("data-guid"),
            CurrentTenant: tenantElem.val()
        };
    }

    function addIncludeTenantPersonInfoToContent(personInfo) {
        let contentElem = $("#agreementModal #Agreement_AgreementContent");
        let content = contentElem.val();
        let contentLines = content.split("\n");
        if (personInfo.IdKinship === "1") {
            let nextPoint = getNextHeaderPoint(contentLines);
            let header = "\u200B" + nextPoint + ") считать по договору";
            if (personInfo.RegistrationNum !== "") {
                header += " № " + personInfo.RegistrationNum;
            }
            if (personInfo.RegistrationDate !== "") {
                header += " от " + personInfo.RegistrationDate;
            }
            let tenant = personInfo.Surname + " " + personInfo.Name + (personInfo.Patronymic !== "" ? " " + personInfo.Patronymic : "");
            $.ajax({
                type: "GET",
                url: "/TenancyAgreements/GetSnpPartsCase?surname=" + personInfo.Surname + "&name=" + personInfo.Name + "&patronymic=" + personInfo.Patronymic +
                    "&padeg=VINITELN",
                async: false,
                success: function (data) {
                    tenant = data.snpAccusative;
                }
            });

            if (personInfo.BirthDate !== "") {
                tenant += " - " + personInfo.BirthDate + " г.р.";
            }
            header += " нанимателем - «" + tenant + "»";
            contentLines.push(header);
        } else {
            var headerWildcard = "^\u200B.*?договор дополнить";
            if (personInfo.Point !== "") {
                headerWildcard = "^\u200B.*?пункт " + personInfo.Point + " договора дополнить";
            }
            if (personInfo.SubPoint !== "") {
                headerWildcard += " подпунктом " + personInfo.SubPoint + " следующего содержания";
            }
            if (!isHeaderInserted(contentLines, headerWildcard)) {
                let nextPoint = getNextHeaderPoint(contentLines);
                let header = "\u200B" + nextPoint + ") договор дополнить";
                if (personInfo.Point !== "") {
                    header = "\u200B" + nextPoint + ") пункт " + personInfo.Point + " договора дополнить";
                }
                if (personInfo.SubPoint !== "") {
                    header += " подпунктом " + personInfo.SubPoint + " следующего содержания";
                }
                header += ":";
                contentLines.push(header);
            }

            var tenant = "";
            if (personInfo.SubPoint !== "")
                tenant += personInfo.SubPoint + ". ";
            tenant += formatTenantPerson(personInfo);
            tenant = "«" + tenant + "»";
            contentLines = insertPoint(contentLines, tenant, headerWildcard);
        }
        contentElem.val(contentLines.join("\n"));
    }

    function addIncludeTenantPersonInfoToModifications(personInfo) {
        if (personInfo.IdKinship === "1") {
            let checkbox = insertAutomateOperationsCheckBox("Исключить нанимателя «" + personInfo.CurrentTenant + "»");
            modifications.push({
                Checkbox: checkbox, Operation: "ExcludePerson", Info: {
                    IdPerson: personInfo.CurrentTenantIdPerson,
                    Guid: personInfo.CurrentTenantGuid
                }
            });
        }
        let checkbox = insertAutomateOperationsCheckBox("Включить нового участника найма «" + formatTenantPerson(personInfo) + "»");
        modifications.push({ Checkbox: checkbox, Operation: "IncludePerson", Info: personInfo });
    }

    function getExplainPointInfo() {
        return {
            Content: $("#agreementModal #Agreement_Type_PointContent").val(),
            Point: $("#agreementModal #Agreement_Type_Point").val(),
            SubPoint: $("#agreementModal #Agreement_Type_SubPoint").val()
        };
    }

    function addExplainPointInfoToContent(explainPointInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var content = contentElem.val();
        var contentLines = content.split("\n");
        var nextPoint = getNextHeaderPoint(contentLines);
        var header = "\u200B" + nextPoint + ") изложить";
        var pointHeader = "";
        if (explainPointInfo.Point !== "" && explainPointInfo.SubPoint === "") {
            pointHeader = " пункт " + explainPointInfo.Point;
        }
        if (explainPointInfo.SubPoint !== "" && explainPointInfo.Point === "") {
            pointHeader = " подпункт " + explainPointInfo.SubPoint;
        }
        if (explainPointInfo.SubPoint !== "" && explainPointInfo.Point !== "") {
            pointHeader = " подпункт " + explainPointInfo.SubPoint + " пункта " + explainPointInfo.Point;
        }
        header += pointHeader+" в новой редакции:";
        contentLines.push(header);

        pointContent = "«" + explainPointInfo.Content + "»";
        contentLines.push(pointContent);
        contentElem.val(contentLines.join("\n"));
    }

    function getTerminateTenancyInfo() {
        var tenancyBaseInfo = getTenancyBaseInfo();
        var rentObjects = getRentObjects();
        rentObjects = rentObjects.toArray().sort(sortRentObjects);
        var rentAddress = buildFullRentAddress(rentObjects);
        return {
            RegistrationNum: tenancyBaseInfo.RegistrationNumber,
            RegistrationDate: tenancyBaseInfo.RegistrationDate,
            RentTypeGenetive: tenancyBaseInfo.RentTypeGenetive,
            RentAddress: rentAddress,
            TerminateDate: formatDate($("#agreementModal #Agreement_Type_TenancyEndDate").val()),
            TerminateReason: $("#agreementModal #Agreement_Type_TenancyEndReason").val()
        };
    }

    function addTerminateTenancyInfoToContent(terminateTenancyInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var contentLines = [];

        var text = "1.1. По настоящему Соглашению Стороны договорились расторгнуть с " + terminateTenancyInfo.TerminateDate + " договор";
        contract = "";
        if (terminateTenancyInfo.RegistrationNum !== "") {
            contract += " № " + terminateTenancyInfo.RegistrationNum;
        }
        if (terminateTenancyInfo.RegistrationDate !== "") {
            contract += " от " + terminateTenancyInfo.RegistrationDate;
        }
        text += contract;
        if (terminateTenancyInfo.RentTypeGenetive !== "") {
            text += " "+terminateTenancyInfo.RentTypeGenetive;
        }
        text += " найма жилого помещения";
        if (terminateTenancyInfo.RentAddress !== "") {
            text += ", расположенного по адресу: " + terminateTenancyInfo.RentAddress;
        }
        text += ", (далее - договор) по " + terminateTenancyInfo.TerminateReason + ".";
        contentLines.push(text);
        text = "1.2. Обязательства, возникшие из указанного договора до момента расторжения, подлежат исполнению в соответствии с указанным договором. Стороны не имеют взаимных претензий по исполнению условий договора";
        text += contract;
        text += ".";
        contentLines.push(text);
        contentElem.val(contentLines.join("\n"));
    }

    function formatTenancyPeriod(prolongInfo) {
        var period = " на неопределенный период";
        if (prolongInfo.StartPeriod !== "" || prolongInfo.EndPeriod !== "" || prolongInfo.UntilDismissal) {
            period = "";
        } 
        if (prolongInfo.StartPeriod !== "") {
            period += " с " + prolongInfo.StartPeriod;
        }
        if (prolongInfo.EndPeriod !== "" && !prolongInfo.UntilDismissal) {
            period += " по " + prolongInfo.EndPeriod;
        }
        if (prolongInfo.UntilDismissal) {
            if (prolongInfo.StartPeriod === "")
                period = "";
            period += " на период трудовых отношений";
        }
        return period;
    }

    function getProlongCommercialInfo() {
        var tenancyBaseInfo = getTenancyBaseInfo();
        var reasonDocElem = $("#agreementModal #Agreement_Type_TenancyProlongRentReason");
        var reasonDocId = reasonDocElem.val();
        var reasonDocOption = reasonDocElem.find("option[value='" + reasonDocId + "']");
        return {
            RegistrationNum: tenancyBaseInfo.RegistrationNumber,
            RegistrationDate: tenancyBaseInfo.RegistrationDate,
            RentTypeGenetive: tenancyBaseInfo.RentTypeGenetive,
            ReasonDoc: reasonDocOption.text(),
            ReasonDocGenetive: reasonDocOption.data("genetive"),
            ReasonDocDate: formatDate($("#agreementModal #Agreement_Type_TenancyProlongRentReasonDate").val()),
            StartPeriod: formatDate($("#agreementModal #Agreement_Type_ProlongBeginDate").val()),
            EndPeriod: formatDate($("#agreementModal #Agreement_Type_ProlongEndDate").val()),
            UntilDismissal: $("#agreementModal #Agreement_Type_ProlongUntilDismissal").is(":checked"),
            PointExclude: $("#agreementModal #Agreement_Type_PointExclude").val(),
            OldStartPeriod: $("#TenancyProcess_BeginDate").val(),
            OldEndPeriod: $("#TenancyProcess_EndDate").val(),
            OldUntilDismissal: $("#TenancyProcess_UntilDismissal").is(":checked")
        };
    }

    function addProlongCommercialInfoToContent(prolongInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var contentLines = [];

        contentLines.push(getDefaultAgreementText());

        var text = "1) на основании " + prolongInfo.ReasonDocGenetive + " от " + prolongInfo.ReasonDocDate + " продлить срок действия договора";
        contract = "";
        if (prolongInfo.RegistrationNum !== "") {
            contract += " № " + prolongInfo.RegistrationNum;
        }
        if (prolongInfo.RegistrationDate !== "") {
            contract += " от " + prolongInfo.RegistrationDate;
        }
        text += contract;
        if (prolongInfo.RentTypeGenetive !== "") {
            text += " " + prolongInfo.RentTypeGenetive;
        }
        text += " найма жилого помещения";
        text += formatTenancyPeriod(prolongInfo) + ".";
        contentLines.push(text);
        if (prolongInfo.PointExclude !== "") {
            text = "2) пункт " + prolongInfo.PointExclude + " исключить.";
            contentLines.push(text);
        }
        contentElem.val(contentLines.join("\n"));
    }

    function addProlongCommercialInfoToModifications(prolongInfo) {
        var checkbox = insertAutomateOperationsCheckBox("Продлить найм" + formatTenancyPeriod(prolongInfo));
        modifications.push({ Checkbox: checkbox, Operation: "Prolong", Info: prolongInfo });
    }

    function getProlongSpecialInfo() {
        return {
            Point: $("#agreementModal #Agreement_Type_Point").val(),
            SubPoint: $("#agreementModal #Agreement_Type_SubPoint").val(),
            StartPeriod: formatDate($("#agreementModal #Agreement_Type_ProlongBeginDate").val()),
            EndPeriod: formatDate($("#agreementModal #Agreement_Type_ProlongEndDate").val()),
            UntilDismissal: $("#agreementModal #Agreement_Type_ProlongUntilDismissal").is(":checked"),
            OldStartPeriod: $("#TenancyProcess_BeginDate").val(),
            OldEndPeriod: $("#TenancyProcess_EndDate").val(),
            OldUntilDismissal: $("#TenancyProcess_UntilDismissal").is(":checked")
        };
    }

    function addProlongSpecialInfoToContent(prolongInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var content = contentElem.val();
        var contentLines = content.split("\n");

        var nextPoint = getNextHeaderPoint(contentLines);
        var headerWildcard = "^\u200B.*?изложить в новой редакции:";
        if (!isHeaderInserted(contentLines, headerWildcard)) {
            var header = "\u200B" + nextPoint + ") изложить в новой редакции:";
            contentLines.push(header);
        }

        var text = "";
        var pointHeader = "";
        if (prolongInfo.Point !== "" && prolongInfo.SubPoint === "") {
            pointHeader = "пункт " + prolongInfo.Point;
        }
        if (prolongInfo.SubPoint !== "" && prolongInfo.Point === "") {
            pointHeader = "подпункт " + prolongInfo.SubPoint;
        }
        if (prolongInfo.SubPoint !== "" && prolongInfo.Point !== "") {
            pointHeader = "подпункт " + prolongInfo.SubPoint + " пункта " + prolongInfo.Point;
        }
        if (pointHeader !== "")
            pointHeader = pointHeader + ": ";
        text += pointHeader;
        text += "«Срок найма жилого помещения устанавливается";
        text += formatTenancyPeriod(prolongInfo) + "».";

        contentLines = insertPoint(contentLines, text, headerWildcard);

        contentElem.val(contentLines.join("\n"));
    }

    function addProlongSpecialInfoToModifications(prolongInfo) {
        var checkbox = insertAutomateOperationsCheckBox("Продлить найм" + formatTenancyPeriod(prolongInfo));
        modifications.push({ Checkbox: checkbox, Operation: "Prolong", Info: prolongInfo });
    }

    function getChangeTenantInfo() {
        var kinshipElem = $("#agreementModal #Agreement_Type_TenantNewIdKinship");
        var idKinship = kinshipElem.val();
        var kinshipOption = kinshipElem.find("option[value='" + idKinship + "']");
        var personsElem = $("#agreementModal #Agreement_Type_TenancyPersonsWithoutTenant");
        var idPerson = personsElem.val();
        var personOption = personsElem.find("option[value='" + idPerson + "']");
        var tenantElem = $("#agreementModal #Agreement_Type_Tenant");
        return {
            CurrentTenantIdPerson: tenantElem.attr("data-id"),
            CurrentTenantGuid: tenantElem.attr("data-guid"),
            CurrentTenant: tenantElem.val(),
            CurrentTenantBirthDate: tenantElem.data("birthdate"),
            CurrentTenantNewIdKinship: idKinship,
            CurrentTenantNewKinship: kinshipOption.text(),
            NewTenantIdPerson: personOption.data("id"),
            NewTenantGuid: personOption.val(),
            NewTenantSurname: personOption.data("surname"),
            NewTenantName: personOption.data("name"),
            NewTenantPatronymic: personOption.data("patronymic"),
            NewTenantBirthDate: personOption.data("birthdate"),
            ExcludeCurrentTenant: $("#agreementModal #Agreement_Type_TenantExclude").is(":checked"),
            DeathCurrentTenant: $("#agreementModal #Agreement_Type_TenantDeath").is(":checked")
        };
    }

    function getChangeKinshipTenantsInfo() {
        return $("#agreementModal #Agreement_Type_TenancyPersonsChangeKinship option[value]")
            .filter(function (ind, elem) {
                return $(elem).attr("data-id-new-kinship") !== "" && $(elem).attr("data-id-new-kinship") !== undefined;
            }).map(function (ind, elem) {
                return {
                    IdPerson: $(elem).attr("data-id"),
                    Guid: $(elem).val(),
                    Tenant: $(elem).text(),
                    OldIdKinship: $(elem).data("id-kinship"),
                    OldKinship: $(elem).data("kinship"),
                    NewIdKinship: $(elem).attr("data-id-new-kinship"),
                    NewKinship: $(elem).attr("data-new-kinship")
                };
            });
    }

    function addChangeTenantInfoToContent(changeTenantInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var contentLines = [];
        var text = "";
        var agreementDefaultText = getDefaultAgreementText();
        var oldTenant = changeTenantInfo.CurrentTenant;
        if (changeTenantInfo.CurrentTenant !== "") {
            $.ajax({
                type: "GET",
                url: "/TenancyAgreements/GetSnpCase?snp=" + changeTenantInfo.CurrentTenant +"&padeg=RODITLN",
                async: false,
                success: function (data) {
                    oldTenant = data.snpAccusative;
                }
            });
            if (changeTenantInfo.CurrentTenantBirthDate !== "") {
                oldTenant += ", " + changeTenantInfo.CurrentTenantBirthDate + " г.р.";
            }
            oldTenant = " «" + oldTenant + "»";
        }
        var tenant = changeTenantInfo.NewTenantSurname + " " + changeTenantInfo.NewTenantName +
            (changeTenantInfo.NewTenantPatronymic !== "" ? " " + changeTenantInfo.NewTenantPatronymic : "");
        $.ajax({
            type: "GET",
            url: "/TenancyAgreements/GetSnpPartsCase?surname=" + changeTenantInfo.NewTenantSurname + "&name=" + changeTenantInfo.NewTenantName +
                "&patronymic=" + changeTenantInfo.NewTenantPatronymic + "&padeg=VINITELN",
            async: false,
            success: function (data) {
                tenant = data.snpAccusative;
            }
        });
        if (changeTenantInfo.NewTenantBirthDate !== "") {
            tenant += ", " + changeTenantInfo.NewTenantBirthDate + " г.р.";
        }
        tenant = "«" + tenant + "»";
        if (changeTenantInfo.DeathCurrentTenant == true) {
            contentLines.push(agreementDefaultText);
            text = "1) исключить из договора нанимателя " + oldTenant + " - по смерти;"
            contentLines.push(text);
            text = "2) считать стороной по договору - нанимателем - " + tenant + ";"
            contentLines.push(text);
        }
        else {
            agreementDefaultText = agreementDefaultText.replace("договорились:",
                "в связи c ________________________________________ нанимателя" + oldTenant + ", договорились:");
            text = "1) считать стороной по договору - нанимателем - ";
            text += tenant;
            contentLines.push(agreementDefaultText);
            contentLines.push(text);
        }
        contentElem.val(contentLines.join("\n"));
    }

    function addChangeKinshipTenantsInfoToContent(changeKinshipsInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var content = contentElem.val();
        var contentLines = content.split("\n");
        var nextPoint = getNextHeaderPoint(contentLines);
        var header = "\u200B" + nextPoint + ") изложить";
        var pointHeader = "";
        var point = $("#agreementModal #Agreement_Type_Point").val();
        var subPoint = $("#agreementModal #Agreement_Type_SubPoint").val();
        if (point !== "" && subPoint === "") {
            pointHeader = " пункт " + point;
        }
        if (subPoint !== "" && point === "") {
            pointHeader = " подпункт " + subPoint;
        }
        if (subPoint !== "" && point !== "") {
            pointHeader = " подпункт " + subPoint + " пункта " + point;
        }
        header += pointHeader + " в новой редакции:";
        contentLines.push(header);
        changeKinshipsInfo.each(function (ind, elem) {
            pointContent = "«" + elem.Tenant + " - " + elem.NewKinship + "»";
            contentLines.push(pointContent);
        });
        contentElem.val(contentLines.join("\n"));
    }

    function addChangeTenantInfoToModifications(changeTenantInfo) {
        if (changeTenantInfo.CurrentTenantGuid !== "") {
            if (changeTenantInfo.ExcludeCurrentTenant || changeTenantInfo.DeathCurrentTenant) {
                let checkbox = insertAutomateOperationsCheckBox("Исключить нанимателя «" + changeTenantInfo.CurrentTenant + "»");
                modifications.push({
                    Checkbox: checkbox, Operation: "ExcludePerson", Info: {
                        IdPerson: changeTenantInfo.CurrentTenantIdPerson,
                        Guid: changeTenantInfo.CurrentTenantGuid
                    }
                });
            } else {
                let checkbox =
                    insertAutomateOperationsCheckBox("Сменить родственное отношение текущего нанимателя на «" + changeTenantInfo.CurrentTenantNewKinship + "»");
                modifications.push({
                    Checkbox: checkbox, Operation: "ChangeKinship", Info: {
                        IdPerson: changeTenantInfo.CurrentTenantIdPerson,
                        Guid: changeTenantInfo.CurrentTenantGuid,
                        IdKinship: changeTenantInfo.CurrentTenantNewIdKinship
                    }
                });
            }
        }
        var newTenant = changeTenantInfo.NewTenantSurname + " " + changeTenantInfo.NewTenantName +
            (changeTenantInfo.NewTenantPatronymic !== "" ? " " + changeTenantInfo.NewTenantPatronymic : "");
        if (changeTenantInfo.NewTenantBirthDate !== "") {
            newTenant += ", " + changeTenantInfo.NewTenantBirthDate + " г.р.";
        }
        let checkbox = insertAutomateOperationsCheckBox("Установить новым нанимателем «" + newTenant + "»");
        modifications.push({
            Checkbox: checkbox, Operation: "ChangeKinship", Info: {
                IdPerson: changeTenantInfo.NewTenantIdPerson,
                Guid: changeTenantInfo.NewTenantGuid,
                IdKinship: "1"
            }
        });
    }

    function addChangeKinshipTenantsInfoToModifications(changeKinshipsInfo) {
        changeKinshipsInfo.each(function (ind, elem) {
            let checkbox =
                insertAutomateOperationsCheckBox("Сменить значение «" + elem.OldKinship +
                    "» для нанимателя «" + elem.Tenant +
                    "» на родственное отношение «" + elem.NewKinship + "»");
            modifications.push({
                Checkbox: checkbox, Operation: "ChangeKinship", Info: {
                    IdPerson: elem.IdPerson,
                    Guid: elem.Guid,
                    IdKinship: elem.NewIdKinship
                }
            });
        });
    }

    function getDefaultAgreementText() {
        return reformAgreementContent("1.1. По настоящему Соглашению Стороны по договору № {0} от {1} {2} найма жилого помещения, расположенного по адресу: {3}, договорились:");
    }

    function insertAutomateOperationsCheckBox(title) {
        var guid = uuidv4();
        $("#agreementModal #Agreement_AutomateOperations").append(
            '<div class="form-group form-check ml-1">' +
            '<input checked type="checkbox" class="form-check-input" id="AutomateOperation_' + guid+'">' + 
            '<label class="form-check-label" for="AutomateOperation_' + guid +'">'+title+'</label>' + 
            '</div>');
        return $("#agreementModal #Agreement_AutomateOperations").find("input[id^='AutomateOperation']").last();
    }

    function insertPoint(contentLines, point, headerWildcard) {
        var newContentLines = [];
        var customHeaderRegex = new RegExp(headerWildcard);
        var commonHeaderRegex = new RegExp("^\u200B?\\s*([0-9]+)\\s*[)]");
        var headerFounded = false;
        var pointInserted = false;
        for (var i = 0; i < contentLines.length; i++) {
            if (headerFounded && commonHeaderRegex.test(contentLines[i])) {
                newContentLines.push(point);
                pointInserted = true;
            }
            if (!headerFounded && customHeaderRegex.test(contentLines[i])) {
                headerFounded = true;
            }
            newContentLines.push(contentLines[i]);
        }
        if (!pointInserted) newContentLines.push(point);
        return newContentLines;
    }

    function isHeaderInserted(contentLines, headerWildcard) {
        var regex = new RegExp(headerWildcard);
        for (var i = 0; i < contentLines.length; i++) {
            if (regex.test(contentLines[i]))
                return true;
        }
        return false;
    }

    function getNextHeaderPoint(contentLines) {
        var regex = new RegExp("^\u200B?\\s*([0-9]+)\\s*[)]");
        var index = 1;
        for(var i = 0; i < contentLines.length; i++) {
            if (regex.test(contentLines[i]))
                index++;
        }
        return index;
    }

    function formatDate(date) {
        if (date !== "" && date !== null && date !== undefined) {
            var dateParts = date.split('-');
            if (dateParts.length === 3)
                return dateParts[2] + "." + dateParts[1] + "." + dateParts[0];
            else
                return date;
        }
        return "";
    }

    function parseDate(date) {
        if (date !== "" && date !== null && date !== undefined) {
            var dateParts = date.split('.');
            if (dateParts.length === 3)
                return dateParts[2] + "-" + dateParts[1] + "-" + dateParts[0];
            else
                return date;
        }
        return "";
    }

    var lastAgreementEndDateBeforeDismissal = undefined;

    $("#agreementModal #Agreement_Type_ProlongUntilDismissal").on("change", function (e) {
        var endDateElem = $("#Agreement_Type_ProlongEndDate");
        if ($(this).is(":checked")) {
            lastAgreementEndDateBeforeDismissal = endDateElem.val();
            endDateElem.val("");
            endDateElem.prop("disabled", "disabled");
        } else {
            endDateElem.prop("disabled", "");
            if (lastAgreementEndDateBeforeDismissal !== undefined) {
                endDateElem.val(lastAgreementEndDateBeforeDismissal);
            }
        }
        e.preventDefault();
    });

    $("#agreementModal #Agreement_Type_TenantExclude, #agreementModal #Agreement_Type_TenantDeath").on("change", function (e) {
        var tenantNewKinshipElem = $("#Agreement_Type_TenantNewIdKinship");
        if ($(this).is(":checked")) {            
            if ($(this).attr("id") == "Agreement_Type_TenantExclude") {
                $("#Agreement_Type_TenantDeath").prop("disabled", "disabled");
            }
            else {
                $("#Agreement_Type_TenantExclude").prop("disabled", "disabled");
            }
            tenantNewKinshipElem.prop("disabled", "disabled");
            tenantNewKinshipElem.attr("data-val", false);
            clearValidationError(tenantNewKinshipElem);
            clearValidationError(tenantNewKinshipElem.closest(".bootstrap-select").find("button[data-id]"));
        } else {
            $("#Agreement_Type_TenantExclude").prop("disabled", "");
            $("#Agreement_Type_TenantDeath").prop("disabled", "");
            tenantNewKinshipElem.prop("disabled", "");
            tenantNewKinshipElem.attr("data-val", true);
        }
        tenantNewKinshipElem.selectpicker("refresh");
        refreshValidationForm($("#TenancyProcessAgreementsModalForm"));
        e.preventDefault();
    });

    $("#agreementModal #Agreement_Type_TenancyPersonsChangeKinship").on("change", function (e) {
        var personOption = $(this).find("option[value='" + $(this).val() + "']");
        $("#Agreement_Type_OldKinship").val(personOption.data("kinship"));
        $("#Agreement_Type_NewKinship option").prop("disabled", false);
        $("#Agreement_Type_NewKinship option[value='" + personOption.data("id-kinship") + "']").prop("disabled", true);
        $("#Agreement_Type_NewKinship").selectpicker("val", "");
        $("#Agreement_Type_NewKinship").selectpicker("refresh");
        if (personOption.attr("data-id-new-kinship") !== "") {
            $("#agreementModal #Agreement_Type_NewKinship").selectpicker("val", personOption.attr("data-id-new-kinship"));
        }
        else {
            $("#agreementModal #Agreement_Type_NewKinship").selectpicker("val", "");
        }
        e.preventDefault();
    });

    $("#agreementModal #Agreement_Type_NewKinship").on("change", function (e) {
        var person = $("#agreementModal #Agreement_Type_TenancyPersonsChangeKinship option:selected");
        if (person.attr("data-id-new-kinship") !== undefined) {
            var kinship = $(this).find("option:selected");
            if (kinship.val() !== "") {
                person.attr("data-icon", "oi oi-check");
                person.attr("data-id-new-kinship", kinship.val());
                person.attr("data-new-kinship", kinship.text());
            }
            else {
                person.removeAttr("data-icon");
                person.attr("data-id-new-kinship", "");
                person.attr("data-new-kinship", "");
            }
        }
        $(this).find("option").prop("disabled", false);
        $(this).find("option[value='" + person.data("id-kinship") + "']").prop("disabled", true);
        $("#agreementModal #Agreement_Type_TenancyPersonsChangeKinship").selectpicker("refresh");
        e.preventDefault();
    });

    $('#TenancyProcessAgreementsForm').on('click', '.tenancy-agreement-delete-btn', deleteTenancyAgreement);
    $("#TenancyProcessAgreementsForm").on("click", "#tenancyAgreementAdd", addTenancyAgreement);
});