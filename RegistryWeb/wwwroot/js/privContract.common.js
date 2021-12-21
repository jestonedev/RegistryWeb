$(function () {
    var form = $('#privatizationForm');
    var addressModal = $('#AddressRegistryModal');
    var contractorModalForm = $('#PrivContractorModalForm');
    var contractorModal = $("#PrivContractorModal");

    $.validator.addMethod('addressRequired', function (value, element) {
        return $('#privatizationForm [name="IdPremise"]').val() != "" ||
            $('#privatizationForm [name="IdSubPremise"]').val() != "" || $('#privatizationForm [name="IdBuilding"]').val() != "";
    }, 'Блок «Адрес» является обязательным для заполнения ');
    
    var validator = form.validate();
    validator.settings.ignoreTitle = true;
    validator.settings.rules = {
        RegNumber: {
            required: true
        },
        AddressRegistryText: {
            addressRequired: true
        }
    };
    validator.settings.messages = {
        RegNumber: {
            required: 'Поле «Рег. номер» является обязательным'
        }
    };

    var validatorModal = contractorModalForm.validate();
    validatorModal.settings.ignore = '.rr-valid-ignore';
    validatorModal.settings.ignoreTitle = true;

    $("#RegNumber").on("change", function () {
        $(this).valid();
    });

    var action = form.attr('data-action');
    if (action === 'Details' || action === 'Delete') {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
        $('input[type="hidden"]').prop('disabled', false);
        $('input[type="submit"]').prop('disabled', false);
    }
    $('#privContractInfo').hide();
    $('#privContractors select').attr('disabled', true);
    if ($('#privatizationForm [name="IdPremise"]').val() != "" ||
        $('#privatizationForm [name="IdSubPremise"]').val() != "" ||
        $('#privatizationForm [name="IdBuilding"]').val() != "") {
        $('#addressRegistryChangeBtn').attr('title', 'Редактировать');
        $('#addressRegistryChangeBtn span').addClass('oi-pencil');
    } else {
        $('#addressRegistryChangeBtn').attr('title', 'Добавить');
        $('#addressRegistryChangeBtn span').removeClass("oi-pencil").addClass('oi-plus');
        $('#HomesBtn').hide();
        $("#addressRegistryChangeBtn").closest(".input-group-append").append($("#addressRegistryChangeBtn"));
    }
    var additionalEstateBlankItem = $(".rr-priv-additional-estate").last();
    var additionalEstateTemplate = additionalEstateBlankItem.clone(true);
    additionalEstateBlankItem.remove();


    function getPrivContractorJson(privContractorElem, isModal = false) {
        var fields = privContractorElem.find('input, select, textarea');
        var contractorJson = {};
        fields.each(function (idx, elem) {
            if ($(elem).attr('id') === undefined) return;
            var name = $(elem).attr('id').split('_')[0];
            if (isModal) {
                name = $(elem).attr('id').split('_')[1];
            }
            if ($(elem).attr('type') == 'checkbox') {
                contractorJson[name] = $(elem).is(':checked');
            }
            else {
                contractorJson[name] = $(elem).val();
            }
        });
        return contractorJson;
    }
    function privContractorFillModal(contractorJson) {
        contractorModal.find('.r-description').show();
        contractorModal.find("[name='PrivContractor.Description']").removeClass('rr-valid-ignore');
        contractorModal.find("[name='PrivContractor.Passport']").removeClass('rr-valid-ignore');
        contractorModal.find("[name='PrivContractor.Part']").removeClass('rr-valid-ignore');
        $.each(contractorJson, function (key, value) {
            if (key == 'IsNoncontractor') {
                if (value == 'False' || value == "false" || value == false) {
                    contractorModal.find("[for='PrivContractor_IsNoncontractor']").text('Участник');
                    contractorModal.find("[name='PrivContractor.IsNoncontractor']").prop("disabled", "").bootstrapToggle('on').prop("disabled", "disabled");
                    contractorModal.find("[for='PrivContractor_Description']").text('Текст доверенности');
                    contractorModal.find(".r-addition-fields-contractor").show();
                }
                else {
                    contractorModal.find("[for='PrivContractor_IsNoncontractor']").text('Неучастник');
                    contractorModal.find("[name='PrivContractor.IsNoncontractor']").prop("disabled", "").bootstrapToggle('off').prop("disabled", "disabled");
                    contractorModal.find("[for='PrivContractor_Description']").text('Причина неучастия');
                    contractorModal.find("[name='PrivContractor.Description']").addClass('rr-valid-ignore');
                    contractorModal.find("[name='PrivContractor.Passport']").addClass('rr-valid-ignore');
                    contractorModal.find("[name='PrivContractor.Part']").addClass('rr-valid-ignore');
                    contractorModal.find('.r-addition-fields-contractor').hide();
                }
            }
            else if (key == 'HasDover') {
                if (value == 'False' || value == "false" || value == false) {
                    contractorModal.find("[for='PrivContractor_HasDover']").text('Нет');
                    contractorModal.find("[name='PrivContractor.HasDover']").prop("disabled", "").bootstrapToggle('off').prop("disabled", "disabled");
                    contractorModal.find('.r-description').hide();
                    if (contractorModal.find("[name='PrivContractor.IsNoncontractor']").is(':checked')) {
                        contractorModal.find("[name='PrivContractor.Description']").addClass('rr-valid-ignore');
                        contractorModal.find('.r-description').hide();
                    }
                }
                else {
                    contractorModal.find("[for='PrivContractor_HasDover']").text('Есть');
                    contractorModal.find("[name='PrivContractor.HasDover']").prop("disabled", "").bootstrapToggle('on').prop("disabled", "disabled");
                }
            }
            else {
                contractorModal.find("[name='PrivContractor." + key + "']").val(value);
            }
        });
        var modalFields = contractorModal.find('input, select, textarea');
        if (action === 'Details' || action === 'Delete') {
            modalFields.prop('disabled', 'disabled');
            $("#PrivContractor_AddDefaultRefusenikReason").remove();
            $("#PrivContractor_Passport").next(".input-group-append").remove();
        }
        else {
            modalFields.prop('disabled', '');
        }
    }
    function privatizationToggle(e) {
        $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
        $('#privatization').toggle();
        e.preventDefault();
    }
    function privContractorsToggle(e) {
        $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
        $('#privContractors').toggle();
        e.preventDefault();
    }
    function privContractInfoToggle(e) {
        $(this).children('span').toggleClass('oi-chevron-top').toggleClass('oi-chevron-bottom');
        $('#privContractInfo').toggle();
        e.preventDefault();
    }
    function getPrivContractorBaseElem() {
        var idContractor = contractorModal.find('#PrivContractor_IdContractor').val();
        var r = $(".list-group-item input[id^='IdContractor']").filter(function (ind, elem) {
            return elem.value == idContractor;
        });
        return r.closest('.list-group-item');
    }
    function liIsNoncontractorChange(li, isNoncontractor) {
        if (isNoncontractor) {
            li.attr('title', 'Неучастник');
            li.removeAttr('style');
        }
        else {
            li.attr('title', 'Участник');
            li.attr('style', 'border-left: 2px solid #ffc107;');
        }
    }

    var clearDescriptionOnNoncontractroChange = false;

    contractorModal.on("show.bs.modal", function () {
        if (!subModalOpened) {
            $("body").css("overflow", "hidden");
            $('#PrivContractor_IsNoncontractor').change();
            $("#PrivContractor_HasDover").change();
            clearDescriptionOnNoncontractroChange = true;
        }
    });

    contractorModal.on("shown.bs.modal", function () {
        $("#PrivContractor_IdKinship").selectpicker('refresh');
        if (subModalOpened) {
            subModalOpened = false;
            if (passportStartSelection !== undefined && passportEndSelection !== undefined) {
                $("#PrivContractor_Passport").focus();
                $("#PrivContractor_Passport")[0].setSelectionRange(passportStartSelection, passportEndSelection);
                passportStartSelection = undefined;
                passportEndSelection = undefined;
            }
        }
    });

    contractorModal.on("hide.bs.modal", function () {
        if (!subModalOpened) {
            $("body").css("overflow", "");
            clearDescriptionOnNoncontractroChange = false;
        }
    });

    $("#PrivContractor_DateBirth").on("change", function (e) {
        var dateBirth = $("#PrivContractor_DateBirth").val();
        contractorModal.find("[name='PrivContractor.HasDover']").removeAttr("disabled");
        contractorModal.find("[name='PrivContractor.Passport']").addClass('rr-valid-ignore');
        if (dateBirth !== "" && dateBirth !== undefined) {
            var age = DatesDiffInYears(new Date(dateBirth), new Date());
            if (age < 14) {
                contractorModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('on').attr("disabled", "disabled");
                contractorModal.find("#PrivContractor_Passport").removeClass("input-validation-error");
                contractorModal.find("[data-valmsg-for='PrivContractor.Passport']").empty();
            } else
            if ($("#PrivContractor_IsNoncontractor").is(':checked')) {
                contractorModal.find("[name='PrivContractor.Passport']").removeClass('rr-valid-ignore');
            }
        }
    });

    function privContractorIsNoncontractorChange(e) {
        contractorModal.find('.r-description').show();
        contractorModal.find("[name='PrivContractor.Description']").removeClass('rr-valid-ignore');
        contractorModal.find("[name='PrivContractor.Part']").removeClass('rr-valid-ignore');
        if ($(this).is(':checked')) {
            contractorModal.find("[for='PrivContractor_IsNoncontractor']").text('Участник');
            contractorModal.find("[for='PrivContractor_Description']").text('Текст доверенности');
            contractorModal.find('.r-addition-fields-contractor').show();
            if (clearDescriptionOnNoncontractroChange) {
                contractorModal.find('#PrivContractor_Passport').val('');
                contractorModal.find('#PrivContractor_Part').val('');
                contractorModal.find("[for='PrivContractor_HasDover']").text('Нет');
                contractorModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('off');
                contractorModal.find("[name='PrivContractor.Description']").addClass('rr-valid-ignore');
                contractorModal.find('.r-description').hide();
            }
            contractorModal.find("#PrivContractor_DefaultRefusenikReasonDropdown").hide();
            contractorModal.find("#PrivContractor_AddContractorWarrantTemplate").show();
        }
        else {
            contractorModal.find("[for='PrivContractor_HasDover']").text('Нет');
            contractorModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('off');
            contractorModal.find("[name='PrivContractor.Description']").addClass('rr-valid-ignore').removeClass("input-validation-error");
            contractorModal.find("[data-valmsg-for='PrivContractor.Description']").html('');
            contractorModal.find("[name='PrivContractor.Part']").addClass('rr-valid-ignore');
            contractorModal.find("[for='PrivContractor_IsNoncontractor']").text('Неучастник');
            contractorModal.find("[for='PrivContractor_Description']").text('Причина неучастия');
            contractorModal.find("#PrivContractor_DefaultRefusenikReasonDropdown").show();
            contractorModal.find("#PrivContractor_AddContractorWarrantTemplate").hide();
            contractorModal.find('.r-addition-fields-contractor input').val('');
            contractorModal.find('.r-addition-fields-contractor').hide();
        }
        if (clearDescriptionOnNoncontractroChange) {
            contractorModal.find('#PrivContractor_Description').val('');
        }
        $("#PrivContractor_DateBirth").change();
    }
    function privContractorHasDoverChange(e) {
        if (contractorModal.find("[name='PrivContractor.IsNoncontractor']").is(':checked')) {
            contractorModal.find("[name='PrivContractor.Description']").removeClass('rr-valid-ignore');
            if ($(this).is(':checked')) {
                contractorModal.find("[for='PrivContractor_HasDover']").text('Есть');                
                contractorModal.find('.r-description').show();
            }
            else {
                contractorModal.find("[for='PrivContractor_HasDover']").text('Нет');
                contractorModal.find("[name='PrivContractor.Description']").addClass('rr-valid-ignore');
                contractorModal.find("[name='PrivContractor.Description']").val('');
                contractorModal.find('.r-description').hide();
            }
        }
        e.preventDefault();
    }
    function privContractorModalSave(e) {        
        var isValid = contractorModalForm.valid();
        if (isValid) {
            var contractor = getPrivContractorJson($('#PrivContractorModal'), true);
            contractor.IsNoncontractor = !contractor.IsNoncontractor;
            if ($('#closePrivContractorModalBtn').attr('data-is-create') == 'true') {
                $.ajax({
                    async: false,
                    type: 'POST',
                    url: window.location.origin + '/Privatization/PrivContractorElemAdd',
                    data: { contractor, action },
                    success: function (elem) {
                        $('#privContractors').append(elem);                
                        var li = $('#privContractors li').last();
                        li.find('select').selectpicker('render');
                        li.find('select').attr('disabled', true);
                    }
                });
            }
            else {
                var baseElem = getPrivContractorBaseElem();
                baseElem.find("[id^='Surname']").val(contractorModal.find("[name$='Surname']").val());
                baseElem.find("[id^='Name']").val(contractorModal.find("[name$='Name']").val());
                baseElem.find("[id^='Patronymic']").val(contractorModal.find("[name$='Patronymic']").val());
                baseElem.find("[id^='DateBirth']").val(contractorModal.find("[name$='DateBirth']").val());
                baseElem.find("[id^='IdKinship']").selectpicker('val', contractorModal.find("[name$='IdKinship']").val());
                baseElem.find("[id^='IsNoncontractor']").val(!contractorModal.find("[name$='IsNoncontractor']").is(':checked'));
                if (contractorModal.find("[name$='IsNoncontractor']").is(':checked')) {
                    baseElem.find("[id^='Passport']").val(contractorModal.find("[name$='Passport']").val());
                    baseElem.find("[id^='Part']").val(contractorModal.find("[name$='Part']").val());
                    baseElem.find("[id^='HasDover']").val(contractorModal.find("[name$='HasDover']").is(':checked'));
                    if (!contractorModal.find("[name$='HasDover']").is(':checked')) {
                        contractorModal.find("[name$='Description']").val('');
                    }
                    liIsNoncontractorChange(baseElem, false);
                }
                else {
                    baseElem.find("[id^='Passport']").val('');
                    baseElem.find("[id^='Part']").val('');
                    baseElem.find("[id^='HasDover']").val('');
                    liIsNoncontractorChange(baseElem, true);
                }
                baseElem.find("[id^='Description']").val(contractorModal.find("[name$='Description']").val());
            }
            contractorModal.modal('hide');
        }         
        e.preventDefault();
    }


    //Адрес
    function addressRegistryChange(e) {
        addressModal.modal('show');
        e.preventDefault();
    }

    function additionalAddresRegistryChange(e) {
        var currentEstate = $(this).closest(".rr-priv-additional-estate");
        var index = currentEstate.index();
        addressModal.data('additional-estate-index', index);
        addressModal.modal('show');
        e.preventDefault();
    }

    function additionalAddresRegistryAdd(e) {
        $("#AdditionalEstates").append(additionalEstateTemplate.clone(true));
        var lastInsertedEstate = $(".rr-priv-additional-estate").last();
        var lastInsertedEstateIndex = lastInsertedEstate.index();
        lastInsertedEstate.removeClass("d-none");
        var changeBtn = lastInsertedEstate.find("[id^='additionalAddressRegistryChangeBtn_']");
        changeBtn.closest(".input-group-append").append(changeBtn);
        addressModal.data('additional-estate-index', lastInsertedEstateIndex);
        addressModal.modal('show');
        e.preventDefault();
    }

    function additionalAddresRegistryDelete(e) {
        $(this).closest(".rr-priv-additional-estate").remove();
        e.preventDefault();
    }

    function addressRegistryModalSearch(e) {
        $('#setAddressRegistryModalBtn').attr('disabled', true);
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/Address/GetAddressRegistryModal',
            data: {
                IdStreet: $('#AddressRegistry_IdStreet').selectpicker('val'),
                House: $('#AddressRegistry_House').val(),
                PremisesNum: $('#AddressRegistry_PremisesNum').val(),
                SubPremisesNum: $('#AddressRegistry_SubPremisesNum').val()
            },
            success: function (elem) {
                var div = $('#resultAddressRegistryModal');
                if (elem == -1)
                    div.text("Адрес не найден");
                else if (elem == -2)
                    div.text("Найдено несколько адресов. Уточните запрос");
                else {
                    $('#AddressRegistry_IdBuilding').val(elem.item1.idParents.IdBuilding);
                    $('#AddressRegistry_IdPremise').val(elem.item1.idParents.IdPremise);
                    $('#AddressRegistry_IdSubPremise').val(elem.item1.idParents.IdSubPremise);
                    if (elem.item2.demolishedDate !== null) {
                        div.html(elem.item1.text + '<span class=rr-priv-address-label>Снесено</span>');
                    } else
                        if (elem.item2.emergencyDate !== null) {
                            if (elem.item2.excludeEmergencyDate == null || new Date(elem.item2.emergencyDate) > new Date(elem.item2.excludeEmergencyDate)) {
                                div.html(elem.item1.text + '<span class=rr-priv-address-label>Аварийное</span>');
                            } else {
                                div.text(elem.item1.text);
                                $('#setAddressRegistryModalBtn').removeAttr('disabled');
                            }
                        } else {
                            div.text(elem.item1.text);
                            $('#setAddressRegistryModalBtn').removeAttr('disabled');
                        }
                }
            }
        });
        e.preventDefault();
    }

    function addressRegistryModalSet(e) {
        var additionalEstateIndex = addressModal.data("additional-estate-index");
        if (additionalEstateIndex === undefined) {
            if ($('#addressRegistryChangeBtn span').hasClass('oi-plus')) {
                $('#addressRegistryChangeBtn span').addClass('oi-pencil').removeClass('oi-plus');
                $('#addressRegistryChangeBtn').attr('title', 'Редактировать');
                var append = $("#addressRegistryChangeBtn").closest(".input-group-append");
                append.append($("#HomesBtn"));
                $('#HomesBtn').show();
                $("#additionalAddressRegistryAddBtn").show();
            }
            var mainEstateElem = $("#MainEstate");
            mainEstateElem.find('#addressRegistryText').val($('#resultAddressRegistryModal').text());
            form.find('[name="IdBuilding"]').val($('#AddressRegistry_IdBuilding').val());
            form.find('[name="IdPremise"]').val($('#AddressRegistry_IdPremise').val());
            form.find('[name="IdSubPremise"]').val($('#AddressRegistry_IdSubPremise').val());
            mainEstateElem.find('#addressRegistryText').valid();
            mainEstateElem.find('[href^="/Buildings/Details"]')
                .attr('href', '/Buildings/Details?idBuilding=' + $('#AddressRegistry_IdBuilding').val());
            var premiseHref = mainEstateElem.find('[href^="/Premises/Details"]');
            if ($('#AddressRegistry_IdPremise').val() == "") {
                premiseHref.addClass("d-none");
            } else {
                premiseHref.attr('href', '/Premises/Details?idPremises=' + $('#AddressRegistry_IdPremise').val());
                premiseHref.removeClass("d-none");
            }
        } else {
            addressModal.data("additional-estate-success", "true");
            var additionalEstateElem = $($(".rr-priv-additional-estate")[additionalEstateIndex]);
            additionalEstateElem.find("[name^='AdditionalAddressRegistryText']").val($('#resultAddressRegistryModal').text());
            additionalEstateElem.find("[name$='.IdBuilding']").val($('#AddressRegistry_IdBuilding').val());
            additionalEstateElem.find("[name$='.IdPremise']").val($('#AddressRegistry_IdPremise').val());
            additionalEstateElem.find("[name$='.IdSubPremise']").val($('#AddressRegistry_IdSubPremise').val());
            additionalEstateElem.find('[href^="/Buildings/Details"]')
                .attr('href', '/Buildings/Details?idBuilding=' + $('#AddressRegistry_IdBuilding').val());
            
            premiseHref = additionalEstateElem.find('[href^="/Premises/Details"]');
            if ($('#AddressRegistry_IdPremise').val() == "") {
                premiseHref.addClass("d-none");
            } else {
                premiseHref.attr('href', '/Premises/Details?idPremises=' + $('#AddressRegistry_IdPremise').val());
                premiseHref.removeClass("d-none");
            }


            var additionalEstateAppend = additionalEstateElem.find("[id^='additionalAddressRegistryChangeBtn_']").closest(".input-group-append");
            var additionalEstateHomeBtn = additionalEstateElem.find("[id^='HomesBtn_']");
            additionalEstateAppend.append(additionalEstateHomeBtn);
            additionalEstateHomeBtn.show();
        }
        $('#resultAddressRegistryModal').text('');
        resetModalForm($("#AddressRegistryModalForm"));
        $('#setAddressRegistryModalBtn').attr('disabled', true);
        addressModal.modal('hide');
        e.preventDefault();
    }
    function addressRegistryModalClear(e) {
        $('#resultAddressRegistryModal').text('');
        resetModalForm($("#AddressRegistryModalForm"));
        $("#AddressRegistry_IdRegion").change();
        e.preventDefault();
    }
    function idRegionAddressRegistryModalChange(e) {
        var idRegion = $('#AddressRegistry_IdRegion').selectpicker('val');
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Address/GetKladrStreets',
            dataType: 'json',
            data: { idRegion },
            success: function (data) {
                var select = $('#AddressRegistry_IdStreet');
                select.selectpicker('destroy');
                select.find('option[value]').remove();
                $.each(data, function (i, d) {
                    select.append('<option value="' + d.idStreet + '">' + d.streetName + '</option>');
                });
                select.selectpicker();
            }
        });
        e.preventDefault();
    }


    function privContractorAdd(e) {
        if ($('#privContractors .rr-list-group-item-empty').is(':visible')) {
            $('#privContractors .rr-list-group-item-empty').hide();
        }
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/Privatization/PrivContractorAdd',
            data: { action },
            success: function (contractorJson) {
                privContractorFillModal(contractorJson);
                $('#savePrivContractorModalBtn').text('Создать');
                $('#closePrivContractorModalBtn').attr('data-is-create','true');
                contractorModal.modal('show');
                $('#PrivContractor_IsNoncontractor').change();
            }
        });
        e.preventDefault();
    }
    function privContractEditDetails(e) {
        var privContractorElem = $(this).closest('.list-group-item');
        var contractorJson = getPrivContractorJson(privContractorElem);
        privContractorFillModal(contractorJson);
        $('#savePrivContractorModalBtn').text('Сохранить');
        $('#closePrivContractorModalBtn').attr('data-is-create', 'false');
        contractorModal.modal('show');
        e.preventDefault();
    }
    function privContractorDelete(e) {
        var privContractorElem = $(this).closest('.list-group-item');
        privContractorElem.remove();
        if ($('#privContractors .list-group-item[data-id]').length == 0) {
            $('#privContractors .rr-list-group-item-empty').show();
        }
        e.preventDefault();
    }

    function snpChange() {
        var val = $(this).val();
        if (val.length > 0)
            val = val[0].toUpperCase() + val.substr(1);
        $(this).val(val);
    }


    $('#privatizationToggle').on('click', privatizationToggle);
    $('#privContractorsToggle').on('click', privContractorsToggle);
    $('#privContractInfoToggle').on('click', privContractInfoToggle);
    $('#PrivContractor_IsNoncontractor').on('change', privContractorIsNoncontractorChange);
    $('#PrivContractor_Surname, #PrivContractor_Name, #PrivContractor_Patronymic').on('focusout', snpChange);
    $('#PrivContractor_HasDover').on('change', privContractorHasDoverChange);
    $("#savePrivContractorModalBtn").on('click', privContractorModalSave);
    $("#privContractorAdd").on('click', privContractorAdd);
    $("#addressRegistryChangeBtn").on('click', addressRegistryChange);
    $("#AdditionalEstates").on('click', "[id^='additionalAddressRegistryChangeBtn_']", additionalAddresRegistryChange);
    $("#AdditionalEstates").on('click', "[id^='additionalAddressRegistryDeleteBtn_']", additionalAddresRegistryDelete);
    $("#additionalAddressRegistryAddBtn").on('click', additionalAddresRegistryAdd);
    $("#searchAddressRegistryModalBtn").on('click', addressRegistryModalSearch);
    $("#setAddressRegistryModalBtn").on('click', addressRegistryModalSet);
    $("#clearAddressRegistryModalBtn").on('click', addressRegistryModalClear);
    $("#AddressRegistry_IdRegion").on('change', idRegionAddressRegistryModalChange);
    $('#privContractors').on('click','.priv-contract-edit-btn, .priv-contract-details-btn', privContractEditDetails);
    $('#privContractors').on('click', '.priv-contract-delete-btn', privContractorDelete);

    $('#privatizationCreate, #privatizationEdit').click(function (e) {
        e.preventDefault();
        var privContractors = $('#privContractors .list-group-item[data-id]');
        form.find('select option').attr('disabled', false);
        privContractors.each(function (ind, elem) {
            var fields = $(elem).find('input, select, textarea');
            fields.each(function (i, el) {
                var name = $(el).attr('id').split('_')[0];
                $(el).attr('name', "PrivContractors[" + ind + "]." + name);
                if (name == 'IdContractor' && !/^\d+$/.test($(el).val())) {
                    $(el).val(0);
                }
            });
        });

        var additionalEstate = $('.rr-priv-additional-estate');
        additionalEstate.each(function (ind, elem) {
            var fields = $(elem).find('input, select, textarea');
            fields.each(function (i, el) {
                var name = $(el).attr('id').split('_')[0];
                $(el).attr('name', "PrivAdditionalEstates[" + ind + "]." + name);
                if (name == 'IdEstate' && !/^\d+$/.test($(el).val())) {
                    $(el).val(0);
                }
            });
        });

        if (form.valid()) {
            $("#privContractors").find('input, select, textarea').prop("disabled", "");
            form.submit();
        }
    });
    $('#privatizationDelete').click(function (e) {
        e.preventDefault();
        form.submit();
    }); 

    addressModal.on("hide.bs.modal", function () {
        var index = addressModal.data("additional-estate-index");
        if (index !== undefined) {
            addressModal.removeData("additional-estate-index");
            if (addressModal.data("additional-estate-success") !== "true") {
                $($(".rr-priv-additional-estate")[index]).remove();
            }
        }
    });

    $("#PrivContractor_DefaultRefusenikReasonDropdown").on("click", '.dropdown-item', function (e) {
        var textarea = $("#PrivContractor_Description");
        var text = $(this).text();
        textarea.val(text);
        e.preventDefault();
    });

    var subModalOpened = false;
    var docIssuedModal = $("#DocumentsIssuedByModal");
    var passportStartSelection = undefined;
    var passportEndSelection = undefined;

    $("#PrivContractor_AddPassportIssuer").on("click", function (e) {
        $("body").css("overflow", "hidden").removeClass("modal-open");
        subModalOpened = true;
        contractorModal.modal('hide');
        docIssuedModal.modal('show');
        e.preventDefault();
    });

    $("#SelectDocumentsIssuedByBtn").on("click", function (e) {
        var name = $("#DocumentsIssuedByModal #DocumentsIssuedBy_DocumentIssuedByName").val();
        var passportElem = $("#PrivContractor_Passport");
        var passportElemValueLength = passportElem.val().length;
        passportElem.val(passportElem.val() + name);
        passportStartSelection = passportElemValueLength;
        passportEndSelection = passportElemValueLength + name.length;
        docIssuedModal.modal('hide');
        contractorModal.modal('show');
        e.preventDefault();
    });

    docIssuedModal.on("hide.bs.modal", function () {
        contractorModal.modal('show');
    });

    docIssuedModal.on("shown.bs.modal", function () {
        $("#DocumentsIssuedBy_DocumentIssuedByName").selectpicker("refresh");
    });

    var privContractorWarrantModal = $("#PrivContractorWarrantModal");

    $("#PrivContractor_AddContractorWarrantTemplate").on("click", function (e) {
        var dateBirth = $("#PrivContractor_DateBirth").val();

        privContractorWarrantModal.find(".rr-priv-contractor-warrant").hide();
        if (dateBirth === "" || dateBirth === undefined) {
            privContractorWarrantModal.find(".rr-priv-contractor-warrant-other").show();
        } else {
            var age = DatesDiffInYears(new Date($("#PrivContractor_DateBirth").val()), new Date());
            if (age < 14) {
                privContractorWarrantModal.find(".rr-priv-contractor-warrant-14").show();
            } else if (age < 18) {
                privContractorWarrantModal.find(".rr-priv-contractor-warrant-18").show();
            } else {
                privContractorWarrantModal.find(".rr-priv-contractor-warrant-other").show();
            }
        }

        $("body").css("overflow", "hidden").removeClass("modal-open");
        subModalOpened = true;
        contractorModal.modal('hide');
        privContractorWarrantModal.modal('show');
        e.preventDefault();
    });

    $("#SelecContractorWarrantTemplateBtn").on("click", function (e) {
        var activeTemplate = privContractorWarrantModal.find(".rr-priv-contractor-warrant")
            .filter(function (idx, elem) { return $(elem).css("display") !== "none"; }).first();
        var warrantText = activeTemplate.find("select").val();
        var prevText = $.trim($("#PrivContractor_Description").val());
        $("#PrivContractor_Description").val(prevText + (prevText.length >0 ? "\r\n" : "") + warrantText);
        privContractorWarrantModal.modal('hide');
        contractorModal.modal('show');
        e.preventDefault();
    });

    privContractorWarrantModal.on("hide.bs.modal", function () {
        contractorModal.modal('show');
    });

    privContractorWarrantModal.on("shown.bs.modal", function () {
        privContractorWarrantModal.find(".selectpicker").selectpicker("refresh");
    });

    function DatesDiffInYears(startDate, endDate) {
        if (startDate.getFullYear === undefined || endDate.getFullYear === undefined) return null;
        var years = endDate.getFullYear() - startDate.getFullYear();
        var monthEq = endDate.getMonth() > startDate.getMonth() ? 1 : endDate.getMonth() === startDate.getMonth() ? 0 : -1;
        var dayEq = endDate.getDate() > startDate.getDate() ? 1 : endDate.getDate() === startDate.getDate() ? 0 : -1;
        if (monthEq === 1 || (monthEq === 0 && dayEq >= 0)) return years;
        else return years - 1;
    }

    $.validator.addMethod('passportDateCorrect', function (value, element) {
        if (value.length === 0) return true;
        var matches = value.match("[0-9]{1,2}[\.][0-9]{1,2}[\.][0-9]{2,4}");
        if (matches === undefined || matches === null || matches.length === 0) return true;
        var match = matches[0];
        var dateParts = match.split('.');
        var day = dateParts[0];
        var month = dateParts[1];
        var year = dateParts[2];
        if (year.length === 2) {
            year = "20" + year;
        } else
            if (year.length === 3) return false;
        var passportDate = new Date(year, month - 1, day);
        var birthDate = $("#PrivContractor_DateBirth").val();
        if (birthDate === "" || birthDate === undefined) return true;
        var age = DatesDiffInYears(new Date(birthDate), new Date());
        var passportAge = DatesDiffInYears(new Date(birthDate), new Date(passportDate));
        if (age < 14) return true;
        if (age < 20 && passportAge >= 14 && passportAge < 20) return true;
        if (age < 45 && passportAge >= 20 && passportAge < 45) return true;
        if (age >= 45 && passportAge >= 45) return true;
        return false;
    }, 'Паспорт является недействующим');

    var contractorForm = $('#PrivContractorModalForm');
    contractorValidator = contractorForm.validate();
    contractorValidator.settings.rules["PrivContractor.Passport"].passportDateCorrect = true;
});