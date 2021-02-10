let CreateBuildingAttachmentFilesAssoc = function () {
    let attachmentFiles = getAttachmentFiles();
    let buildingAttachmentFilesAssoc = [];
    attachmentFiles.each(function (idx, item) {
        buildingAttachmentFilesAssoc.push({
            IdBuilding: 0,
            IdAttachment: 0,
            ObjectAttachmentFileNavigation: item
        });
    });
    return buildingAttachmentFilesAssoc;
};

function getAttachmentFiles() {
    return $("#attachmentFilesList .list-group-item").map(function (idx, elem) {
        return getAttachmentFile($(elem));
    });
}

function getAttachmentFile(attachmentFileElem) {
    return {
        IdAttachment: attachmentFileElem.find("[name^='IdAttachment']").val(),
        Description: attachmentFileElem.find("[name^='AttachmentFileDescription']").val(),
        AttachmentFile: attachmentFileElem.find("[name='AttachmentFile']")[0],
        AttachmentFileRemove: attachmentFileElem.find("[name^='AttachmentFileRemove']").val()
    };
}

function attachmentFileToFormData(attachmentFile, address) {
    var formData = new FormData();
    formData.append("Attachment.IdAttachment", attachmentFile.IdAttachment);
    formData.append("Attachment.Description", attachmentFile.Description);
    formData.append("AttachmentFile", attachmentFile.AttachmentFile.files[0]);
    formData.append("AttachmentFileRemove", attachmentFile.AttachmentFileRemove);
    formData.append("Address.AddressType", address.addressType);
    formData.append("Address.Id", address.id);
    return formData;
}

let getCurrentAddressAttachmentFiles = function () {
    let address = {
        addressType: $('#attachmentFilesList').data('addresstype'),
        id: $('#attachmentFilesList').data('id')
    };
    return address;
};

function deleteAttachmentFile(e) {
    let isOk = confirm("Вы уверены что хотите удалить документ?");
    if (isOk) {
        let attachmentFileElem = $(this).closest(".list-group-item");
        let idAttachment = attachmentFileElem.find("input[name^='IdAttachment']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/AttachmentFiles/DeleteAttachmentFile',
            data: { idAttachment: idAttachment },
            success: function (ind) {
                if (ind === 1) {
                    attachmentFileElem.remove();
                    
                    if ($("#attachmentFilesList").find('.list-group-item').length - 1 > 0)
                        $(".attachmentFilebadge").text($("#attachmentFilesList").find('.list-group-item').length - 1);
                    else
                        $(".attachmentFilebadge").css("display", "none");
                }
                else {
                    alert("Ошибка удаления!");
                }
            }
        });
    }
    e.preventDefault();
}

function addAttachmentFile(e) {
    let action = $('#attachmentFilesList').data('action');
    let addressType = $('#attachmentFilesList').data('addresstype');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/AttachmentFiles/AddAttachmentFile',
        data: { addressType, action },
        success: function (elem) {
            let list = $('#attachmentFilesList');
            let attachmentFilesToggle = $('#attachmentFilesToggle');
            if (!isExpandElemntArrow(attachmentFilesToggle)) // развернуть при добавлении, если было свернуто 
                attachmentFilesToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find(".attachment-file-edit-btn").click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
        }
    });
    e.preventDefault();
}

function editAttachmentFile(e) {
    let attachmentFile = $(this).closest(".list-group-item");
    let fields = attachmentFile.find('input, select, textarea');
    let yesNoPanel = attachmentFile.find('.yes-no-panel');
    let editDelPanel = attachmentFile.find('.edit-del-panel');
    fields.prop('disabled', false);
    editDelPanel.hide();
    yesNoPanel.show();
    showAttachmentFileEditFileBtns(attachmentFile,
        attachmentFile.find(".rr-attachment-file-download").length > 0 &&
        !attachmentFile.find(".rr-attachment-file-download").hasClass("disabled"));
    e.preventDefault();
}

