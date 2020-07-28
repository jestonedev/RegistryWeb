
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

    $('body').on('click', '.tenancy-person-open', function (e) {
        var wrapper = $(this).closest('.tenancy-person-item');

        var surname = wrapper.find('[id$="Surname"]').val();
        var name = wrapper.find('[id$="Name"]').val();
        var patronimyc = wrapper.find('[id$="Patronymic"]').val();
        var dateOfBirth = wrapper.find('[id$="DateOfBirth"]').val();
        var idKinship = wrapper.find('[id$="IdKinship"]').val();
        var personalAccount = wrapper.find('[id$="PersonalAccount"]').val();
        var includeDate = wrapper.find('[id$="IncludeDate"]').val();
        var excludeDate = wrapper.find('[id$="ExcludeDate"]').val();
        var idDocType = wrapper.find('[id$="IdDocumentType"]').val();
        var docSeria = wrapper.find('[id$="DocumentSeria"]').val();
        var docNum = wrapper.find('[id$="DocumentNum"]').val();
        var idDocIssuedBy = wrapper.find('[id$="IdDocumentIssuedBy"]').val();
        var dateOfDocIssue = wrapper.find('[id$="DateOfDocumentIssue"]').val();
        var snils = wrapper.find('[id$="Snils"]').val();

        var regIdStreet = wrapper.find('[id$="RegistrationIdStreet"]').val();
        var regHouse = wrapper.find('[id$="RegistrationHouse"]').val();
        var regFlat = wrapper.find('[id$="RegistrationFlat"]').val();
        var regRoom = wrapper.find('[id$="RegistrationRoom"]').val();

        var resIdStreet = wrapper.find('[id$="ResidenceIdStreet"]').val();
        var resHouse = wrapper.find('[id$="ResidenceHouse"]').val();
        var resFlat = wrapper.find('[id$="ResidenceFlat"]').val();
        var resRoom = wrapper.find('[id$="ResidenceRoom"]').val();

        var modal = $("#personModal");

        modal.find("#Person_Surname").val(surname);
        modal.find("#Person_Name").val(name);
        modal.find("#Person_Patronymic").val(patronimyc);
        modal.find("#Person_DateOfBirth").prop("disabled", "").val(dateOfBirth).prop("disabled", "disabled");
        modal.find("select#Person_IdKinship").val(idKinship).selectpicker('render');
        modal.find("#Person_Phone").val(personalAccount);
        modal.find("#Person_IncludeDate").prop("disabled", "").val(includeDate).prop("disabled", "disabled");
        modal.find("#Person_ExcludeDate").prop("disabled", "").val(excludeDate).prop("disabled", "disabled");

        modal.find("select#Person_IdDocType").val(idDocType).selectpicker('render');
        modal.find("#Person_DocSeria").val(docSeria);
        modal.find("#Person_DocNumber").val(docNum);
        modal.find("select#Person_IdDocIssuer").val(idDocIssuedBy).selectpicker('render');
        modal.find("#Person_IssueDate").prop("disabled", "").val(dateOfDocIssue).prop("disabled", "disabled");
        modal.find("#Person_Snils").val(snils);

        modal.find("select#Person_IdRegStreet").val(regIdStreet).selectpicker('render');
        modal.find("#Person_RegHouse").val(regHouse);
        modal.find("#Person_RegPremise").val(regFlat);
        modal.find("#Person_RegSubPremise").val(regRoom);

        modal.find("select#Person_IdLivigStreet").val(resIdStreet).selectpicker('render');
        modal.find("#Person_LivingHouse").val(resHouse);
        modal.find("#Person_LivingPremise").val(resFlat);
        modal.find("#Person_LivingSubPremise").val(resRoom);

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

    function tenancyClearValidationError(elem) {
        var spanError = $("span[data-valmsg-for='" + elem.attr("name") + "']");
        spanError.empty().removeClass("field-validation-error").addClass("field-validation-valid");
        elem.removeClass("input-validation-error");
    }

    function removeErrorFromValidator(validator, elem) {
        validator.errorList = $(validator.errorList)
            .filter(function (idx, error) {
                return $(error.element).prop("name") !== elem.attr("name");
            });

        delete validator.errorMap[elem.attr("name")];
    }

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
            tenancyClearValidationError(regNum);
            removeErrorFromValidator(validator, regNum);
        }
        if (regDate.val() === "" && $.trim(regNum.val()) !== "") {
            let error = {};
            error[regDate.attr("name")] = "Введите дату регистрации договора";
            validator.showErrors(error);
            isValid = false;
        } else {
            tenancyClearValidationError(regDate);
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
            tenancyClearValidationError(subTenancyNum);
            removeErrorFromValidator(validator, subTenancyNum);
        }
        if (subTenancyDate.val() === "" && $.trim(subTenancyNum.val()) !== "") {
            let error = {};
            error[subTenancyDate.attr("name")] = "Введите дату выдачи разрешения";
            validator.showErrors(error);
            isValid = false;
        } else {
            tenancyClearValidationError(subTenancyDate);
            removeErrorFromValidator(validator, subTenancyDate);
        }
        return isValid;
    }

    $("#TenancyProcess_RegistrationDate, #TenancyProcess_RegistrationNum, #TenancyProcess_SubTenancyDate, #TenancyProcess_SubTenancyNum").on("change", function () {
        var validator = $("#TenancyProcessForm").validate();
        tenancyCustomValidations(validator);
    });

    $("#TenancyProcessForm").on("submit", function (e) {
        $("button[data-id], .bootstrap-select").removeClass("input-validation-error");
        var validator = $(this).validate();
        var isFormValid = $(this).valid();
        if (!tenancyCustomValidations(validator)) {
            isFormValid = false;
        }

        if (!isFormValid) {
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