function ownerFileRemove(e) {
    var ownerFileElem = $(this).closest('.list-group-item');
    ownerFileElem.find('input[name="attachmentFiles"]').val('');
    ownerFileElem.find('input[name$="FileDisplayName"]').val('');
    ownerFileElem.find('input[name="removeFiles"]').val(true);
    ownerFileElem.find('.rr-owner-file-remove').hide();
    ownerFileElem.find('.rr-owner-file-download').hide();
    ownerFileElem.find('.rr-owner-file-attach').show();
    e.preventDefault();
}
function ownerFileAttach(e) {
    var ownerFileElem = $(this).closest('.list-group-item');
    ownerFileElem.find('input[name="attachmentFiles"]').click();
    e.preventDefault();
}
function ownerFileDocumentDelete(e) {
    var ownerFileElem = $(this).closest('.list-group-item');
    var idFile = ownerFileElem.find('input')[0].value;
    $('#owners option[value="' + idFile + '"]').remove();
    $('#owners select').selectpicker("refresh");
    $('#owners input[name$="IdFile"]').each(function () {
        if (this.value == idFile) {
            this.value = '';
        }
    });
    ownerFileElem.remove();
    $('#ownerFiles li').each(function (i) {
        var inputs = $(this).find('input');
        inputs[0].name = 'OwnerFiles[' + i + '].Id';
        inputs[1].name = 'OwnerFiles['  + i + '].IdProcess';
        $(this).find('select')[0].name = 'OwnerFiles[' + i + '].IdReasonType';
        inputs[2].name = 'OwnerFiles[' + i + '].ReasonNumber';
        inputs[3].name = 'OwnerFiles[' + i + '].ReasonDate';
        inputs[4].name = 'OwnerFiles[' + i + '].FileDisplayName';
    });
    e.preventDefault();
}
var idNewOwnerFile = -1;
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
            newOwnerFileElem.find('input[name$="Id"]').val(idNewOwnerFile);
            var doc = getDocumentString(newOwnerFileElem);
            $('#owners select').append('<option value="' + idNewOwnerFile + '">' + doc + '</option>')
                .selectpicker("refresh");
            idNewOwnerFile--;
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
function getDocumentString(ownerFileElem) {
    var date = new Date(ownerFileElem.find('input[name$="ReasonDate"]').val());
    return '№' + ownerFileElem.find('input[name$="ReasonNumber"]').val() +
           ' от ' + date.toLocaleDateString('ru') +
           ' (' + ownerFileElem.find('select option:selected').text() + ')';
}
function ownerReasonAddOption(ownerFileElem) {
    var idFile = ownerFileElem.find('input[name$="Id"]').val();
    var doc = getDocumentString(ownerFileElem);
    $('#owners select').each(function () {
        var ownerFileAssocIdFile = $(this).closest('li').find('input[name$="IdFile"]').val();
        var selected = idFile == ownerFileAssocIdFile ? ' selected' : '';
        $(this).append('<option value="' + idFile + '"' + selected + '>' + doc + '</option>')
            .selectpicker("refresh");
    });
}
function documentChange(e) {
    var ownerFileElem = $(this).closest('li');
    var idFile = ownerFileElem.find('input[name$="Id"]').val();
    var doc = getDocumentString(ownerFileElem);
    $('#owners option[value="' + idFile + '"]').text(doc);
    $('#owners select').selectpicker("refresh");
    e.preventDefault();
}
$(function () {
    $('#ownerFiles .list-group-item').each(function () {
        ownerFilePanelInit($(this));
        ownerReasonAddOption($(this));
    });
    $('#ownerFiles').on('click', '.rr-owner-file-remove', ownerFileRemove);
    $('#ownerFiles').on('click', '.rr-owner-file-attach', ownerFileAttach);
    $('#ownerFiles').on('change', 'input[name="attachmentFiles"]', ownerFileInputChange);
    $('#ownerFiles').on('click', '.rr-owner-file-document-delete', ownerFileDocumentDelete);
    $('#ownerFileAdd').on('click', ownerFileAdd);
    $('#ownerFiles').on('change', 'select[name$="IdReasonType"]', documentChange);
    $('#ownerFiles').on('input', 'input[name$="ReasonNumber"]', documentChange);
    $('#ownerFiles').on('change', 'input[name$="ReasonDate"]', documentChange);
});