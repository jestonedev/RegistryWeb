let CreateLitigationPremisesAssoc = function () {
    let litigations = getLitigations();
    let litigationPremisesAssoc = [];
    litigations.each(function (idx, item) {
        litigationPremisesAssoc.push({
            IdPremises: 0,
            IdLitigation: 0,
            LitigationNavigation: item
        });
    });
    return litigationPremisesAssoc;
};

function getLitigations() {
    return $("#litigationsList .list-group-item").map(function (idx, elem) {
        return getLitigation($(elem));
    });
}

function getLitigation(litegationElem) {
    return {
        IdLitigation: litegationElem.find("[name^='IdLitigation']").val(),
        Number: litegationElem.find("[name^='LitigationNum']").val(),
        Date: litegationElem.find("[name^='LitigationDate']").val(),
        Description: litegationElem.find("[name^='LitigationDescription']").val(),
        IdLitigationType: litegationElem.find("[name^='IdLitigationType']").val(),
        LitigationFile: litegationElem.find("[name^='LitigationFile']")[0],
        LitigationFileRemove: litegationElem.find("[name^='LitigationFileRemove']").val()
    };
}

function litigationToFormData(litegationElem, address) {
    var formData = new FormData();
    formData.append("Litigation.IdLitigation", litegationElem.IdLitigation);
    formData.append("Litigation.Number", litegationElem.Number);
    formData.append("Litigation.Date", litegationElem.Date);
    formData.append("Litigation.Description", litegationElem.Description);
    formData.append("Litigation.IdLitigationType", litegationElem.IdLitigationType);
    formData.append("LitigationFile", litegationElem.LitigationFile.files[0]);
    formData.append("LitigationFileRemove", litegationElem.LitigationFileRemove);
    formData.append("Address.AddressType", address.addressType);
    formData.append("Address.Id", address.id);
    return formData;
}

let getCurrentAddressLitigations = function () {
    let address = {
        addressType: $('#litigationsList').data('addresstype'),
        id: $('#litigationsList').data('id')
    };
    return address;
};

let getErrorSpanLitigations = function (dataValmsgFor) {
    return "<span class=\"text-danger field-validation-valid\" data-valmsg-for=\"" + dataValmsgFor +
        "\" data-valmsg-replace=\"true\"></span>";
};

let initializeVilidationLitigation = function (litegationElem) {

    let idLitigation = litegationElem.find("input[name^='IdLitigation']").val();
    if (idLitigation === "0") idLitigation = uuidv4();
    //Дата документа
    let date = 'LitigationDate_' + idLitigation;
    litegationElem.find("[name^='LitigationDate']").addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Дата документа" является обязательным')
        .attr('id', date)
        .attr('name', date)
        .attr('aria-describedby', date + '-error')
        .after(getErrorSpanLitigations(date));
    // Тип документа
    let idLitigationTypeName = 'IdLitigationType_' + idLitigation;
    var litigationTypeElem = litegationElem.find("[name^='IdLitigationType']");
    litigationTypeElem.addClass('valid')
        .attr('data-val', 'true')
        .attr('data-val-required', 'Поле "Тип документа" является обязательным')
        .attr('id', idLitigationTypeName)
        .attr('name', idLitigationTypeName)
        .attr('aria-describedby', idLitigationTypeName + '-error').parent()
        .after(getErrorSpanLitigations(idLitigationTypeName));
    litigationTypeElem.next().attr("data-id", idLitigationTypeName);

    refreshValidationLitigationsForm();
};

