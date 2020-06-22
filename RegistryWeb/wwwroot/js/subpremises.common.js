function editSubPremise(e) {
    let subPremise = $(this).closest(".list-group-item");
    let fields = subPremise.find('input, select, textarea');
    let yesNoPanel = subPremise.find('.yes-no-panel');
    let editDelPanel = subPremise.find('.edit-del-panel');
    fields.filter(function (idx, val) { return !$(val).prop("name").startsWith("IdFundType"); }).prop('disabled', false);
    subPremise.find("select").selectpicker('refresh');
    editDelPanel.hide();
    yesNoPanel.show();
    e.preventDefault();
}

function cancelEditSubPremise(e) {
    let subPremiseElem = $(this).closest(".list-group-item");
    let idSubPremise = subPremiseElem.find("input[name^='IdSubPremise']").val();
    //Отменить изменения внесенные в комнату
    if (idSubPremise !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/SubPremises/GetSubPremise',
            data: { idSubPremise: idSubPremise },
            success: function (subPremise) {
                refreshSubPremise(subPremiseElem, subPremise);
                showEditDelPanelSubPremise(subPremiseElem);
                clearValidationsSubPremises(subPremiseElem);
            }
        });
    }
    //Отменить вставку новой компнаты
    else {
        subPremiseElem.remove();
    }
    e.preventDefault();
}

function clearValidationsSubPremises(subPremiseElem) {
    $(subPremiseElem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
    $(subPremiseElem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
}

function showEditDelPanelSubPremise(subPremiseElem) {
    let fields = subPremiseElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = subPremiseElem.find('.edit-del-panel');
    let yesNoPanel = subPremiseElem.find('.yes-no-panel');
    yesNoPanel.hide();
    editDelPanel.show();
}

function refreshSubPremise(subPremiseElem, subPremise) {
    subPremiseElem.find("[name^='SubPremisesNum']").val(subPremise.subPremisesNum);
    subPremiseElem.find("[name^='TotalArea']").val(subPremise.totalArea);
    subPremiseElem.find("[name^='IdState']").val(subPremise.idState).selectpicker('refresh');
    subPremiseElem.find("[name^='LivingArea']").val(subPremise.livingArea);
    subPremiseElem.find("[name^='Description']").val(subPremise.description);
    subPremiseElem.find("[name^='CadastralNum']").val(subPremise.cadastralNum);
    subPremiseElem.find("[name^='CadastralCost']").val(subPremise.cadastralCost.toFixed(2).replace('.', ','));
    subPremiseElem.find("[name^='BalanceCost']").val(subPremise.balanceCost.toFixed(2).replace('.', ','));
    subPremiseElem.find("[name^='Account']").val(subPremise.account);
}

function saveSubPremise(e) {
    $('#subpremisesForm').find(".decimal").each(function (idx, elem) {
        $(elem).val($(elem).val().replace(".", ","));
    });
    let subPremiseElem = $(this).closest(".list-group-item");
    subPremiseElem.find("button[data-id]").removeClass("input-validation-error");
    if (subPremiseElem.find("input, textarea, select").valid()) {
        let subPremise = getSubPremise(subPremiseElem);
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/SubPremises/SaveSubPremise',
            data: { subPremise, idFundType: subPremise.IdFundType },
            success: function (idSubPremise) {
                if (idSubPremise > 0) {
                    subPremiseElem.find("input[name^='IdSubPremise']").val(idSubPremise);
                }
                showEditDelPanelSubPremise(subPremiseElem);
            }
        });
    } else {
        subPremiseElem.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
    }

    e.preventDefault();
}

function getSubPremise(subPremiseElem) {
    return {
        IdSubPremises: subPremiseElem.find("[name^='IdSubPremise']").val(),
        IdPremises: subPremiseElem.find("[name^='IdPremise']").val(),
        SubPremisesNum: subPremiseElem.find("[name^='SubPremisesNum']").val(),
        TotalArea: subPremiseElem.find("[name^='TotalArea']").val(),
        IdState: subPremiseElem.find("[name^='IdState']").val(),
        LivingArea: subPremiseElem.find("[name^='LivingArea']").val(),
        Description: subPremiseElem.find("[name^='Description']").val(),
        CadastralNum: subPremiseElem.find("[name^='CadastralNum']").val(),
        CadastralCost: subPremiseElem.find("[name^='CadastralCost']").val(),
        BalanceCost: subPremiseElem.find("[name^='BalanceCost']").val(),
        Account: subPremiseElem.find("[name^='Account']").val(),
        IdFundType: subPremiseElem.find("[name^='IdFundType']").prop("disabled") ? null :
            subPremiseElem.find("[name^='IdFundType']").val()
    };
}

function getSubPremises() {
    return $("#subPremisesList .list-group-item").map(function (idx, elem) {
        return getSubPremise($(elem));
    });
}

