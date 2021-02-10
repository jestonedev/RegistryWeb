function getTenancyFiles() {
    var items = $("#TenancyProcessFiles .list-group-item").filter(function (idx, elem) {
        return !$(elem).hasClass("rr-list-group-item-empty");
    });

    return items.map(function (idx, elem) {
        return getTenancyFile($(elem));
    });
}

function getTenancyFile(attachmentFileElem) {
    return {
        IdFile: attachmentFileElem.find("[name^='IdFile']").val(),
        IdProcess: $("#TenancyProcessFiles").data("id"),
        Description: attachmentFileElem.find("[name^='TenancyFileDescription']").val(),
        AttachmentFile: attachmentFileElem.find("[name^='TenancyFile_']")[0],
        AttachmentFileRemove: attachmentFileElem.find("[name^='TenancyFileRemove']").val()
    };
}

function tenancyFileToFormData(attachmentFile) {
    var formData = new FormData();
    formData.append("File.IdFile", attachmentFile.IdFile);
    formData.append("File.IdProcess", attachmentFile.IdProcess);
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

function deleteTenancyFile(e) {
    let isOk = confirm("Вы уверены что хотите удалить документ?");
    if (isOk) {
        let attachmentFileElem = $(this).closest(".list-group-item");
        let idFile = attachmentFileElem.find("input[name^='IdFile']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/TenancyFiles/DeleteTenancyFile',
            data: { idFile: idFile },
            success: function (ind) {
                if (ind === 1) {
                    attachmentFileElem.remove();
                    if ($("#TenancyProcessFiles .list-group-item").length === 1) {
                        $("#TenancyProcessFiles .rr-list-group-item-empty").show();
                    }

                    if ($("#TenancyProcessFiles").find('.list-group-item').length - 1 > 0)
                        $(".TenancyProcessFilesbadge").text($("#TenancyProcessFiles").find('.list-group-item').length - 1);
                    else
                        $(".TenancyProcessFilesbadge").css("display", "none");
                }
                else {
                    alert("Ошибка удаления!");
                }
            }
        });
    }
    e.preventDefault();
}

function addTenancyFile(e) {
    let action = $('#TenancyProcessFiles').data('action');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/TenancyProcesses/AddTenancyFile',
        data: { action },
        success: function (elem) {
            let list = $('#TenancyProcessFiles');
            list.find(".rr-list-group-item-empty").hide();
            let attachmentFilesToggle = $('#TenancyFilesForm .tenancy-process-toggler');
            if (!isExpandElemntArrow(attachmentFilesToggle)) // развернуть при добавлении, если было свернуто 
                attachmentFilesToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find(".tenancy-file-edit-btn").click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
        }
    });
    e.preventDefault();
}

function editTenancyFile(e) {
    let attachmentFile = $(this).closest(".list-group-item");
    let fields = attachmentFile.find('input, select, textarea');
    let yesNoPanel = attachmentFile.find('.yes-no-panel');
    let editDelPanel = attachmentFile.find('.edit-del-panel');
    fields.prop('disabled', false);
    editDelPanel.hide();
    yesNoPanel.show();
    showTenancyFileEditFileBtns(attachmentFile,
        attachmentFile.find(".rr-tenancy-file-download").length > 0 &&
        !attachmentFile.find(".rr-tenancy-file-download").hasClass("disabled"));
    e.preventDefault();
}

