let CreateResettlePremisesAssoc = function () {
    let resettles = getResettles();
    let resettlePremisesAssoc = [];
    resettles.each(function (idx, item) {
        resettlePremisesAssoc.push({
            IdPremises: 0,
            IdResettleInfo: 0,
            ResettleInfoNavigation: item
        });
    });
    return resettlePremisesAssoc;
};

function getResettles() {
    return $("#resettlesList .list-group-item").map(function (idx, elem) {
        return getResettle($(elem));
    });
}

function getResettle(resettleElem) {
    return {
        IdResettleInfo: resettleElem.find("[name^='IdResettleInfo']").val(),
        ResettleDate: resettleElem.find("[name^='ResettleDate']").val(),
        IdResettleKind: resettleElem.find("[name^='IdResettleKind']").val(),
        ResettleInfoSubPremisesFrom: $(resettleElem.find("[name^='SubPremisesFrom']").val()).map(function (idx, val) {
            return {
                IdSubPremises: val,
                IdResettleInfo: 0
            };
        }),
        FinanceSource1: resettleElem.find("[name^='FinanceSource1']").val(),
        FinanceSource2: resettleElem.find("[name^='FinanceSource2']").val(),
        FinanceSource3: resettleElem.find("[name^='FinanceSource3']").val(),
        FinanceSource4: resettleElem.find("[name^='FinanceSource4']").val()
    };
}

function resettleToFormData(resettle, address) {
    var formData = new FormData();
    formData.append("ResettleInfo.IdResettleInfo", resettle.IdResettleInfo);
    formData.append("ResettleInfo.ResettleDate", resettle.ResettleDate);
    formData.append("ResettleInfo.IdResettleKind", resettle.IdResettleKind);
    for (var i = 0; i < resettle.ResettleInfoSubPremisesFrom.length; i++) {
        formData.append("ResettleInfo.ResettleInfoSubPremisesFrom["+i+"].IdSubPremise", resettle.ResettleInfoSubPremisesFrom[i].IdSubPremises);
        formData.append("ResettleInfo.ResettleInfoSubPremisesFrom[" + i +"].IdResettleInfo", resettle.ResettleInfoSubPremisesFrom[i].IdResettleInfo);
    }
    formData.append("ResettleInfo.FinanceSource1", resettle.FinanceSource1);
    formData.append("ResettleInfo.FinanceSource2", resettle.FinanceSource2);
    formData.append("ResettleInfo.FinanceSource3", resettle.FinanceSource3);
    formData.append("ResettleInfo.FinanceSource4", resettle.FinanceSource4);
    formData.append("Address.AddressType", address.addressType);
    formData.append("Address.Id", address.id);
    return formData;
}

let getCurrentAddressResettles = function () {
    let address = {
        addressType: $('#resettlesList').data('addresstype'),
        id: $('#resettlesList').data('id')
    };
    return address;
};

let getErrorSpanResettles = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
};

