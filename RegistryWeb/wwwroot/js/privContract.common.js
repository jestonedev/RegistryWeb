$(function () {
    var action = $('#privatizationForm').attr('data-action');    
    var pModal = $("#PrivContractorModal");
    if (action == 'Details' || action == 'Delete') {
        $('select option:not(:selected)').attr('disabled', true);
        $('input').attr('readonly', true);
        $('textarea').attr('readonly', true);
        $('input:checkbox').attr('disabled', true);
    }
    $('#privContractInfo').hide();
    $('#privContractors select option:not(:selected)').attr('disabled', true);

    function privContractorFillModal(privContractorElem) {
        var fields = privContractorElem.find('input, select, textarea');
        pModal.find('.r-description').show();
        fields.each(function (idx, elem) {
            var name = $(elem).attr('name').split('_')[0];
            if (name == 'IsNoncontractor') {
                if ($(elem).val().toLowerCase() == 'false') {
                    pModal.find("[for='PrivContractor_IsNoncontractor']").text('Контрактор');
                    pModal.find("[name='PrivContractor.IsNoncontractor']").bootstrapToggle('on');
                    pModal.find("[for='PrivContractor_Description']").text('Текст доверенности');
                    pModal.find(".r-addition-fields-contractor").show();
                }
                else {
                    pModal.find("[for='PrivContractor_IsNoncontractor']").text('Не контрактор');
                    pModal.find("[name='PrivContractor.IsNoncontractor']").bootstrapToggle('off');
                    pModal.find("[for='PrivContractor_Description']").text('Причина не участия');
                    pModal.find('.r-addition-fields-contractor').hide();
                }
            }
            else if (name == 'HasDover') {
                if ($(elem).val().toLowerCase() == 'false' || $(elem).val() === "") {
                    pModal.find("[for='PrivContractor_HasDover']").text('Нет');
                    pModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('off');
                    if (pModal.find("[name='PrivContractor.IsNoncontractor']").is(':checked')) {
                        pModal.find('.r-description').hide();
                    }
                }
                else {
                    pModal.find("[for='PrivContractor_HasDover']").text('Есть');
                    pModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('on');
                }
            }
            else {
                pModal.find("[name='PrivContractor." + name + "']").val($(elem).val());
            }
        });
        var modalFields = pModal.find('input, select, textarea');
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
        pModal.modal('show');
        e.preventDefault();
    }
    function getPrivContractorBaseElem() {
        var idContractor = pModal.find('#PrivContractor_IdContractor').val();
        return $(".list-group-item input[name^='IdContractor']").filter(function (ind, elem) {
            return elem.value == idContractor;
        }).closest('.list-group-item');
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
        pModal.find('.r-description').show();
        if ($(this).is(':checked')) {
            pModal.find("[for='PrivContractor_IsNoncontractor']").text('Контрактор');
            pModal.find("[for='PrivContractor_Description']").text('Текст доверенности');
            pModal.find('#PrivContractor_Passport').val(baseElem.find("[name^='Passport_']").val());
            pModal.find('#PrivContractor_Part').val(baseElem.find("[name^='Part_']").val());
            pModal.find('.r-addition-fields-contractor').show();
            if (baseElem.find("[name^='HasDover_']").val() == 'True') {
                pModal.find("[for='PrivContractor_HasDover']").text('Есть');
                pModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('on');
            }
            else {
                pModal.find("[for='PrivContractor_HasDover']").text('Нет');
                pModal.find("[name='PrivContractor.HasDover']").bootstrapToggle('off');
                pModal.find('.r-description').hide();
            }
        }
        else {
            pModal.find("[for='PrivContractor_IsNoncontractor']").text('Не контрактор');
            pModal.find("[for='PrivContractor_Description']").text('Причина не участия');
            pModal.find('.r-addition-fields-contractor input').val('');
            pModal.find('.r-addition-fields-contractor').hide();
        }
        pModal.find('#PrivContractor_Description').val(baseElem.find("[name^='Description_']").val());
    }
    function privContractorHasDoverChange(e) {
        if (pModal.find("[name='PrivContractor.IsNoncontractor']").is(':checked')) {
            if ($(this).is(':checked')) {
                pModal.find("[for='PrivContractor_HasDover']").text('Есть');
                pModal.find('.r-description').show();
            }
            else {
                pModal.find("[for='PrivContractor_HasDover']").text('Нет');
                pModal.find("[name='PrivContractor.Description']").val('');
                pModal.find('.r-description').hide();
            }
        }
        e.preventDefault();
    }
    function privContractorModalSave(e) {
        var form = $('#PrivContractorModalForm');
        //var validator = form.validate();
        //var isValid = form.valid();
        //if (!tenancyPersonsCustomValidations(form, validator)) {
        //    isValid = false;
        //}
        var isValid = true;
        if (isValid) {
            var baseElem = getPrivContractorBaseElem();
            baseElem.find("[name^='Surname']").val(pModal.find("[name$='Surname']").val());
            baseElem.find("[name^='Name']").val(pModal.find("[name$='Name']").val());
            baseElem.find("[name^='Patronymic']").val(pModal.find("[name$='Patronymic']").val());
            baseElem.find("[name^='DateBirth']").val(pModal.find("[name$='DateBirth']").val());
            baseElem.find("[name^='IdKinship']").selectpicker('val', pModal.find("[name$='IdKinship']").val());
            baseElem.find("[name^='IsNoncontractor']").val(!pModal.find("[name$='IsNoncontractor']").is(':checked'));
            if (pModal.find("[name$='IsNoncontractor']").is(':checked')) {
                baseElem.find("[name^='Passport']").val(pModal.find("[name$='Passport']").val());
                baseElem.find("[name^='Part']").val(pModal.find("[name$='Part']").val());
                baseElem.find("[name^='HasDover']").val(pModal.find("[name$='HasDover']").is(':checked'));
                if (!pModal.find("[name$='HasDover']").is(':checked')) {
                    pModal.find("[name$='Description']").val('');
                }
                liIsNoncontractorChange(baseElem, false);
            }
            else {
                baseElem.find("[name^='Passport']").val('');
                baseElem.find("[name^='Part']").val('');
                baseElem.find("[name^='HasDover']").val('');
                liIsNoncontractorChange(baseElem, true);
            }
            baseElem.find("[name^='Description']").val(pModal.find("[name$='Description']").val());
            baseElem.find("[name^='User']").val(pModal.find("[name$='User']").val());
            baseElem.find("[name^='InsertDate']").val(pModal.find("[name$='InsertDate']").val());
        } else {
            form.find('select').each(function (idx, elem) {
                var id = $(elem).prop('id');
                var name = $(elem).prop('name');
                var errorSpan = $("span[data-valmsg-for='" + name + "']");
                if (errorSpan.hasClass("field-validation-error")) {
                    $("button[data-id='" + id + "']").addClass('input-validation-error');
                }
            });
        }
        pModal.modal('hide');
        e.preventDefault();
    }

    function addressRegistryChange(e) {
        console.log('addressRegistryChange');
        var addressModal = $('#AddressRegistryModal');
        //$("#IdStreet").on('change', function () {
        //    var idStreet = $(this).val();
        //    if (idStreet === "") return;
        //    $.getJSON('/Address/GetBuilding/?' + "idStreet=" + idStreet, function (data) {
        //        var options = "<option></option>";
        //        $(data).each(function (idx, elem) {
        //            options += "<option value=" + elem.idBuilding + ">" + elem.house + "</option>";
        //        });

        //        var buildingElem = $("select[name$='Premise.IdBuilding']");
        //        var idBuilding = $("input[name='IdBuildingPrev']").val();

        //        var disabled = (buildingElem.prop("disabled"));
        //        buildingElem.removeAttr("disabled");
        //        buildingElem.html(options).selectpicker('refresh').val(idBuilding).selectpicker('refresh');
        //        buildingElem.prop("disabled", disabled);
        //    });
        //});
        addressModal.modal('show');
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
    $('#privatizationToggle').on('click', privatizationToggle);
    $('#privContractorsToggle').on('click', privContractorsToggle);
    $('#privContractInfoToggle').on('click', privContractInfoToggle);
    $('#PrivContractor_IsNoncontractor').on('change', privContractorIsNoncontractorChange);
    $('#PrivContractor_HasDover').on('change', privContractorHasDoverChange);
    $("#savePrivContractorModalBtn").on('click', privContractorModalSave);
    $("#privContractorAdd").on('click', privContractorAdd);
    $("#addressRegistryChangeBtn").on('click', addressRegistryChange);
    $('#privContractors').on('click','.priv-contract-edit-btn, .priv-contract-details-btn', privContractEditDetails);
    $('#privContractors').on('click', '.priv-contract-delete-btn', privContractorDelete);

    $('#privatizationCreate, #privatizationEdit').click(function (e) {
        e.preventDefault();
        var form = $('#privatizationForm');
        var privContracts = $('#privContractors .list-group-item[data-id]');
        console.log(privContracts);
        form.find('select option').attr('disabled', false);
        privContracts.each(function (ind, elem) {
            var fields = $(elem).find('input, select, textarea');
            fields.each(function (i, el) {
                var name = $(el).attr('name').split('_')[0];
                $(el).attr('name', "PrivContractors[" + ind + "]." + name);
                if (name == 'IdContractor' && !/^\d+$/.test($(el).val())) {
                    $(el).val(0);
                }
            });
        });
        form.submit();
        //if (form.valid()) {
        //    form.submit();
        //}
        //$('input[data-val="true"]')
        //    .removeAttr('aria-describedby')
        //    .removeAttr('aria-invalid');
    });
    $('#privatizationDelete').click(function (e) {
        e.preventDefault();
        $('#privatizationForm').submit();
    });    
});