function cancelEditTenancyFile(e) {
    let attachmentFileElem = $(this).closest(".list-group-item");
    let idFile = attachmentFileElem.find("input[name^='IdFile']").val();
    //Отменить изменения внесенные в документ
    if (idFile !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyFiles/GetTenancyFile',
            data: { idFile: idFile },
            success: function (attachmentFile) {
                refreshTenancyFile(attachmentFileElem, attachmentFile);
                showEditDelPanelTenancyFile(attachmentFileElem);
                showTenancyFileDownloadFileBtn(attachmentFileElem, attachmentFile.fileOriginName !== "" && attachmentFile.fileOriginName !== null);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        attachmentFileElem.remove();
        if ($("#TenancyProcessFiles .list-group-item").length === 1) {
            $("#TenancyProcessFiles .rr-list-group-item-empty").show();
        }
    }
    e.preventDefault();
}

function showTenancyFileEditFileBtns(attachmentFileElem, fileExists) {
    let attachmentFileBtns = attachmentFileElem.find(".rr-tenancy-file-buttons");
    attachmentFileElem.find(".rr-tenancy-file-download").hide();
    if (fileExists) {
        attachmentFileBtns.append(attachmentFileElem.find(".rr-tenancy-file-remove").show());
        attachmentFileElem.find(".rr-tenancy-file-attach").hide();
    } else {
        attachmentFileElem.find(".rr-tenancy-file-remove").hide();
        attachmentFileBtns.append(attachmentFileElem.find(".rr-tenancy-file-attach").show());
    }
}

function showTenancyFileDownloadFileBtn(attachmentFileElem, fileExists) {
    let attachmentFileBtns = attachmentFileElem.find(".rr-tenancy-file-buttons");
    attachmentFileBtns.append(attachmentFileElem.find(".rr-tenancy-file-download").show());
    if (fileExists) {
        attachmentFileElem.find(".rr-tenancy-file-download").removeClass("disabled");
    } else {
        attachmentFileElem.find(".rr-tenancy-file-download").addClass("disabled");
    }
    attachmentFileElem.find(".rr-tenancy-file-remove").hide();
    attachmentFileElem.find(".rr-tenancy-file-attach").hide();
}

function showEditDelPanelTenancyFile(attachmentFileElem) {
    let fields = attachmentFileElem.find('input, select, textarea');
    fields.prop('disabled', true);
    let editDelPanel = attachmentFileElem.find('.edit-del-panel');
    let yesNoPanel = attachmentFileElem.find('.yes-no-panel');
    yesNoPanel.hide();
    attachmentFileElem.removeClass("list-group-item-warning");
    editDelPanel.show();
}

function refreshTenancyFile(attachmentFileElem, attachmentFile) {
    attachmentFileElem.find("[name^='TenancyFileDescription']").val(attachmentFile.description);
    attachmentFileElem.find("[name^='TenancyFile_']").val("");
    attachmentFileElem.find("[name^='TenancyFileRemove']").val(false);
}

function saveTenancyFile(e) {
    let attachmentFileElem = $(this).closest(".list-group-item");
    let attachmentFile = tenancyFileToFormData(getTenancyFile(attachmentFileElem));
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/TenancyFiles/SaveTenancyFile',
        data: attachmentFile,
        processData: false,
        contentType: false,
        success: function (attachmentFile) {
            if (attachmentFile.idFile > 0) {
                attachmentFileElem.find("input[name^='IdFile']").val(attachmentFile.idFile);
                attachmentFileElem.find(".rr-tenancy-file-download")
                    .prop("href", "/TenancyFiles/DownloadFile/?idFile=" + attachmentFile.idFile);
                showTenancyFileDownloadFileBtn(attachmentFileElem, attachmentFile.fileName !== "" && attachmentFile.fileName !== null);
            }
            showEditDelPanelTenancyFile(attachmentFileElem);

            if ($("#TenancyProcessFiles").find('.list-group-item').length - 1 > 0)
            {
                $(".TenancyProcessFilesbadge").text($("#TenancyProcessFiles").find('.list-group-item').length + 1);
                $(".TenancyProcessFilesbadge").css("display", "inline-block");
            }
        }
    });
    e.preventDefault();
}

function attachTenancyFile(e) {
    var attachmentFileElem = $(this).closest(".list-group-item");
    attachmentFileElem.find("input[name^='TenancyFile_']").click();
    attachmentFileElem.find("input[name^='TenancyFileRemove']").val(false);
    e.preventDefault();
}

function changeTenancyFileAttachment() {
    var attachmentFileElem = $(this).closest(".list-group-item");
    if ($(this).val() !== "") {
        let attachmentFileBtns = attachmentFileElem.find(".rr-tenancy-file-buttons");
        attachmentFileElem.find(".rr-tenancy-file-attach").hide();
        attachmentFileBtns.append(attachmentFileElem.find(".rr-tenancy-file-remove").show());
        var descriptionElem = attachmentFileElem.find("input[name^='TenancyFileDescription']");
        if (descriptionElem.val() === "") {
            descriptionElem.val($(this)[0].files[0].name);
        }
    }
}

function removeTenancyFile(e) {
    var attachmentFileElem = $(this).closest(".list-group-item");
    attachmentFileElem.find("input[name^='TenancyFile_']").val("");
    attachmentFileElem.find("input[name^='TenancyFileRemove']").val(true);
    let attachmentFileBtns = attachmentFileElem.find(".rr-tenancy-file-buttons");
    attachmentFileElem.find(".rr-tenancy-file-remove").hide();
    attachmentFileBtns.append(attachmentFileElem.find(".rr-tenancy-file-attach").show());
    e.preventDefault();
}

$(function () {
    $('#TenancyProcessFiles .yes-no-panel').hide();
    $('#tenancyFileAdd').click(addTenancyFile);
    $('#TenancyProcessFiles').on('click', '.tenancy-file-edit-btn', editTenancyFile);
    $('#TenancyProcessFiles').on('click', '.tenancy-file-cancel-btn', cancelEditTenancyFile);
    $('#TenancyProcessFiles').on('click', '.tenancy-file-save-btn', saveTenancyFile);
    $('#TenancyProcessFiles').on('click', '.tenancy-file-delete-btn', deleteTenancyFile);
    $('#TenancyProcessFiles').on('click', '.rr-tenancy-file-attach', attachTenancyFile);
    $('#TenancyProcessFiles').on('click', '.rr-tenancy-file-remove', removeTenancyFile);
    $('#TenancyProcessFiles').on('change', "input[name^='TenancyFile_']", changeTenancyFileAttachment);

    if ($("#TenancyProcessFiles").find('.list-group-item').length-1 == 0)
        $(".TenancyProcessFilesbadge").css("display", "none");

});