let initializeVilidationResettle = function (resettleElem) {

    let idResettleInfo = resettleElem.find("input[name^='IdResettleInfo']").val();
    if (idResettleInfo === "0") idResettleInfo = uuidv4();

    let fs1 = 'FinanceSource1_' + idResettleInfo;
    resettleElem.find("[name^='FinanceSource1']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Федеральный бюджет" является обязательным')
        .attr('id', fs1)
        .attr('name', fs1)
        .attr('aria-describedby', fs1 + '-error').next()
        .after(getErrorSpanResettles(fs1));

    let fs2 = 'FinanceSource2_' + idResettleInfo;
    resettleElem.find("[name^='FinanceSource2']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Областной бюджет" является обязательным')
        .attr('id', fs2)
        .attr('name', fs2)
        .attr('aria-describedby', fs2 + '-error').next()
        .after(getErrorSpanResettles(fs2));

    let fs3 = 'FinanceSource3_' + idResettleInfo;
    resettleElem.find("[name^='FinanceSource3']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Муниципальный бюджет" является обязательным')
        .attr('id', fs3)
        .attr('name', fs3)
        .attr('aria-describedby', fs3 + '-error').next()
        .after(getErrorSpanResettles(fs3));

    let fs4 = 'FinanceSource4_' + idResettleInfo;
    resettleElem.find("[name^='FinanceSource4']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Прочие источники" является обязательным')
        .attr('id', fs4)
        .attr('name', fs4)
        .attr('aria-describedby', fs4 + '-error').next()
        .after(getErrorSpanResettles(fs4));

    refreshValidationResettlesForm();
};

let refreshValidationResettlesForm = function () {
    var form = $("#resettlesForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

let initializeVilidationResettles = function () {
    let resettles = $('#resettlesList .list-group-item');
    resettles.each(function () {
        initializeVilidationResettle($(this));
    });
};

function deleteResettle(e) {
    let isOk = confirm("Вы уверены что хотите удалить информацию о переселении?");
    if (isOk) {
        let resettleElem = $(this).closest(".list-group-item");
        let idResettleInfo = resettleElem.find("input[name^='IdResettleInfo']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/Resettles/DeleteResettle',
            data: { idResettleInfo: idResettleInfo },
            success: function (ind) {
                if (ind === 1) {
                    resettleElem.remove();
                }
                else {
                    alert("Ошибка удаления!");
                }
            }
        });
    }
    e.preventDefault();
}

function addResettle(e) {
    let action = $('#resettlesList').data('action');
    let addressType = $('#resettlesList').data('addresstype');
    let id = $('#resettlesList').data('id');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/Resettles/AddResettle',
        data: { addressType, action, id },
        success: function (elem) {
            let list = $('#resettlesList');
            let resettlesToggle = $('#resettlesToggle');
            if (!isExpandElemntArrow(resettlesToggle)) // развернуть при добавлении, если было свернуто 
                resettlesToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find("select").selectpicker("refresh");
            elem.find(".resettle-edit-btn").click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            initializeVilidationResettle(elem);
        }
    });
    e.preventDefault();
}

function editResettle(e) {
    let resettle = $(this).closest(".list-group-item");
    let fields = resettle.find('input, select, textarea');
    let yesNoPanel = resettle.find('.yes-no-panel');
    let editDelPanel = resettle.find('.edit-del-panel');
    fields.filter(function (idx, val) { return !$(val).prop("name").startsWith("FinanceSourceTotal"); }).prop('disabled', false);
    resettle.find("select").selectpicker('refresh');
    editDelPanel.hide();
    yesNoPanel.show();
    showResettleEditFileBtns(resettle);  // TODO
    e.preventDefault();
}

function cancelEditResettle(e) {
    let resettleElem = $(this).closest(".list-group-item");
    let idResettleInfo = resettleElem.find("input[name^='IdResettleInfo']").val();
    //Отменить изменения внесенные в переселение
    if (idResettleInfo !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Resettles/GetResettle',
            data: { idResettleInfo: idResettleInfo },
            success: function (resettle) {
                refreshResettle(resettleElem, resettle);
                showEditDelPanelResettle(resettleElem);
                clearValidationsResettles(resettleElem);
                showResettleDownloadFileBtn(resettleElem, resettle);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        resettleElem.remove();
    }
    e.preventDefault();
}

function showResettleEditFileBtns(resettleElem) {
    /*let restrictionFileBtns = restrictionElem.find(".rr-restriction-file-buttons");
    restrictionElem.find(".rr-restriction-file-download").hide();
    if (fileExists) {
        restrictionFileBtns.append(restrictionElem.find(".rr-restriction-file-remove").show());
        restrictionElem.find(".rr-restriction-file-attach").hide();
    } else {
        restrictionElem.find(".rr-restriction-file-remove").hide();
        restrictionFileBtns.append(restrictionElem.find(".rr-restriction-file-attach").show());
    }*/
}

function showResettleDownloadFileBtn(resettleElem, resettle) {
    /*let restrictionFileBtns = restrictionElem.find(".rr-restriction-file-buttons");
    restrictionFileBtns.append(restrictionElem.find(".rr-restriction-file-download").show());
    if (fileExists) {
        restrictionElem.find(".rr-restriction-file-download").removeClass("disabled");
    } else {
        restrictionElem.find(".rr-restriction-file-download").addClass("disabled");
    }
    restrictionElem.find(".rr-restriction-file-remove").hide();
    restrictionElem.find(".rr-restriction-file-attach").hide();*/
}

function clearValidationsResettles(resettleElem) {
    $(resettleElem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
    $(resettleElem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
}

function showEditDelPanelResettle(resettleElem) {
    let fields = resettleElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = resettleElem.find('.edit-del-panel');
    let yesNoPanel = resettleElem.find('.yes-no-panel');
    yesNoPanel.hide();
    editDelPanel.show();
}

function refreshResettle(resettleElem, resettle) {
    resettleElem.find("[name^='ResettleDate']").val(resettle.resettleDate);
    resettleElem.find("[name^='IdResettleKind']").val(resettle.idResettleKind).selectpicker('refresh');
    resettleElem.find("[name^='SubPremisesFrom']").val(resettle.subPremisesFrom).selectpicker('refresh');
    resettleElem.find("[name^='FinanceSource1']").val(resettle.financeSource1.toFixed(2).replace('.', ','));
    resettleElem.find("[name^='FinanceSource2']").val(resettle.financeSource2.toFixed(2).replace('.', ','));
    resettleElem.find("[name^='FinanceSource3']").val(resettle.financeSource3.toFixed(2).replace('.', ','));
    resettleElem.find("[name^='FinanceSource4']").val(resettle.financeSource4.toFixed(2).replace('.', ','));
}

function saveResettle(e) {
    let resettleElem = $(this).closest(".list-group-item");
    resettleElem.find(".decimal").each(function (idx, elem) {
        $(elem).val($(elem).val().replace(".", ","));
    });
    var subPremises = resettleElem.find("[name^='SubPremisesFrom']");
    if (subPremises.val().indexOf("0") > -1 || subPremises.val().length === 0) {
        subPremises.val("0").selectpicker('refresh');
    }
    resettleElem.find("button[data-id]").removeClass("input-validation-error");
    if (resettleElem.find("input, textarea, select").valid()) {
        let resettle = resettleToFormData(getResettle(resettleElem), getCurrentAddressResettles());
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Resettles/SaveResettle',
            data: resettle,
            processData: false,
            contentType: false,
            success: function (resettle) {
                if (resettle.idResettleInfo > 0) {
                    resettleElem.find("input[name^='IdResettleInfo']").val(resettle.idResettleInfo);
                    //TODO
                    /*resettleElem.find(".rr-resettle-file-download")
                        .prop("href", "/Restrictions/DownloadFile/?idRestriction=" + restriction.idRestriction);*/
                    showResettleDownloadFileBtn(resettleElem, resettle);
                }
                showEditDelPanelResettle(resettleElem);
            }
        });
    } else {
        resettleElem.find("select").each(function (idx, elem) {
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

function attachResettleFile(e) {
    var resettleElem = $(this).closest(".list-group-item");
    // TODO
    /*resettleElem.find("input[name^='RestrictionFile']").click();
    resettleElem.find("input[name^='RestrictionFileRemove']").val(false);*/
    e.preventDefault();
}

function changeResettleFileAttachment() {
    var resettleElem = $(this).closest(".list-group-item");
    /*if ($(this).val() !== "") {
        let restrictionFileBtns = resettleElem.find(".rr-restriction-file-buttons");
        resettleElem.find(".rr-restriction-file-attach").hide();
        restrictionFileBtns.append(resettleElem.find(".rr-restriction-file-remove").show());
    }*/
}

function removeResettleFile(e) {
    /*var restrictionElem = $(this).closest(".list-group-item");
    restrictionElem.find("input[name^='RestrictionFile']").val("");
    restrictionElem.find("input[name^='RestrictionFileRemove']").val(true);
    let restrictionFileBtns = restrictionElem.find(".rr-restriction-file-buttons");
    restrictionElem.find(".rr-restriction-file-remove").hide();
    restrictionFileBtns.append(restrictionElem.find(".rr-restriction-file-attach").show());
    e.preventDefault();*/
}

$(function () {
    $('#resettlesList').hide();
    $('.yes-no-panel').hide();
    initializeVilidationResettles();
    $('#resettleAdd').click(addResettle);
    $('#resettlesToggle').on('click', $('#resettlesList'), elementToogle);
    $('#resettlesList').on('click', '.resettle-edit-btn, .resettle-edit-btn-2', editResettle);
    $('#resettlesList').on('click', '.resettle-cancel-btn, .resettle-cancel-btn-2', cancelEditResettle);
    $('#resettlesList').on('click', '.resettle-save-btn, .resettle-save-btn-2', saveResettle);
    $('#resettlesList').on('click', '.resettle-delete-btn, .resettle-delete-btn-2', deleteResettle);
    /*$('#resettlesList').on('click', '.rr-resettle-file-attach', attachResettleFile);
    $('#resettlesList').on('click', '.rr-resettle-file-remove', removeResettleFile);
    $('#resettlesList').on('change', "input[name^='ResettleFile']", changeResettleFileAttachment);*/
});