function deleteSubPremise(e) {
    let isOk = confirm("Вы уверены что хотите удалить комнату?");
    if (isOk) {
        let subPremiseElem = $(this).closest(".list-group-item");
        let idSubPremise = subPremiseElem.find("input[name^='IdSubPremise']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/SubPremises/DeleteSubPremise',
            data: { idSubPremise: idSubPremise },
            success: function (ind) {
                if (ind === 1) {
                    subPremiseElem.remove();
                }
                else {
                    alert("Ошибка удаления!");
                }
            }
        });
    }

    e.preventDefault();
}

function addSubPremise(e) {
    let action = $('#subPremisesList').data('action');
    let premiseId = $('#subPremisesList').data('premise-id');
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/SubPremises/AddSubPremise',
        data: { action },
        success: function (elem) {
            let list = $('#subPremisesList');
            let subPremisesToggle = $('#subPremisesToggle');
            if (!isExpandElemntArrow(subPremisesToggle)) // развернуть при добавлении, если было свернуто 
                subPremisesToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find("[name^='IdPremise']").val(premiseId);
            elem.find("[name^='IdFundType']").prop('disabled', false);
            elem.find("select").selectpicker("refresh");
            elem.find(".subpremise-edit-btn").click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            initializeVilidationSubPremise(elem);
        }
    });
    e.preventDefault();
}

let getErrorSpanSubPremises = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
};

function initializeVilidationSubPremise(subPremiseElem) {
    let idSubPremise = subPremiseElem.find("input[name^='IdSubPremise']").val();
    if (idSubPremise === "0") idSubPremise = uuidv4();
    //Общая площадь
    let totalAreaName = 'TotalArea_' + idSubPremise;
    subPremiseElem.find("[name^='TotalArea']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Общая площадь" является обязательным')
        .attr('data-val-number', 'Введите числовое значение')
        .attr('id', totalAreaName)
        .attr('name', totalAreaName)
        .attr('aria-describedby', totalAreaName + '-error')
        .after(getErrorSpanSubPremises(totalAreaName));
    // Жилая площадь
    let livingAreaName = 'LivingArea_' + idSubPremise;
    subPremiseElem.find("[name^='LivingArea']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Жилая площадь" является обязательным')
        .attr('data-val-number', 'Введите числовое значение')
        .attr('id', livingAreaName)
        .attr('name', livingAreaName)
        .attr('aria-describedby', livingAreaName + '-error')
        .after(getErrorSpanSubPremises(livingAreaName));
    // Текущее состояние
    let idStateName = 'IdState_' + idSubPremise;
    var stateElem = subPremiseElem.find("[name^='IdState']");
    stateElem.addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Текущее состояние" является обязательным')
        .attr('id', idStateName)
        .attr('name', idStateName)
        .attr('aria-describedby', idStateName + '-error').parent()
        .after(getErrorSpanSubPremises(idStateName));
    stateElem.next().attr("data-id", idStateName);
    
    // Кадастровая стоимость
    let cadastralCostName = 'CadastralCost_' + idSubPremise;
    subPremiseElem.find("[name^='CadastralCost']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Кадастровая стоимость" является обязательным')
        .attr('data-val-number', 'Введите числовое значение')
        .attr('id', cadastralCostName)
        .attr('name', cadastralCostName)
        .attr('aria-describedby', cadastralCostName + '-error')
        .after(getErrorSpanSubPremises(cadastralCostName));
    // Балансовая стоимость
    let balanceCostName = 'BalanceCost_' + idSubPremise;
    subPremiseElem.find("[name^='BalanceCost']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Балансовая стоимость" является обязательным')
        .attr('data-val-number', 'Введите числовое значение')
        .attr('id', balanceCostName)
        .attr('name', balanceCostName)
        .attr('aria-describedby', balanceCostName + '-error')
        .after(getErrorSpanSubPremises(balanceCostName));

    refreshValidationSubPremiseesForm();
}

function initializeValidationSubPremises() {
    let subPremiseElems = $('#subPremisesList .list-group-item');
    subPremiseElems.each(function () {
        initializeVilidationSubPremise($(this));
    });
}

let refreshValidationSubPremiseesForm = function () {
    var form = $("#subpremisesForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};


$(function () {
    $('#subPremisesList').hide();
    $('.yes-no-panel').hide();
    initializeValidationSubPremises();
    $('#subPremiseAdd').click(addSubPremise);
    $('#subPremisesToggle').on('click', $('#subPremisesList'), elementToogle);
    $('#subPremisesList').on('click', '.subpremise-edit-btn', editSubPremise);
    $('#subPremisesList').on('click', '.subpremise-cancel-btn', cancelEditSubPremise);
    $('#subPremisesList').on('click', '.subpremise-save-btn', saveSubPremise);
    $('#subPremisesList').on('click', '.subpremise-delete-btn', deleteSubPremise);
});