function cancelEditAttachmentFile(e) {
    let attachmentFileElem = $(this).closest(".list-group-item");
    let idAttachment = attachmentFileElem.find("input[name^='IdAttachment']").val();
    //Отменить изменения внесенные в документ
    if (idAttachment !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/AttachmentFiles/GetAttachmentFile',
            data: { idAttachment: idAttachment },
            success: function (attachmentFile) {
                refreshAttachmentFile(attachmentFileElem, attachmentFile);
                showEditDelPanelAttachmentFile(attachmentFileElem);
                showAttachmentFileDownloadFileBtn(attachmentFileElem, attachmentFile.fileOriginName !== null);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        attachmentFileElem.remove();
    }
    e.preventDefault();
}

function showAttachmentFileEditFileBtns(attachmentFileElem, fileExists) {
    let attachmentFileBtns = attachmentFileElem.find(".rr-attachment-file-buttons");
    attachmentFileElem.find(".rr-attachment-file-download").hide();
    if (fileExists) {
        attachmentFileBtns.append(attachmentFileElem.find(".rr-attachment-file-remove").show());
        attachmentFileElem.find(".rr-attachment-file-attach").hide();
    } else {
        attachmentFileElem.find(".rr-attachment-file-remove").hide();
        attachmentFileBtns.append(attachmentFileElem.find(".rr-attachment-file-attach").show());
    }
}

function showAttachmentFileDownloadFileBtn(attachmentFileElem, fileExists) {
    let attachmentFileBtns = attachmentFileElem.find(".rr-attachment-file-buttons");
    attachmentFileBtns.append(attachmentFileElem.find(".rr-attachment-file-download").show());
    if (fileExists) {
        attachmentFileElem.find(".rr-attachment-file-download").removeClass("disabled");
    } else {
        attachmentFileElem.find(".rr-attachment-file-download").addClass("disabled");
    }
    attachmentFileElem.find(".rr-attachment-file-remove").hide();
    attachmentFileElem.find(".rr-attachment-file-attach").hide();
}

function showEditDelPanelAttachmentFile(attachmentFileElem) {
    let fields = attachmentFileElem.find('input, select, textarea');
    fields.prop('disabled', true);
    let editDelPanel = attachmentFileElem.find('.edit-del-panel');
    let yesNoPanel = attachmentFileElem.find('.yes-no-panel');
    yesNoPanel.hide();
    attachmentFileElem.removeClass("list-group-item-warning");
    editDelPanel.show();
}

function refreshAttachmentFile(attachmentFileElem, attachmentFile) {
    attachmentFileElem.find("[name^='AttachmentFileDescription']").val(attachmentFile.description);
    attachmentFileElem.find("[name='AttachmentFile']").val("");
    attachmentFileElem.find("[name^='AttachmentFileRemove']").val(false);
}

function saveAttachmentFile(e) {
    let attachmentFileElem = $(this).closest(".list-group-item");
    let attachmentFile = attachmentFileToFormData(getAttachmentFile(attachmentFileElem), getCurrentAddressAttachmentFiles());
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/AttachmentFiles/SaveAttachmentFile',
        data: attachmentFile,
        processData: false,
        contentType: false,
        success: function (attachmentFile) {
            if (attachmentFile.idAttachment > 0) {            

                if ($("#attachmentFilesList").find('.list-group-item').length - 1 > 0) {
                    $(".attachmentFilebadge").text($("#attachmentFilesList").find('.list-group-item').length + 1);
                    $(".attachmentFilebadge").css("display", "inline-block");
                }

                attachmentFileElem.find("input[name^='IdAttachment']").val(attachmentFile.idAttachment);
                attachmentFileElem.find(".rr-attachment-file-download")
                    .prop("href", "/AttachmentFiles/DownloadFile/?idAttachment=" + attachmentFile.idAttachment);
                showAttachmentFileDownloadFileBtn(attachmentFileElem, attachmentFile.fileOriginName !== null);
            }
            showEditDelPanelAttachmentFile(attachmentFileElem);
        }
    });
    e.preventDefault();
}

function attachAttachmentFile(e) {
    var attachmentFileElem = $(this).closest(".list-group-item");
    attachmentFileElem.find("input[name='AttachmentFile']").click();
    attachmentFileElem.find("input[name^='AttachmentFileRemove']").val(false);
    e.preventDefault();
}

function changeAttachmentFileAttachment() {
    var attachmentFileElem = $(this).closest(".list-group-item");
    if ($(this).val() !== "") {
        let attachmentFileBtns = attachmentFileElem.find(".rr-attachment-file-buttons");
        attachmentFileElem.find(".rr-attachment-file-attach").hide();
        attachmentFileBtns.append(attachmentFileElem.find(".rr-attachment-file-remove").show());
        var descriptionElem = attachmentFileElem.find("input[name='AttachmentFileDescription']");
        if (descriptionElem.val() === "") {
            descriptionElem.val($(this)[0].files[0].name);
        }
    }
}

function removeAttachmentFile(e) {
    var attachmentFileElem = $(this).closest(".list-group-item");
    attachmentFileElem.find("input[name='AttachmentFile']").val("");
    attachmentFileElem.find("input[name^='AttachmentFileRemove']").val(true);
    let attachmentFileBtns = attachmentFileElem.find(".rr-attachment-file-buttons");
    attachmentFileElem.find(".rr-attachment-file-remove").hide();
    attachmentFileBtns.append(attachmentFileElem.find(".rr-attachment-file-attach").show());
    e.preventDefault();
}

$(function () {
    $('.yes-no-panel').hide();
    $('#attachmentFileAdd').click(addAttachmentFile);
    $('#attachmentFilesToggle').on('click', $('#attachmentFilesList'), elementToogleHide);
    $('#attachmentFilesList').on('click', '.attachment-file-edit-btn', editAttachmentFile);
    $('#attachmentFilesList').on('click', '.attachment-file-cancel-btn', cancelEditAttachmentFile);
    $('#attachmentFilesList').on('click', '.attachment-file-save-btn', saveAttachmentFile);
    $('#attachmentFilesList').on('click', '.attachment-file-delete-btn', deleteAttachmentFile);
    $('#attachmentFilesList').on('click', '.rr-attachment-file-attach', attachAttachmentFile);
    $('#attachmentFilesList').on('click', '.rr-attachment-file-remove', removeAttachmentFile);
    $('#attachmentFilesList').on('change', "input[name='AttachmentFile']", changeAttachmentFileAttachment);

    if ($("#attachmentFilesList").find('.list-group-item').length == 0)
        $(".attachmentFilebadge").css("display", "none");
    
});