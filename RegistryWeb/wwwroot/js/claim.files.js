function getClaimFiles() {
    var items = $("#ClaimFiles .list-group-item").filter(function (idx, elem) {
        return !$(elem).hasClass("rr-list-group-item-empty");
    });

    return items.map(function (idx, elem) {
        return getClaimFile($(elem));
    });
}

function getClaimFile(attachmentFileElem) {
    return {
        IdFile: attachmentFileElem.find("[name^='IdFile']").val(),
        IdClaim: $("#ClaimFiles").data("id"),
        Description: attachmentFileElem.find("[name^='ClaimFileDescription']").val(),
        AttachmentFile: attachmentFileElem.find("[name^='ClaimFile_']")[0],
        AttachmentFileRemove: attachmentFileElem.find("[name^='ClaimFileRemove']").val()
    };
}

function claimFileToFormData(attachmentFile) {
    var formData = new FormData();
    formData.append("File.IdFile", attachmentFile.IdFile);
    formData.append("File.IdClaim", attachmentFile.IdClaim);
    formData.append("File.Description", attachmentFile.Description);
    formData.append("AttachmentFile", attachmentFile.AttachmentFile.files[0]);
    formData.append("AttachmentFileRemove", attachmentFile.AttachmentFileRemove);
    return formData;
}

let getCurrentAddressAttachmentFiles = function () {
    let address = {
        addressType: $('#attachmentFilesList').data('addresstype'),
        id: $('#attachmentFilesList').data('id')
    };
    return address;
};

