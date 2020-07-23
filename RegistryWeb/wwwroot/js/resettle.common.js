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
    return $("#resettlesList > .list-group-item").map(function (idx, elem) {
        return getResettle($(elem));
    });
}

function getResettle(resettleElem) {
    return {
        IdResettleInfo: resettleElem.find("[name^='IdResettleInfo']").val(),
        ResettleDate: resettleElem.find("[name^='ResettleDate']").val(),
        IdResettleKind: resettleElem.find("[name^='IdResettleKindPlan']").val(),
        IdResettleKindFact: resettleElem.find("[name^='IdResettleKindFact']").val(),
        ResettleInfoSubPremisesFrom: $(resettleElem.find("[name^='SubPremisesFrom']").val()).map(function (idx, val) {
            return {
                IdSubPremises: val,
                IdResettleInfo: 0
            };
        }),
        FinanceSource1: resettleElem.find("[name^='FinanceSource1']").val(),
        FinanceSource2: resettleElem.find("[name^='FinanceSource2']").val(),
        FinanceSource3: resettleElem.find("[name^='FinanceSource3']").val(),
        FinanceSource4: resettleElem.find("[name^='FinanceSource4']").val(),
        ResettleInfoTo: GetRessetleInfoTo(resettleElem, ""),
        ResettleInfoToFact: GetRessetleInfoTo(resettleElem, "Fact"),
        ResettleDocuments: resettleElem.find("#resettleDocumentsList .list-group-item").map(function (idx, elem) {
            return {
                IdResettleInfo: 0,
                IdDocument: $(elem).find("[name^='IdDocument']").val(),
                Number: $(elem).find("[name^='ResettleDocumentNum']").val(),
                Date: $(elem).find("[name^='ResettleDocumentDate']").val(),
                Description: $(elem).find("[name^='ResettleDocumentDescription']").val(),
                IdDocumentType: $(elem).find("[name^='IdDocumentType']").val(),
                ResettleDocumentFile: $(elem).find("[name^='ResettleDocumentFile']")[0],
                ResettleDocumentFileRemove: $(elem).find("[name^='ResettleDocumentFileRemove']").val()
            };
        })
    };
}

function GetRessetleInfoTo(resettleElem, postfix) {
    var subPremisesSelect = resettleElem.find('select[name="ResettleToSubPremises' + postfix+'"]');
    var premisesSelect = resettleElem.find('select[name="ResettleToIdPremise' + postfix +'"]');
    var buildingsSelect = resettleElem.find('select[name="ResettleToIdBuilding' + postfix +'"]');
    if (subPremisesSelect.val().length !== 0) {
        return $(subPremisesSelect.val()).map(function (idx, val) {
            return {
                ObjectType: 'SubPremise',
                IdObject: val,
                IdResettleInfo: 0
            };
        });
    }
    if (premisesSelect.val() !== "") {
        return [{
            ObjectType: 'Premise',
            IdObject: premisesSelect.val(),
            IdResettleInfo: 0
        }];
    }
    if (buildingsSelect.val() !== "") {
        return [{
            ObjectType: 'Building',
            IdObject: buildingsSelect.val(),
            IdResettleInfo: 0
        }];
    }
    return null;
}

