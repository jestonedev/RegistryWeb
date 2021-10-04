﻿$(function () {
    $.validator.addMethod('addressRequired', function (value, element) {
        return $('#privatizationForm [name="IdPremise"]').val() != "" ||
            $('#privatizationForm [name="IdSubPremise"]').val() != "";
    }, 'Блок «Адрес» является обязательным для заполнения');
    $.validator.addMethod('surnameContractorRequired', $.validator.methods.required,
        'Поле «Фамилия» является обязательным для заполнения');
    $.validator.addMethod('nameContractorRequired', $.validator.methods.required,
        'Поле «Имя» является обязательным для заполнения');
    $.validator.addMethod('birthdayContractorRequired', $.validator.methods.required,
        'Поле «День рождения» является обязательным для заполнения');

    $.validator.addClassRules('rr-сontractor-surname', {
        surnameContractorRequired: true
    });
    $.validator.addClassRules('rr-сontractor-name', {
        nameContractorRequired: true
    });
    $.validator.addClassRules('rr-сontractor-birthday', {
        birthdayContractorRequired: true
    });

    var form = $('#privatizationForm');
    var validator = form.validate();
    validator.settings.ignore = '.rr-valid-ignore';
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
    var action = form.attr('data-action');
    var contractorModal = $("#PrivContractorModal");
    var addressModal = $('#AddressRegistryModal');
    if (action == 'Details' || action == 'Delete') {
        $('select option:not(:selected)').attr('disabled', true);
        $('input').attr('readonly', true);
        $('textarea').attr('readonly', true);
        $('input:checkbox').attr('disabled', true);
    }
    $('#privContractInfo').hide();
    $('#privContractors select option:not(:selected)').attr('disabled', true);
    if ($('#privatizationForm [name="IdPremise"]').val() != "" ||
        $('#privatizationForm [name="IdSubPremise"]').val() != "") {
        $('#addressRegistryChangeBtn').attr('title', 'Редактировать');
        $('#addressRegistryChangeBtn span').addClass('oi-pencil');
    } else {
        $('#addressRegistryChangeBtn').attr('title', 'Добавить');
        $('#addressRegistryChangeBtn span').addClass('oi-plus');
        $('#HomesBtn').hide();
    }

    function privContractorFillModal(privContractorElem) {
        var fields = privContractorElem.find('input, select, textarea');
        contractorModal.find('.r-description').show();
        fields.each(function (idx, elem) {
            var name = $(elem).attr('name').split('_')[0];
            if (name == 'IsNoncontractor') {
                if ($(elem).val().toLowerCase() == 'false') {
                    contractorModal.find("[for='PrivContractor_IsNoncontractor']").text('Контрактор');
                    contractorModal.find("[name='PrivContractor.IsNoncontractor']").bootstrapToggle('on');
                    contractorModal.find("[for='PrivContractor_Description']").text('Текст доверенности');
                    contractorModal.find(".r-addition-fields-contractor").show();
                }
                else {
                    contractorModal.find("[for='PrivContractor_IsNoncontractor']").text('Не контрактор');
                    contractorModal.find("[name='PrivContractor.IsNoncontractor']").bootstrapToggle('off');
                    contractorModal.find("[for='PrivContractor_Description']").text('Причина не участия');
                    contractorModal.find('.r-addition-fields-contractor').hide();
                }
            }
            else if (name == 'HasDover') {
                if ($(elem).val().toLowerCase() == 'false' || $(elem).val() === "") {
                    contractorModal.find("[for='PrivContractor_HasDover']").text('Нет');
                    contractorModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('off');
                    if (contractorModal.find("[name='PrivContractor.IsNoncontractor']").is(':checked')) {
                        contractorModal.find('.r-description').hide();
                    }
                }
                else {
                    contractorModal.find("[for='PrivContractor_HasDover']").text('Есть');
                    contractorModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('on');
                }
            }
            else {
                contractorModal.find("[name='PrivContractor." + name + "']").val($(elem).val());
            }
        });
        var modalFields = contractorModal.find('input, select, textarea');
        if (action === 'Details' || action === 'Delete')
            modalFields.prop('disabled', 'disabled');
        else
            modalFields.prop('disabled', '');
    }
    function correctSnp(form) {
        $(form).find("[name^='Surname'],[name^='Name'],[name^='Patronymic']").each(function (idx, elem) {
            var value = $(elem).val();
            if (value.length > 0) {
                value = value[0].toUpperCase() + value.substring(1);
                $(elem).val(value);
            }
        });
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
    function privContractEditDetails(e) {
        var privContractorElem = $(this).closest('.list-group-item');
        privContractorFillModal(privContractorElem);
        contractorModal.modal('show');
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
            li.attr('title', 'Не контрактор');
            li.removeAttr('style');
        }
        else {
            li.attr('title', 'Контрактор');
            li.attr('style', 'border-left: 2px solid #ffc107;');
        }
    }
    function privContractorIsNoncontractorChange(e) {
        var baseElem = getPrivContractorBaseElem();
        contractorModal.find('.r-description').show();
        if ($(this).is(':checked')) {
            contractorModal.find("[for='PrivContractor_IsNoncontractor']").text('Контрактор');
            contractorModal.find("[for='PrivContractor_Description']").text('Текст доверенности');
            contractorModal.find('#PrivContractor_Passport').val(baseElem.find("[name^='Passport_']").val());
            contractorModal.find('#PrivContractor_Part').val(baseElem.find("[name^='Part_']").val());
            contractorModal.find('.r-addition-fields-contractor').show();
            if (baseElem.find("[name^='HasDover_']").val() == 'True') {
                contractorModal.find("[for='PrivContractor_HasDover']").text('Есть');
                contractorModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('on');
            }
            else {
                contractorModal.find("[for='PrivContractor_HasDover']").text('Нет');
                contractorModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('off');
                contractorModal.find('.r-description').hide();
            }
        }
        else {
            contractorModal.find("[for='PrivContractor_IsNoncontractor']").text('Не контрактор');
            contractorModal.find("[for='PrivContractor_Description']").text('Причина не участия');
            contractorModal.find('.r-addition-fields-contractor input').val('');
            contractorModal.find('.r-addition-fields-contractor').hide();
        }
        contractorModal.find('#PrivContractor_Description').val(baseElem.find("[name^='Description_']").val());
    }
    function privContractorHasDoverChange(e) {
        if (contractorModal.find("[name='PrivContractor.IsNoncontractor']").is(':checked')) {
            if ($(this).is(':checked')) {
                contractorModal.find("[for='PrivContractor_HasDover']").text('Есть');
                contractorModal.find('.r-description').show();
            }
            else {
                contractorModal.find("[for='PrivContractor_HasDover']").text('Нет');
                contractorModal.find("[name='PrivContractor.Description']").val('');
                contractorModal.find('.r-description').hide();
            }
        }
        e.preventDefault();
    }
    function privContractorModalSave(e) {
        var formModal = $('#PrivContractorModalForm');
        var validator = formModal.validate();
        var isValid = formModal.valid();
        console.log(isValid);
        if (isValid) {
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
            baseElem.find("[id^='User']").val(contractorModal.find("[name$='User']").val());
            baseElem.find("[id^='InsertDate']").val(contractorModal.find("[name$='InsertDate']").val());
            contractorModal.modal('hide');
        }         
        e.preventDefault();
    }
    function addressRegistryChange(e) {
        addressModal.modal('show');
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
                    $('#AddressRegistry_IdBuilding').val(elem.idParents.IdBuilding);
                    $('#AddressRegistry_IdPremise').val(elem.idParents.IdPremise);
                    $('#AddressRegistry_IdSubPremise').val(elem.idParents.IdSubPremise);
                    div.text(elem.text);
                    $('#setAddressRegistryModalBtn').removeAttr('disabled');
                }
            }
        });
        e.preventDefault();
    }
    function addressRegistryModalSet(e) {
        if ($('#addressRegistryChangeBtn span').hasClass('oi-plus')) {
            $('#addressRegistryChangeBtn span').addClass('oi-pencil').removeClass('oi-plus');
            $('#addressRegistryChangeBtn').attr('title', 'Редактировать');
            $('#HomesBtn').show();
        }
        $('#addressRegistryText').val($('#resultAddressRegistryModal').text());
        form.find('[name="IdPremise"]').val($('#AddressRegistry_IdPremise').val());
        form.find('[name="IdSubPremise"]').val($('#AddressRegistry_IdSubPremise').val());
        form.find('[href^="/Buildings/Details"]')
            .attr('href', '/Buildings/Details?idBuilding=' + $('#AddressRegistry_IdBuilding').val());
        form.find('[href^="/Premises/Details"]')
            .attr('href', '/Premises/Details?idPremises=' + $('#AddressRegistry_IdPremise').val());
        $('#resultAddressRegistryModal').text('');
        resetModalForm($("#AddressRegistryModalForm"));
        $('#setAddressRegistryModalBtn').attr('disabled', true);
        addressModal.modal('hide');
        e.preventDefault();
    }
    function addressRegistryModalClear(e) {
        $('#resultAddressRegistryModal').text('');
        resetModalForm($("#AddressRegistryModalForm"));
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
            success: function (elem) {
                $('#privContractors').append(elem);
                var li = $('#privContractors li').last();
                li.find('select').selectpicker('render');
                li.find('select option:not(:selected)').attr('disabled', true);
            }
        });
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
    $('#privatizationToggle').on('click', privatizationToggle);
    $('#privContractorsToggle').on('click', privContractorsToggle);
    $('#privContractInfoToggle').on('click', privContractInfoToggle);
    $('#PrivContractor_IsNoncontractor').on('change', privContractorIsNoncontractorChange);
    $('#PrivContractor_HasDover').on('change', privContractorHasDoverChange);
    $("#savePrivContractorModalBtn").on('click', privContractorModalSave);
    $("#privContractorAdd").on('click', privContractorAdd);
    $("#addressRegistryChangeBtn").on('click', addressRegistryChange);
    $("#searchAddressRegistryModalBtn").on('click', addressRegistryModalSearch);
    $("#setAddressRegistryModalBtn").on('click', addressRegistryModalSet);
    $("#clearAddressRegistryModalBtn").on('click', addressRegistryModalClear);
    $("#AddressRegistry_IdRegion").on('change', idRegionAddressRegistryModalChange);
    $('#privContractors').on('click','.priv-contract-edit-btn, .priv-contract-details-btn', privContractEditDetails);
    $('#privContractors').on('click', '.priv-contract-delete-btn', privContractorDelete);

    $('#privatizationCreate, #privatizationEdit').click(function (e) {
        e.preventDefault();
        var privContractors = $('#privContractors .list-group-item[data-id]');
        console.log(privContractors);
        form.find('select option').attr('disabled', false);
        privContractors.each(function (ind, elem) {
            var fields = $(elem).find('input, select, textarea');
            fields.each(function (i, el) {
                var name = $(el).attr('name').split('_')[0];
                $(el).attr('name', "PrivContractors[" + ind + "]." + name);
                if (name == 'IdContractor' && !/^\d+$/.test($(el).val())) {
                    $(el).val(0);
                }
            });
        });
        if (form.valid()) {
            form.submit();
        }
        //$('input[data-val="true"]')
        //    .removeAttr('aria-describedby')
        //    .removeAttr('aria-invalid');
    });
    $('#privatizationDelete').click(function (e) {
        e.preventDefault();
        form.submit();
    });    
});