function deleteClaimFile(e) {
    let isOk = confirm("Вы уверены что хотите удалить документ?");
    if (isOk) {
        let attachmentFileElem = $(this).closest(".list-group-item");
        let idFile = attachmentFileElem.find("input[name^='IdFile']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/ClaimFiles/DeleteClaimFile',
            data: { idFile: idFile },
            success: function (ind) {
                if (ind === 1) {
                    attachmentFileElem.remove();
                    if ($("#ClaimFiles .list-group-item").length === 1) {
                        $("#ClaimFiles .rr-list-group-item-empty").show();
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

function addClaimFile(e) {
    let action = $('#ClaimFiles').data('action');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/Claims/AddClaimFile',
        data: { action },
        success: function (elem) {
            let list = $('#ClaimFiles');
            list.find(".rr-list-group-item-empty").hide();
            let attachmentFilesToggle = $('#ClaimFilesForm .claim-toggler');
            if (!isExpandElemntArrow(attachmentFilesToggle)) // развернуть при добавлении, если было свернуто 
                attachmentFilesToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find(".claim-file-edit-btn").click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
        }
    });
    e.preventDefault();
}

function editClaimFile(e) {
    let attachmentFile = $(this).closest(".list-group-item");
    let fields = attachmentFile.find('input, select, textarea');
    let yesNoPanel = attachmentFile.find('.yes-no-panel');
    let editDelPanel = attachmentFile.find('.edit-del-panel');
    fields.prop('disabled', false);
    editDelPanel.hide();
    yesNoPanel.show();
    showClaimFileEditFileBtns(attachmentFile,
        attachmentFile.find(".rr-claim-file-download").length > 0 &&
        !attachmentFile.find(".rr-claim-file-download").hasClass("disabled"));
    e.preventDefault();
}

function cancelEditClaimFile(e) {
    let attachmentFileElem = $(this).closest(".list-group-item");
    let idFile = attachmentFileElem.find("input[name^='IdFile']").val();
    //Отменить изменения внесенные в документ
    if (idFile !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/ClaimFiles/GetClaimFile',
            data: { idFile: idFile },
            success: function (attachmentFile) {
                refreshClaimFile(attachmentFileElem, attachmentFile);
                showEditDelPanelClaimFile(attachmentFileElem);
                showClaimFileDownloadFileBtn(attachmentFileElem, attachmentFile.fileOriginName !== "" && attachmentFile.fileOriginName !== null);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        attachmentFileElem.remove();
        if ($("#ClaimFiles .list-group-item").length === 1) {
            $("#ClaimFiles .rr-list-group-item-empty").show();
        }
    }
    e.preventDefault();
}

function showClaimFileEditFileBtns(attachmentFileElem, fileExists) {
    let attachmentFileBtns = attachmentFileElem.find(".rr-claim-file-buttons");
    attachmentFileElem.find(".rr-claim-file-download").hide();
    if (fileExists) {
        attachmentFileBtns.append(attachmentFileElem.find(".rr-claim-file-remove").show());
        attachmentFileElem.find(".rr-claim-file-attach").hide();
    } else {
        attachmentFileElem.find(".rr-claim-file-remove").hide();
        attachmentFileBtns.append(attachmentFileElem.find(".rr-claim-file-attach").show());
    }
}

function showClaimFileDownloadFileBtn(attachmentFileElem, fileExists) {
    let attachmentFileBtns = attachmentFileElem.find(".rr-claim-file-buttons");
    attachmentFileBtns.append(attachmentFileElem.find(".rr-claim-file-download").show());
    if (fileExists) {
        attachmentFileElem.find(".rr-claim-file-download").removeClass("disabled");
    } else {
        attachmentFileElem.find(".rr-claim-file-download").addClass("disabled");
    }
    attachmentFileElem.find(".rr-claim-file-remove").hide();
    attachmentFileElem.find(".rr-claim-file-attach").hide();
}

function showEditDelPanelClaimFile(attachmentFileElem) {
    let fields = attachmentFileElem.find('input, select, textarea');
    fields.prop('disabled', true);
    let editDelPanel = attachmentFileElem.find('.edit-del-panel');
    let yesNoPanel = attachmentFileElem.find('.yes-no-panel');
    yesNoPanel.hide();
    attachmentFileElem.removeClass("list-group-item-warning");
    editDelPanel.show();
}

function refreshClaimFile(attachmentFileElem, attachmentFile) {
    attachmentFileElem.find("[name^='ClaimFileDescription']").val(attachmentFile.description);
    attachmentFileElem.find("[name^='ClaimFile_']").val("");
    attachmentFileElem.find("[name^='ClaimFileRemove']").val(false);
}

function saveClaimFile(e) {
    let attachmentFileElem = $(this).closest(".list-group-item");
    let attachmentFile = claimFileToFormData(getClaimFile(attachmentFileElem));
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/ClaimFiles/SaveClaimFile',
        data: attachmentFile,
        processData: false,
        contentType: false,
        success: function (attachmentFile) {
            if (attachmentFile.idFile > 0) {
                attachmentFileElem.find("input[name^='IdFile']").val(attachmentFile.idFile);
                attachmentFileElem.find(".rr-claim-file-download")
                    .prop("href", "/ClaimFiles/DownloadFile/?idFile=" + attachmentFile.idFile);
                showClaimFileDownloadFileBtn(attachmentFileElem, attachmentFile.fileName !== "" && attachmentFile.fileName !== null);
            }
            showEditDelPanelClaimFile(attachmentFileElem);
        }
    });
    e.preventDefault();
}

function attachClaimFile(e) {
    var attachmentFileElem = $(this).closest(".list-group-item");
    attachmentFileElem.find("input[name^='ClaimFile_']").click();
    attachmentFileElem.find("input[name^='ClaimFileRemove']").val(false);
    e.preventDefault();
}

function changeClaimFileAttachment() {
    var attachmentFileElem = $(this).closest(".list-group-item");
    if ($(this).val() !== "") {
        let attachmentFileBtns = attachmentFileElem.find(".rr-claim-file-buttons");
        attachmentFileElem.find(".rr-claim-file-attach").hide();
        attachmentFileBtns.append(attachmentFileElem.find(".rr-claim-file-remove").show());
        var descriptionElem = attachmentFileElem.find("input[name^='ClaimFileDescription']");
        if (descriptionElem.val() === "") {
            descriptionElem.val($(this)[0].files[0].name);
        }
    }
}

function removeClaimFile(e) {
    var attachmentFileElem = $(this).closest(".list-group-item");
    attachmentFileElem.find("input[name^='ClaimFile_']").val("");
    attachmentFileElem.find("input[name^='ClaimFileRemove']").val(true);
    let attachmentFileBtns = attachmentFileElem.find(".rr-claim-file-buttons");
    attachmentFileElem.find(".rr-claim-file-remove").hide();
    attachmentFileBtns.append(attachmentFileElem.find(".rr-claim-file-attach").show());
    e.preventDefault();
}

$(function () {
    $('.yes-no-panel').hide();
    $('#claimFileAdd').click(addClaimFile);
    $('#ClaimFiles').on('click', '.claim-file-edit-btn', editClaimFile);
    $('#ClaimFiles').on('click', '.claim-file-cancel-btn', cancelEditClaimFile);
    $('#ClaimFiles').on('click', '.claim-file-save-btn', saveClaimFile);
    $('#ClaimFiles').on('click', '.claim-file-delete-btn', deleteClaimFile);
    $('#ClaimFiles').on('click', '.rr-claim-file-attach', attachClaimFile);
    $('#ClaimFiles').on('click', '.rr-claim-file-remove', removeClaimFile);
    $('#ClaimFiles').on('change', "input[name^='ClaimFile_']", changeClaimFileAttachment);
});