function resettleToFormData(resettle, address) {
    var formData = new FormData();
    formData.append("ResettleInfo.IdResettleInfo", resettle.IdResettleInfo);
    formData.append("ResettleInfo.ResettleDate", resettle.ResettleDate);
    formData.append("ResettleInfo.IdResettleKind", resettle.IdResettleKind);
    formData.append("ResettleInfo.IdResettleKindFact", resettle.IdResettleKindFact);
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
    if (resettle.ResettleInfoTo !== null) {
        for (let j = 0; j < resettle.ResettleInfoTo.length; j++) {
            formData.append("ResettleInfo.ResettleInfoTo[" + j + "].IdObject", resettle.ResettleInfoTo[j].IdObject);
            formData.append("ResettleInfo.ResettleInfoTo[" + j + "].ObjectType", resettle.ResettleInfoTo[j].ObjectType);
            formData.append("ResettleInfo.ResettleInfoTo[" + j + "].IdResettleInfo", resettle.ResettleInfoTo[j].IdResettleInfo);
        }
    }
    if (resettle.ResettleInfoToFact !== null) {
        for (let j = 0; j < resettle.ResettleInfoToFact.length; j++) {
            formData.append("ResettleInfo.ResettleInfoToFact[" + j + "].IdObject", resettle.ResettleInfoToFact[j].IdObject);
            formData.append("ResettleInfo.ResettleInfoToFact[" + j + "].ObjectType", resettle.ResettleInfoToFact[j].ObjectType);
            formData.append("ResettleInfo.ResettleInfoToFact[" + j + "].IdResettleInfo", resettle.ResettleInfoToFact[j].IdResettleInfo);
        }
    }
    if (resettle.ResettleDocuments !== null) {
        for (var k = 0; k < resettle.ResettleDocuments.length; k++) {
            formData.append("ResettleInfo.ResettleDocuments[" + k + "].IdDocument", resettle.ResettleDocuments[k].IdDocument);
            formData.append("ResettleInfo.ResettleDocuments[" + k + "].Number", resettle.ResettleDocuments[k].Number);
            formData.append("ResettleInfo.ResettleDocuments[" + k + "].Date", resettle.ResettleDocuments[k].Date);
            formData.append("ResettleInfo.ResettleDocuments[" + k + "].Description", resettle.ResettleDocuments[k].Description);
            formData.append("ResettleInfo.ResettleDocuments[" + k + "].IdDocumentType", resettle.ResettleDocuments[k].IdDocumentType);
            formData.append("ResettleDocumentFiles[" + k + "]", resettle.ResettleDocuments[k].ResettleDocumentFile.files[0]);
            formData.append("RestrictionFilesRemove[" + k + "]", resettle.ResettleDocuments[k].ResettleDocumentFileRemove);
        }
    }
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
        .attr('data-val-number', 'Введите числовое значение')
        .attr('id', fs1)
        .attr('name', fs1)
        .attr('aria-describedby', fs1 + '-error').closest(".input-group")
        .after(getErrorSpanResettles(fs1));

    let fs2 = 'FinanceSource2_' + idResettleInfo;
    resettleElem.find("[name^='FinanceSource2']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Областной бюджет" является обязательным')
        .attr('data-val-number', 'Введите числовое значение')
        .attr('id', fs2)
        .attr('name', fs2)
        .attr('aria-describedby', fs2 + '-error').closest(".input-group")
        .after(getErrorSpanResettles(fs2));

    let fs3 = 'FinanceSource3_' + idResettleInfo;
    resettleElem.find("[name^='FinanceSource3']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Муниципальный бюджет" является обязательным')
        .attr('data-val-number', 'Введите числовое значение')
        .attr('id', fs3)
        .attr('name', fs3)
        .attr('aria-describedby', fs3 + '-error').closest(".input-group")
        .after(getErrorSpanResettles(fs3));

    let fs4 = 'FinanceSource4_' + idResettleInfo;
    resettleElem.find("[name^='FinanceSource4']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Прочие источники" является обязательным')
        .attr('data-val-number', 'Введите числовое значение')
        .attr('id', fs4)
        .attr('name', fs4)
        .attr('aria-describedby', fs4 + '-error').closest(".input-group")
        .after(getErrorSpanResettles(fs4));

    resettleElem.find("#resettleDocumentsList .list-group-item").each(function (idx, elem) {
        initializeValidationResettleDocument($(elem));
    });

    refreshValidationResettlesForm();
};

function initializeValidationResettleDocument(documentElem) {
    let idResettleDocument = $(documentElem).find("input[name^='IdDocument']").val();
    if (idResettleDocument === "0") idResettleDocument = uuidv4();
    //Дата документа
    let date = 'ResettleDocumentDate_' + idResettleDocument;
    $(documentElem).find("[name^='ResettleDocumentDate']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Дата документа" является обязательным')
        .attr('id', date)
        .attr('name', date)
        .attr('aria-describedby', date + '-error')
        .after(getErrorSpanRestrictions(date));
    // Тип документа
    let idDocumentTypeName = 'IdDocumentType' + idResettleDocument;
    var documentTypeElem = $(documentElem).find("[name^='IdDocumentType']");
    documentTypeElem.addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Тип документа" является обязательным')
        .attr('id', idDocumentTypeName)
        .attr('name', idDocumentTypeName)
        .attr('aria-describedby', idDocumentTypeName + '-error').parent()
        .after(getErrorSpanRestrictions(idDocumentTypeName));
    documentTypeElem.next().attr("data-id", idDocumentTypeName);

    refreshValidationResettlesForm();
}

let refreshValidationResettlesForm = function () {
    var form = $("#resettlesForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

let initializeVilidationResettles = function () {
    let resettles = $('#resettlesList > .list-group-item');
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

            elem.find("#resettleDocumentsToggle").on('click', elem.find('#resettleDocumentsList'), elementToogleHide);

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


function addResettleDocument(e) {
    let action = $('#resettlesList').data('action');
    let addressType = $('#resettlesList').data('addresstype');
    let documentsList = $(this).closest(".card").find("#resettleDocumentsList");
    let toggleElem = $(this).closest(".card").find('#resettleDocumentsToggle');
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/Resettles/AddResettleDocument',
        data: { addressType, action },
        success: function (elem) {
            if (!isExpandElemntArrow(toggleElem)) // развернуть при добавлении, если было свернуто 
                toggleElem.click();

            documentsList.append(elem);
            elem = documentsList.find(".list-group-item").last();
            elem.find('input, select, textarea').prop("disabled", false);
            elem.find(".resettle-document-cancel-btn").removeClass("disabled");
            elem.find("select").selectpicker("refresh");
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            initializeValidationResettleDocument(elem);
            showResettleDocumentEditFileBtns(elem);
        }
    });
    e.preventDefault();
}

let resettleDocumentTemplateForCancelation = undefined;

function editResettle(e) {
    let resettle = $(this).closest(".list-group-item");
    let fields = resettle.find('input, select, textarea');
    let yesNoPanel = resettle.find('.yes-no-panel');
    let editDelPanel = resettle.find('.edit-del-panel');
    fields.filter(function (idx, val) { return !$(val).prop("name").startsWith("FinanceSourceTotal"); }).prop('disabled', false);
    resettle.find("select").selectpicker('refresh');
    resettle.find(".resettle-document-cancel-btn").removeClass("disabled");
    resettle.find("#resettleDocumentAdd").removeClass("disabled");
    editDelPanel.hide();
    yesNoPanel.show();
    showResettleEditFileBtns(resettle);
    let resettleDocumentElems = resettle.find("#resettleDocumentsList > .list-group-item");
    if (resettleDocumentElems.length > 0) {
        resettleDocumentTemplateForCancelation = resettleDocumentElems.first().clone();
    } else {
        resettleDocumentTemplateForCancelation = undefined;
    }
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
                showResettleDownloadFileBtns(resettleElem, resettle);
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
    let documentElems = resettleElem.find("#resettleDocumentsList > .list-group-item");
    documentElems.each(function (idx, documentElem) {
        showResettleDocumentEditFileBtns($(documentElem));
    });
}

function showResettleDocumentEditFileBtns(resettleDocumentElem) {
    let resettleDocumentFileBtns = resettleDocumentElem.find(".rr-resettle-document-file-buttons");
    resettleDocumentElem.find(".rr-resettle-document-file-download").hide();
    if (resettleDocumentElem.find(".rr-resettle-document-file-download").length > 0 &&
        !resettleDocumentElem.find(".rr-resettle-document-file-download").hasClass("disabled")) {
        resettleDocumentFileBtns.append(resettleDocumentElem.find(".rr-resettle-document-file-remove").show());
        resettleDocumentElem.find(".rr-resettle-document-file-attach").hide();
    } else {
        resettleDocumentElem.find(".rr-resettle-document-file-remove").hide();
        resettleDocumentFileBtns.append(resettleDocumentElem.find(".rr-resettle-document-file-attach").show());
    }
}

function showResettleDownloadFileBtns(resettleElem, resettle) {
    let documentElems = resettleElem.find("#resettleDocumentsList > .list-group-item");
    documentElems.each(function (idx, documentElem) {
        let resettleDocumentFileBtns = $(documentElem).find(".rr-resettle-document-file-buttons");
        resettleDocumentFileBtns.append($(documentElem).find(".rr-resettle-document-file-download").show());
        let documentId = parseInt($(documentElem).find("[name^=IdDocument]").val());
        let hasFile = false;
        for (let i = 0; i < resettle.documents.length; i++) {
            let document = resettle.documents[i];
            if (document.idDocument === documentId && documentId !== 0 && document.fileOriginName !== null) {

                    /*resettleElem.find(".rr-resettle-file-download")
                        .prop("href", "/Restrictions/DownloadFile/?idRestriction=" + restriction.idRestriction);*/

                $(documentElem).find(".rr-resettle-document-file-download").removeClass("disabled");
                hasFile = true;
            }
        }
        if (!hasFile) {
            $(documentElem).find(".rr-resettle-document-file-download").addClass("disabled");
        }
        $(documentElem).find(".rr-resettle-document-file-remove").hide();
        $(documentElem).find(".rr-resettle-document-file-attach").hide();
    });
}

function clearValidationsResettles(resettleElem) {
    $(resettleElem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
    $(resettleElem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
}

function showEditDelPanelResettle(resettleElem) {
    let fields = resettleElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    resettleElem.find("#resettleDocumentAdd").addClass("disabled");
    resettleElem.find(".resettle-document-cancel-btn").addClass("disabled");
    let editDelPanel = resettleElem.find('.edit-del-panel');
    let yesNoPanel = resettleElem.find('.yes-no-panel');
    yesNoPanel.hide();
    editDelPanel.show();
}

function refreshResettle(resettleElem, resettle) {
    resettleElem.find("[name^='ResettleDate']").val(resettle.resettleDate);
    resettleElem.find("[name^='IdResettleKindPlan']").val(resettle.idResettleKind).selectpicker('refresh');
    resettleElem.find("[name^='IdResettleKindFact']").val(resettle.idResettleKindFact).selectpicker('refresh');
    resettleElem.find("[name^='SubPremisesFrom']").val(resettle.subPremisesFrom).selectpicker('refresh');
    resettleElem.find("[name^='FinanceSource1']").val(resettle.financeSource1.toFixed(2).replace('.', ','));
    resettleElem.find("[name^='FinanceSource2']").val(resettle.financeSource2.toFixed(2).replace('.', ','));
    resettleElem.find("[name^='FinanceSource3']").val(resettle.financeSource3.toFixed(2).replace('.', ','));
    resettleElem.find("[name^='FinanceSource4']").val(resettle.financeSource4.toFixed(2).replace('.', ','));
    resettleElem.find("[name^='FinanceSource4']").val(resettle.financeSource4.toFixed(2).replace('.', ','));
    resettleElem.find('select[name="ResettleToIdStreet"]').val(resettleElem.find('input[name="ResettleToIdStreetPrev"]').val()).change().selectpicker('refresh');
    resettleElem.find('select[name="ResettleToIdStreetFact"]').val(resettleElem.find('input[name="ResettleToIdStreetFactPrev"]').val()).change().selectpicker('refresh');
    var documentListElem = resettleElem.find("#resettleDocumentsList");
    documentListElem.empty();
    if (resettleDocumentTemplateForCancelation !== undefined) {
        for (let i = 0; i < resettle.documents.length; i++) {
            let document = resettle.documents[i];
            let documentElem = resettleDocumentTemplateForCancelation.clone();
            documentListElem.append(documentElem);
            documentElem = documentListElem.find(".list-group-item").last();
            documentElem.find("[name^='IdDocument']").val(document.idDocument);
            documentElem.find("[name^='ResettleDocumentNum']").val(document.number);
            documentElem.find("[name^='ResettleDocumentDate']").val(document.date);
            documentElem.find("[name^='ResettleDocumentDescription']").val(document.description);
            var idDocumentTypeElem = documentElem.find('select[name^="IdDocumentType"]');
            var formGroup = idDocumentTypeElem.closest('.form-group');
            idDocumentTypeElem.closest('.bootstrap-select').remove();
            $(idDocumentTypeElem).insertAfter(formGroup.find("label"));
            idDocumentTypeElem.find("option[class='bs-title-option']").remove();
            idDocumentTypeElem.selectpicker().val(document.idDocumentType).selectpicker('refresh');

            let idResettleDocument = documentElem.find("input[name^='IdDocument']").val();
            if (idResettleDocument === "0") idResettleDocument = uuidv4();
            let date = 'ResettleDocumentDate_' + idResettleDocument;
            $(documentElem).find("[name^='ResettleDocumentDate']")
                .attr('id', date)
                .attr('name', date)
                .attr('aria-describedby', date + '-error').next().attr("data-valmsg-for", date);
            let idDocumentTypeName = 'IdDocumentType' + idResettleDocument;
            var documentTypeElem = $(documentElem).find("[name^='IdDocumentType']");
            documentTypeElem
                .attr('id', idDocumentTypeName)
                .attr('name', idDocumentTypeName)
                .attr('aria-describedby', idDocumentTypeName + '-error').parent()
                .next().attr("data-valmsg-for", idDocumentTypeName);
            documentTypeElem.next().attr("data-id", idDocumentTypeName);
        }
        refreshValidationResettlesForm();
    }
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

                    resettleElem.find('input[name="ResettleToIdStreetPrev"]').val(resettleElem.find('select[name="ResettleToIdStreet"]').val());
                    resettleElem.find('input[name="ResettleToIdBuildingPrev"]').val(resettleElem.find('select[name="ResettleToIdBuilding"]').val());
                    resettleElem.find('input[name="ResettleToIdPremisePrev"]').val(resettleElem.find('select[name="ResettleToIdPremise"]').val());
                    resettleElem.find('input[name="ResettleToSubPremisesPrev"]').remove();
                    var resettleToSubPremisesIds = resettleElem.find('select[name="ResettleToSubPremises"]').val();
                    for (let i = 0; i < resettleToSubPremisesIds.length; i++) {
                        resettleElem.append("<input type='hidden' name='ResettleToSubPremisesPrev' value='" + resettleToSubPremisesIds[i]+"'>");
                    }

                    resettleElem.find('input[name="ResettleToIdStreetFactPrev"]').val(resettleElem.find('select[name="ResettleToIdStreetFact"]').val());
                    resettleElem.find('input[name="ResettleToIdBuildingFactPrev"]').val(resettleElem.find('select[name="ResettleToIdBuildingFact"]').val());
                    resettleElem.find('input[name="ResettleToIdPremiseFactPrev"]').val(resettleElem.find('select[name="ResettleToIdPremiseFact"]').val());
                    resettleElem.find('input[name="ResettleToSubPremisesFactPrev"]').remove();
                    resettleToSubPremisesIds = resettleElem.find('select[name="ResettleToSubPremisesFact"]').val();
                    for (let i = 0; i < resettleToSubPremisesIds.length; i++) {
                        resettleElem.append("<input type='hidden' name='ResettleToSubPremisesFactPrev' value='" + resettleToSubPremisesIds[i] + "'>");
                    }

                    var resettleDocumentElems = resettleElem.find("#resettleDocumentsList .list-group-item");
                    for (var j = 0; j < resettleDocumentElems.length; j++) {
                        var document = resettle.documents[j];
                        $(resettleDocumentElems[j]).find("input[name^='IdDocument']").val(document.idDocument);
                        $(resettleDocumentElems[j]).find(".rr-resettle-document-file-download").attr("href", "/Resettles/DownloadFile/?idDocument=" + document.idDocument);
                        
                    }
                    showResettleDownloadFileBtns(resettleElem, resettle);
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
        if (resettleElem.find("#resettleDocumentsList .field-validation-error").length > 0) {
            if (!isExpandElemntArrow(resettleElem.find("#resettleDocumentsToggle"))) {
                resettleElem.find("#resettleDocumentsToggle").click();
            }
        }
        $([document.documentElement, document.body]).animate({
            scrollTop: resettleElem.find(".input-validation-error").first().offset().top - 35
        }, 1000);
    }
    e.preventDefault();
}

function attachResettleDocumentFile(e) {
    var resettleDocumentElem = $(this).closest(".list-group-item");
    resettleDocumentElem.find("input[name^='ResettleDocumentFile']").click();
    resettleDocumentElem.find("input[name^='ResettleDocumentFileRemove']").val(false);
    e.preventDefault();
}

function changeResettleDocumentFileAttachment() {
    var resettleDocumentElem = $(this).closest(".list-group-item");
    if ($(this).val() !== "") {
        let resettleDocumentFileBtns = resettleDocumentElem.find(".rr-resettle-document-file-buttons");
        resettleDocumentElem.find(".rr-resettle-document-file-attach").hide();
        resettleDocumentFileBtns.append(resettleDocumentElem.find(".rr-resettle-document-file-remove").show());
    }
}

function removeResettleDocumentFile(e) {
    var resettleDocumentElem = $(this).closest(".list-group-item");
    resettleDocumentElem.find("input[name^='ResettleDocumentFile']").val("");
    resettleDocumentElem.find("input[name^='ResettleDocumentFileRemove']").val(true);
    let resettleDocumentFileBtns = resettleDocumentElem.find(".rr-resettle-document-file-buttons");
    resettleDocumentElem.find(".rr-resettle-document-file-remove").hide();
    resettleDocumentFileBtns.append(resettleDocumentElem.find(".rr-resettle-document-file-attach").show());
    e.preventDefault();
}

function getResettleHouses() {
    var idStreet = $(this).val();
    var postfix = $(this).prop("name") === "ResettleToIdStreetFact" ? "Fact" : "";
    var resettleElem = $(this).closest(".list-group-item");
    var buildingToSelect = resettleElem.find('select[name="ResettleToIdBuilding' + postfix+'"]');
    var buildingPrevId = resettleElem.find('input[name="ResettleToIdBuilding' + postfix+'Prev"]').val();
    buildingToSelect.empty();
    buildingToSelect.append("<option></option>");
    buildingToSelect.selectpicker('refresh');
    $.getJSON('/Resettles/GetHouses', { idStreet: idStreet }, function (buildings) {
        $(buildings).each(function (idx, building) {
            var option = '<option value="' + building.idBuilding + '">' + building.house + '</option>';
            buildingToSelect.append(option);
        });
        buildingToSelect.val(buildingPrevId);
        buildingToSelect.selectpicker('refresh');
        buildingToSelect.change();
    });
}

function getResettlePremises() {
    var idBuilding = $(this).val();
    var postfix = $(this).prop("name") === "ResettleToIdBuildingFact" ? "Fact" : "";
    var resettleElem = $(this).closest(".list-group-item");
    var premiseToSelect = resettleElem.find('select[name="ResettleToIdPremise' + postfix +'"]');
    var premisePrevId = resettleElem.find('input[name="ResettleToIdPremise' + postfix +'Prev"]').val();
    premiseToSelect.empty();
    premiseToSelect.append("<option></option>");
    premiseToSelect.selectpicker('refresh');
    $.getJSON('/Resettles/GetPremises', { idBuilding: idBuilding }, function (premises) {
        $(premises).each(function (idx, premise) {
            var option = '<option value="' + premise.idPremises + '">' + premise.premisesNum + '</option>';
            premiseToSelect.append(option);
        });
        premiseToSelect.val(premisePrevId);
        premiseToSelect.selectpicker('refresh');
        premiseToSelect.change();
    });
}

function getResettleSubPremises() {
    var idPremise = $(this).val();
    var postfix = $(this).prop("name") === "ResettleToIdPremiseFact" ? "Fact" : "";
    var resettleElem = $(this).closest(".list-group-item");
    var subPremiseToSelect = resettleElem.find('select[name="ResettleToSubPremises' + postfix +'"]');
    var subPremisePrevIds = resettleElem.find('input[name="ResettleToSubPremises' + postfix +'Prev"]').map(function (idx, elem) { return $(elem).val() }).toArray();
    subPremiseToSelect.empty();
    subPremiseToSelect.selectpicker('refresh');
    $.getJSON('/Resettles/GetSubPremises', { idPremise: idPremise }, function (subPremises) {
        $(subPremises).each(function (idx, subPremise) {
            var option = '<option value="' + subPremise.idSubPremises + '">' + subPremise.subPremisesNum + '</option>';
            subPremiseToSelect.append(option);
        });
        subPremiseToSelect.val(subPremisePrevIds);
        subPremiseToSelect.selectpicker('refresh');
    });
}

function removeResettleDocument(e) {
    $(this).closest(".list-group-item").remove();
    e.preventDefault();
}

function toggleDocuments(e) {
    $("#resettleDocumentsList").toggle();
    e.preventDefault();
}

$(function () {
    $('.yes-no-panel').hide();
    initializeVilidationResettles();
    $('#resettleAdd').click(addResettle);
    $('#resettlesToggle').on('click', $('#resettlesList'), elementToogleHide);

    $('#resettlesList .list-group-item').each(function (idx, elem) {
        $(elem).find('#resettleDocumentsToggle').on('click', $(elem).find('#resettleDocumentsList'), elementToogleHide);
    });


    $('#resettlesList').on('click', '.resettle-edit-btn, .resettle-edit-btn-2', editResettle);
    $('#resettlesList').on('click', '.resettle-cancel-btn, .resettle-cancel-btn-2', cancelEditResettle);
    $('#resettlesList').on('click', '.resettle-save-btn, .resettle-save-btn-2', saveResettle);
    $('#resettlesList').on('click', '.resettle-delete-btn, .resettle-delete-btn-2', deleteResettle);
    $('#resettlesList').on('change', 'select[name="ResettleToIdStreet"], select[name="ResettleToIdStreetFact"]', getResettleHouses);
    $('#resettlesList').on('change', 'select[name="ResettleToIdBuilding"], select[name="ResettleToIdBuildingFact"]', getResettlePremises);
    $('#resettlesList').on('change', 'select[name="ResettleToIdPremise"], select[name="ResettleToIdPremiseFact"]', getResettleSubPremises);
    $('#resettlesList').on('click', '#resettleDocumentAdd', addResettleDocument);
    $('#resettlesList').on('click', '.resettle-document-cancel-btn', removeResettleDocument);
    $('#resettlesList').on('click', '.rr-resettle-document-file-attach', attachResettleDocumentFile);
    $('#resettlesList').on('click', '.rr-resettle-document-file-remove', removeResettleDocumentFile);
    $('#resettlesList').on('change', "input[name^='ResettleDocumentFile']", changeResettleDocumentFileAttachment);
});
