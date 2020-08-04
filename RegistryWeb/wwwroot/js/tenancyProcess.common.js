
$(function () {
    $('.tenancy-process-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogleHide);
    });

    $('body').on('click', '.tenancy-agreement-open', function (e) {
        var wrapper = $(this).closest('.tenancy-agreement-item');
        var idExecutor = wrapper.find('[id$="IdExecutor"]').val();
        var agreementDate = wrapper.find('[id$="AgreementDate"]').val();
        var agreementContent = wrapper.find('[id$="AgreementContent"]').val();
        var modal = $("#agreementModal");
        modal.find("#Agreement_Date").prop("disabled", "").val(agreementDate).prop("disabled", "disabled");
        modal.find("select#Agreement_IdExecutor").val(idExecutor).selectpicker('render');
        modal.find("#Agreement_Content").val(agreementContent);
        modal.on('hide.bs.modal', function () {
            modal.find("#Agreement_Content").scrollTop(0);
        });
        modal.modal('show');
        e.preventDefault();
    });

    var idRentTypeCategories = $("#TenancyProcess_IdRentTypeCategory option[value]").clone(true);

    $("#TenancyProcess_IdRentType").on("change", function (e) {
        var idRentType = $(this).val();
        $("#TenancyProcess_IdRentTypeCategory option[value]").remove();
        $(idRentTypeCategories).each(function (idx, option) {
            if ($(option).data("id-rent-type")+"" === idRentType) {
                $("#TenancyProcess_IdRentTypeCategory").append(option);
            }
        });
        $("#TenancyProcess_IdRentTypeCategory").selectpicker("refresh");
        e.preventDefault();
    });

    var lastEndDateBeforeDismissal = undefined;

    $("#TenancyProcess_UntilDismissal").on("change", function (e) {
        var endDateElem = $("#TenancyProcess_EndDate");
        if ($(this).is(":checked")) {
            lastEndDateBeforeDismissal = endDateElem.val();
            endDateElem.val("");
            endDateElem.prop("disabled", "disabled");
        } else {
            endDateElem.prop("disabled", "");
            if (lastEndDateBeforeDismissal !== undefined) {
                endDateElem.val(lastEndDateBeforeDismissal);
            }
        }
        e.preventDefault();
    });

    $("#TenancyProcess_IdRentType").change();

    function tenancyCustomValidations(validator) {
        var isValid = true;

        var regDate = $("#TenancyProcess_RegistrationDate");
        var regNum = $("#TenancyProcess_RegistrationNum");


        regNum.val($.trim(regNum.val()));
        if (regDate.val() !== "" && regNum.val() === "") {
            let error = {};
            error[regNum.attr("name")] = "Введите номер договора найма";
            validator.showErrors(error);
            isValid = false;
        } else {
            clearValidationError(regNum);
            removeErrorFromValidator(validator, regNum);
        }
        if (regDate.val() === "" && $.trim(regNum.val()) !== "") {
            let error = {};
            error[regDate.attr("name")] = "Введите дату регистрации договора";
            validator.showErrors(error);
            isValid = false;
        } else {
            clearValidationError(regDate);
            removeErrorFromValidator(validator, regDate);
        }
        
        var subTenancyNum = $("#TenancyProcess_SubTenancyNum");
        var subTenancyDate = $("#TenancyProcess_SubTenancyDate");


        subTenancyNum.val($.trim(subTenancyNum.val()));
        if (subTenancyDate.val() !== "" && subTenancyNum.val() === "") {
            let error = {};
            error[subTenancyNum.attr("name")] = "Введите номер реквизита разрешения";
            validator.showErrors(error);
            isValid = false;
        } else {
            clearValidationError(subTenancyNum);
            removeErrorFromValidator(validator, subTenancyNum);
        }
        if (subTenancyDate.val() === "" && $.trim(subTenancyNum.val()) !== "") {
            let error = {};
            error[subTenancyDate.attr("name")] = "Введите дату выдачи разрешения";
            validator.showErrors(error);
            isValid = false;
        } else {
            clearValidationError(subTenancyDate);
            removeErrorFromValidator(validator, subTenancyDate);
        }
        return isValid;
    }

    $("#TenancyProcess_RegistrationDate, #TenancyProcess_RegistrationNum, #TenancyProcess_SubTenancyDate, #TenancyProcess_SubTenancyNum").on("change", function () {
        var validator = $("#TenancyProcessForm").validate();
        tenancyCustomValidations(validator);
    });

    $("#TenancyProcessForm").on("submit", function (e) {
        var action = $("#TenancyProcessForm").data("action");
        $("button[data-id], .bootstrap-select").removeClass("input-validation-error");
        var validator = $(this).validate();
        var isFormValid = $(this).valid();
        if (!tenancyCustomValidations(validator)) {
            isFormValid = false;
        }
        var isReasonsValid = true;
        if (action === "Create") {
            isReasonsValid = $("#TenancyProcessReasonsForm").valid();
        }

        var isRentObjectsValid = true;
        if (action === "Create") {
            $("#TenancyProcessRentObjectsForm").find("input[id^='RentArea_']").each(function (idx, elem) {
                $(elem).val($(elem).val().replace('.', ','));
            });
            isRentObjectsValid = $("#TenancyProcessRentObjectsForm").valid();
            $("#TenancyProcessRentObjects .list-group-item").each(function (idx, elem) {
                if ($(elem).hasClass("rr-list-group-item-empty")) return;
                if (!tenancyRentObjectCustomValidations($(elem), $("#TenancyProcessRentObjectsForm").validate())) {
                    isRentObjectsValid = false;
                }
            });
        }

        if (!isFormValid || !isReasonsValid || !isRentObjectsValid) {
            $("select").each(function (idx, elem) {
                var id = $(elem).prop("id");
                var name = $(elem).prop("name");
                var errorSpan = $("span[data-valmsg-for='" + name + "']");
                if (errorSpan.hasClass("field-validation-error")) {
                    $("button[data-id='" + id + "']").addClass("input-validation-error");
                }
            });

            $(".toggle-hide").each(function (idx, elem) {
                if ($(elem).find(".field-validation-error").length > 0) {
                    var toggler = $(elem).closest(".card").find(".card-header .tenancy-process-toggler").first();
                    if (!isExpandElemntArrow(toggler)) {
                        toggler.click();
                    }
                }
            });
            $([document.documentElement, document.body]).animate({
                scrollTop: $(".input-validation-error").first().offset().top - 35
            }, 1000);

            e.preventDefault();
        } else {
            rentPeriodsCorrectNaming();
            if (action !== "Create") return true;
            var inputTemplate = "<input type='hidden' name='{0}' value='{1}'>";
            // Переносим основания найма в форму
            let tenancyReasons = getTenancyReasons();
            for (let i = 0; i < tenancyReasons.length; i++) {
                let tr = "TenancyProcess.TenancyReasons[" + i + "].";
                $(this).append(inputTemplate.replace('{0}', tr + "IdReason").replace('{1}', tenancyReasons[i].IdReason));
                $(this).append(inputTemplate.replace('{0}', tr + "IdProcess").replace('{1}', tenancyReasons[i].IdProcess));
                $(this).append(inputTemplate.replace('{0}', tr + "IdReasonType").replace('{1}', tenancyReasons[i].IdReasonType));
                $(this).append(inputTemplate.replace('{0}', tr + "ReasonDate").replace('{1}', tenancyReasons[i].ReasonDate));
                $(this).append(inputTemplate.replace('{0}', tr + "ReasonNumber").replace('{1}', tenancyReasons[i].ReasonNumber));
            }

            let tenancyPersons = getTenancyPersons();
            for (let i = 0; i < tenancyPersons.length; i++) {
                let tp = "TenancyProcess.TenancyPersons[" + i + "].";
                for (var field in tenancyPersons[i]) {
                    $(this).append(inputTemplate.replace('{0}', tp + field).replace('{1}', tenancyPersons[i][field]));
                }
            }

            let tenancyRentObjects = getTenancyRentObjects();
            for (let i = 0; i < tenancyRentObjects.length; i++) {
                let ro = "RentObjects[" + i + "].";
                $(this).append(inputTemplate.replace('{0}', ro + "Address.AddressType").replace('{1}', tenancyRentObjects[i].AddressType));
                $(this).append(inputTemplate.replace('{0}', ro + "Address.Id").replace('{1}', tenancyRentObjects[i].IdObject));
                $(this).append(inputTemplate.replace('{0}', ro + "RentArea").replace('{1}', tenancyRentObjects[i].RentArea));
            }

            let tenancyFiles = getTenancyFiles();
            for (let i = 0; i < tenancyFiles.length; i++) {
                let tf = "TenancyProcess.TenancyFiles[" + i + "].";
                $(this).append(inputTemplate.replace('{0}', tf + "IdFile").replace('{1}', tenancyFiles[i].IdFile));
                $(this).append(inputTemplate.replace('{0}', tf + "IdProcess").replace('{1}', tenancyFiles[i].IdProcess));
                $(this).append(inputTemplate.replace('{0}', tf + "Description").replace('{1}', tenancyFiles[i].Description));
                let file = $(tenancyFiles[i].AttachmentFile).clone();
                file.attr("name", "TenancyFile[" + i + "]");
                $(this).append(file);
            }
        }
    });

    $("form").on("change", "select", function () {
        var isValid = $(this).valid();
        var id = $(this).prop("id");
        if (!isValid) {
            $("button[data-id='" + id + "']").addClass("input-validation-error");
        } else {

            $("button[data-id='" + id + "']").removeClass("input-validation-error");
        }
    });

    $("#createBtn, #editBtn, #deleteBtn").on("click", function () {
        $("#TenancyProcessForm").submit();
    });

    function checkRentPeriod(rentPeriod) {
        if (rentPeriod.BeginDate === "" && rentPeriod.EndDate === "" && rentPeriod.UntilDismissal === false) {
            alert("Не задан период найма для переноса");
            return false;
        }
        var equalPeriod = $("#TenancyProcessRentPeriods .list-group-item").filter(function (idx, elem) {
            return !$(elem).hasClass("rr-list-group-item-empty") &&
                $(elem).find("input[id^='BeginDate']").val() === rentPeriod.BeginDate &&
                $(elem).find("input[id^='EndDate']").val() === rentPeriod.EndDate &&
                $(elem).find("input[id^='UntilDismissal']").is(":checked") === rentPeriod.UntilDismissal;
        });
        if (equalPeriod.length > 0) {
            alert("Указанный период уже присутствует в списке предыдущих периодов найма");
            return false;
        }
        return true;
    }

    $("#RentPeriodToHistroy").on("click", function (e) {
        e.preventDefault();
        var rentPeriod = {
            IdProcess: $("#TenancyProcess_IdProcess").val(),
            BeginDate: $("#TenancyProcess_BeginDate").val(),
            EndDate: $("#TenancyProcess_EndDate").val(),
            UntilDismissal: $("#TenancyProcess_UntilDismissal").is(":checked")
        };
        if (!checkRentPeriod(rentPeriod)) return false;

        var formData = rentPeriodToFormData(rentPeriod);
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyRentPeriods/SaveRentPeriod',
            data: formData,
            processData: false,
            contentType: false,
            success: function (rentPeriodSuccess) {
                let action = $('#TenancyProcessRentPeriods').data('action');

                $.ajax({
                    type: 'POST',
                    url: window.location.origin + '/TenancyProcesses/AddRentPeriod',
                    data: { action },
                    success: function (elem) {
                        let list = $('#TenancyProcessRentPeriods');
                        list.find(".rr-list-group-item-empty").hide();
                        let rentPeriodToggle = $('.rr-rent-periods-card .tenancy-process-toggler');
                        if (!isExpandElemntArrow(rentPeriodToggle)) // развернуть при добавлении, если было свернуто 
                            rentPeriodToggle.click();
                        list.append(elem);
                        elem = list.find(".list-group-item").last();
                        $(elem).find("input[id^='BeginDate']").val(rentPeriod.BeginDate);
                        $(elem).find("input[id^='EndDate']").val(rentPeriod.EndDate);
                        $(elem).find("input[id^='UntilDismissal']").prop("checked", rentPeriod.UntilDismissal);
                        $("#TenancyProcess_BeginDate").val("");
                        $("#TenancyProcess_EndDate").val("").prop("disabled", "");
                        $("#TenancyProcess_UntilDismissal").prop("checked", false);
                        if (rentPeriodSuccess.idRentPeriod > 0) {
                            $(elem).find("input[name^='IdRentPeriod']").val(rentPeriodSuccess.idRentPeriod);
                        }

                        $([document.documentElement, document.body]).animate({
                            scrollTop: $(elem).offset().top
                        }, 1000);
                    }
                });
            }
        });
    });
});
;