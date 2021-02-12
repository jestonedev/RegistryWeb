function ownerFileRemove(e) {
    var ownerFileElem = $(this).closest('.list-group-item');
    ownerFileElem.find('input[name="attachmentFiles"]').val('');
    ownerFileElem.find('input[name$="FileDisplayName"]').val('');
    ownerFileElem.find('input[name="removeFiles"]').val(true);
    ownerFileElem.find('.rr-owner-file-remove').hide();
    ownerFileElem.find('.rr-owner-file-download').hide();
    ownerFileElem.find('.rr-owner-file-attach').show();
    ownerFileElem.find('input[name$="DateDownload"]')
        .attr("readonly", false)
        .val('')
        .attr("readonly", true);
    e.preventDefault();
}
function ownerFileAttach(e) {
    var ownerFileElem = $(this).closest('.list-group-item');
    ownerFileElem.find('input[name="attachmentFiles"]').click();
    e.preventDefault();
}
function ownerFileDocumentDelete(e) {
    var ownerFileElem = $(this).closest('.list-group-item');
    ownerFileElem.remove();
    $('#ownerFiles li').each(function (i) {
        var inputs = $(this).find('input');
        inputs[0].name = 'OwnerFiles[' + i + '].Id';
        inputs[1].name = 'OwnerFiles[' + i + '].IdProcess';
        $(this).find('select')[0].name = 'OwnerFiles[' + i + '].IdReasonType';
        inputs[2].name = 'OwnerFiles[' + i + '].DateDocument';
        inputs[3].name = 'OwnerFiles[' + i + '].DateDownload';
        inputs[4].name = 'OwnerFiles[' + i + '].FileDisplayName';
    });
    e.preventDefault();
}
function ownerFileAdd(e) {
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/OwnerProcesses/OwnerFileAdd',
        data:
        {
            i: $('#ownerFiles li').length,
            action: $('#ownerProcessForm').data('action')
        },
        success: function (data) {
            $('#ownerFiles').append(data);
            var newOwnerFileElem = $('#ownerFiles .list-group-item').last();
            ownerFilePanelInit(newOwnerFileElem);
            newOwnerFileElem.find('input[name$="IdProcess"]').val($('#ownerProcess input[name="IdProcess"]').val());
            newOwnerFileElem.find('select').selectpicker("render");
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(jqXHR);
            console.log(textStatus);
            console.log(errorThrown);
        }
    });
    e.preventDefault();
}
function ownerFileInputChange(e) {
    var ownerFileElem = $(this).closest('.list-group-item');
    ownerFileElem.find('.rr-owner-file-attach').hide();
    ownerFileElem.find('.rr-owner-file-remove').show();
    $(this).prev().val($(this)[0].files[0].name);
    var date = new Date().toISOString().slice(0, 10);
    ownerFileElem.find('input[name$="DateDownload"]').val(date);
    e.preventDefault();
}
function ownerFilePanelInit(ownerFileElem) {
    if (ownerFileElem.find('input[name$="FileDisplayName"]').val() == "") {
        ownerFileElem.find('.rr-owner-file-remove').hide();
        ownerFileElem.find('.rr-owner-file-download').hide();
    }
    else {
        ownerFileElem.find('.rr-owner-file-attach').hide();
    }
}
$(function () {
    $('#ownerFiles .list-group-item').each(function () {
        ownerFilePanelInit($(this));
    });
    $('#ownerFiles').on('click', '.rr-owner-file-remove', ownerFileRemove);
    $('#ownerFiles').on('click', '.rr-owner-file-attach', ownerFileAttach);
    $('#ownerFiles').on('change', 'input[name="attachmentFiles"]', ownerFileInputChange);
    $('#ownerFiles').on('click', '.rr-owner-file-document-delete', ownerFileDocumentDelete);
    $('#ownerFileAdd').on('click', ownerFileAdd);
});