let refreshValidationLitigationsForm = function () {
    var form = $("#litigationsForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

let initializeVilidationLitigations = function () {
    let litigations = $('#litigationsList .list-group-item');
    litigations.each(function () {
        initializeVilidationLitigation($(this));
    });
};

function deleteLitigation(e) {
    let isOk = confirm("Вы уверены что хотите удалить судебное разбирательство?");
    if (isOk) {
        let litegationElem = $(this).closest(".list-group-item");
        let idLitigation = litegationElem.find("input[name^='IdLitigation']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/Litigations/DeleteLitigation',
            data: { idLitigation: idLitigation },
            success: function (ind) {
                if (ind === 1) {
                    litegationElem.remove();

                    if ($("#litigationsList").find('.list-group-item').length > 0)
                        $(".litigationsbadge").text(String(parseInt($(".litigationsbadge").text()) - 1))
                    else {
                        $(".litigationsbadge").css("display", "none");
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

function addLitigation(e) {
    let action = $('#litigationsList').data('action');
    let addressType = $('#litigationsList').data('addresstype');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/Litigations/AddLitigation',
        data: { addressType, action },
        success: function (elem) {
            let list = $('#litigationsList');
            let litigationsToggle = $('#litigationsToggle');
            if (!isExpandElemntArrow(litigationsToggle)) // развернуть при добавлении, если было свернуто 
                litigationsToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find("select").selectpicker("refresh");
            elem.find(".litigation-edit-btn").click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            initializeVilidationLitigation(elem);
        }
    });
    e.preventDefault();
}

function editLitigation(e) {
    let litegationElem = $(this).closest(".list-group-item");
    let fields = litegationElem.find('input, select, textarea');
    let yesNoPanel = litegationElem.find('.yes-no-panel');
    let editDelPanel = litegationElem.find('.edit-del-panel');
    fields.prop('disabled', false);
    litegationElem.find("select").selectpicker('refresh');
    editDelPanel.hide();
    yesNoPanel.show();
    showLitigationEditFileBtns(litegationElem,
        litegationElem.find(".rr-litigation-file-download").length > 0 &&
        !litegationElem.find(".rr-litigation-file-download").hasClass("disabled"));
    e.preventDefault();
}

function cancelEditLitigation(e) {
    let litegationElem = $(this).closest(".list-group-item");
    let idLitigation = litegationElem.find("input[name^='IdLitigation']").val();
    //Отменить изменения внесенные в документ
    if (idLitigation !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Litigations/GetLitigation',
            data: { idLitigation: idLitigation },
            success: function (litegation) {
                refreshLitigation(litegationElem, litegation);
                showEditDelPanelLitigation(litegationElem);
                clearValidationsLitigations(litegationElem);
                showLitigationDownloadFileBtn(litegationElem, litegation.fileOriginName !== null);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        litegationElem.remove();
    }
    e.preventDefault();
}

function showLitigationEditFileBtns(litegationElem, fileExists) {
    let litegationFileBtns = litegationElem.find(".rr-litigation-file-buttons");
    litegationElem.find(".rr-litigation-file-download").hide();
    if (fileExists) {
        litegationFileBtns.append(litegationElem.find(".rr-litigation-file-remove").show());
        litegationElem.find(".rr-litigation-file-attach").hide();
    } else {
        litegationElem.find(".rr-litigation-file-remove").hide();
        litegationFileBtns.append(litegationElem.find(".rr-litigation-file-attach").show());
    }
}

function showLitigationDownloadFileBtn(litegationElem, fileExists) {
    let litegationFileBtns = litegationElem.find(".rr-litigation-file-buttons");
    litegationFileBtns.append(litegationElem.find(".rr-litigation-file-download").show());
    if (fileExists) {
        litegationElem.find(".rr-litigation-file-download").removeClass("disabled");
    } else {
        litegationElem.find(".rr-litigation-file-download").addClass("disabled");
    }
    litegationElem.find(".rr-litigation-file-remove").hide();
    litegationElem.find(".rr-litigation-file-attach").hide();
}

function clearValidationsLitigations(litegationElem) {
    $(litegationElem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
    $(litegationElem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
}

function showEditDelPanelLitigation(litegationElem) {
    let fields = litegationElem.find('input, select, textarea');
    fields.prop('disabled', true).selectpicker('refresh');
    let editDelPanel = litegationElem.find('.edit-del-panel');
    let yesNoPanel = litegationElem.find('.yes-no-panel');
    yesNoPanel.hide();
    litegationElem.removeClass("list-group-item-warning");
    editDelPanel.show();
}

function refreshLitigation(litegationElem, litegation) {
    litegationElem.find("[name^='LitigationNum']").val(litegation.number);
    litegationElem.find("[name^='LitigationDate']").val(litegation.date);
    litegationElem.find("[name^='LitigationDescription']").val(litegation.description);
    litegationElem.find("[name^='IdLitigationType']").val(litegation.idLitigationType).selectpicker('refresh');
    litegationElem.find("[name^='LitigationFile']").val("");
    litegationElem.find("[name^='LitigationFileRemove']").val(false);
}

function saveLitigation(e) {
    let litegationElem = $(this).closest(".list-group-item");
    litegationElem.find("button[data-id]").removeClass("input-validation-error");
    if (litegationElem.find("input, textarea, select").valid()) {
        let litegation = litigationToFormData(getLitigation(litegationElem), getCurrentAddressLitigations());
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Litigations/SaveLitigation',
            data: litegation,
            processData: false,
            contentType: false,
            success: function (litegation) {
                if (litegation.idLitigation > 0) {
                    litegationElem.find("input[name^='IdLitigation']").val(litegation.idLitigation);
                    litegationElem.find(".rr-litigation-file-download")
                        .prop("href", "/Litigations/DownloadFile/?idLitigation=" + litegation.idLitigation);
                    showLitigationDownloadFileBtn(litegationElem, litegation.fileOriginName !== null);
                }
                showEditDelPanelLitigation(litegationElem);

                if ($("#litigationsList").find('.list-group-item').length-1 > 0)
                    $(".litigationsbadge").text(String(parseInt($(".litigationsbadge").text()) + 1))
                else {
                    $(".litigationsbadge").css("display", "inline-block");
                    $(".litigationsbadge").text(1);
                }
            }
        });
    } else {
        litegationElem.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
        $([document.documentElement, document.body]).animate({
            scrollTop: litegationElem.find(".input-validation-error").first().offset().top - 35
        }, 1000);
    }
    e.preventDefault();
}

function attachLitigationFile(e) {
    var litegationElem = $(this).closest(".list-group-item");
    litegationElem.find("input[name^='LitigationFile']").click();
    litegationElem.find("input[name^='LitigationFileRemove']").val(false);
    e.preventDefault();
}

function changeLitigationFileAttachment() {
    var litegationElem = $(this).closest(".list-group-item");
    if ($(this).val() !== "") {
        let litigationFileBtns = litegationElem.find(".rr-litigation-file-buttons");
        litegationElem.find(".rr-litigation-file-attach").hide();
        litigationFileBtns.append(litegationElem.find(".rr-litigation-file-remove").show());
        var descriptionElem = litegationElem.find("input[name^='LitigationDescription']");
        if (descriptionElem.val() === "") {
            descriptionElem.val($(this)[0].files[0].name);
        }
    }
}

function removeLitigationFile(e) {
    var litegationElem = $(this).closest(".list-group-item");
    litegationElem.find("input[name^='LitigationFile']").val("");
    litegationElem.find("input[name^='LitigationFileRemove']").val(true);
    let litigationFileBtns = litegationElem.find(".rr-litigation-file-buttons");
    litegationElem.find(".rr-litigation-file-remove").hide();
    litigationFileBtns.append(litegationElem.find(".rr-litigation-file-attach").show());
    e.preventDefault();
}

$(function () {
    $('.yes-no-panel').hide();
    initializeVilidationLitigations();
    $('#litigationAdd').click(addLitigation);
    $('#litigationsToggle').on('click', $('#litigationsList'), elementToogleHide);
    $('#litigationsList').on('click', '.litigation-edit-btn', editLitigation);
    $('#litigationsList').on('click', '.litigation-cancel-btn', cancelEditLitigation);
    $('#litigationsList').on('click', '.litigation-save-btn', saveLitigation);
    $('#litigationsList').on('click', '.litigation-delete-btn', deleteLitigation);
    $('#litigationsList').on('click', '.rr-litigation-file-attach', attachLitigationFile);
    $('#litigationsList').on('click', '.rr-litigation-file-remove', removeLitigationFile);
    $('#litigationsList').on('change', "input[name^='LitigationFile']", changeLitigationFileAttachment);

    if ($("#litigationsList").find('.list-group-item').length == 0)
        $(".litigationsbadge").css("display", "none");
});