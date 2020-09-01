function addClaimPerson(e) {
    let action = $('#ClaimPersons').data('action');

    $.ajax({
        type: 'POST',
        url: window.location.origin + '/Claims/AddClaimPerson',
        data: { action },
        success: function (elem) {
            let list = $('#ClaimPersons');
            list.find(".rr-list-group-item-empty").hide();
            let claimToggle = $('#ClaimPersonsForm .claim-toggler');
            if (!isExpandElemntArrow(claimToggle)) // развернуть при добавлении, если было свернуто 
                claimToggle.click();
            list.append(elem);
            elem = list.find(".list-group-item").last();
            elem.find("select").selectpicker("refresh");
            elem.find(".claim-person-edit-btn").first().click();
            $([document.documentElement, document.body]).animate({
                scrollTop: $(elem).offset().top
            }, 1000);
            refreshValidationClaimPersonsForm();
        }
    });
    e.preventDefault();
}

function editClaimPerson(e) {
    let claimPerson = $(this).closest(".list-group-item");
    let fields = claimPerson.find('input, select, textarea');
    let yesNoPanel = claimPerson.find('.yes-no-panel');
    let editDelPanel = claimPerson.find('.edit-del-panel');
    fields.prop('disabled', false);
    editDelPanel.hide();
    yesNoPanel.show();
    e.preventDefault();
}

function cancelEditClaimPerson(e) {
    let claimPersonElem = $(this).closest(".list-group-item");
    let idPerson = claimPersonElem.find("input[name^='IdPerson']").val();
    //Отменить изменения внесенные в документ
    if (idPerson !== "0") {
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/ClaimPersons/GetPerson',
            data: { idPerson: idPerson },
            success: function (claimPerson) {
                refreshClaimPerson(claimPersonElem, claimPerson);
                showEditDelPanelClaimPerson(claimPersonElem);
                clearValidationError(claimPersonElem);
            }
        });
    }
    //Отменить вставку нового документа
    else {
        claimPersonElem.remove();
        if ($("#ClaimPersons .list-group-item").length === 1) {
            $("#ClaimPersons .rr-list-group-item-empty").show();
        }
    }
    e.preventDefault();
}

function refreshClaimPerson(claimPersonElem, claimPerson) {
    claimPersonElem.find("[name^='Surname']").val(claimPerson.surname);
    claimPersonElem.find("[name^='Name']").val(claimPerson.name);
    claimPersonElem.find("[name^='Patronymic']").val(claimPerson.patronymic);
    claimPersonElem.find("[name^='DateOfBirth']").val(claimPerson.dateOfBirth);
    claimPersonElem.find("[name^='PlaceOfBirth']").val(claimPerson.placeOfBirth);
    claimPersonElem.find("[name^='WorkPlace']").val(claimPerson.workPlace);
    claimPersonElem.find("[name^='IsClaimer']").prop("checked", claimPerson.isClaimer);
}

function showEditDelPanelClaimPerson(claimPersonElem) {
    let fields = claimPersonElem.find('input, select, textarea');
    fields.prop('disabled', true);
    let editDelPanel = claimPersonElem.find('.edit-del-panel');
    let yesNoPanel = claimPersonElem.find('.yes-no-panel');
    yesNoPanel.hide();
    claimPersonElem.removeClass("list-group-item-warning");
    editDelPanel.show();
}

function saveClaimPerson(e) {
    let claimPersonElem = $(this).closest(".list-group-item");
    if (claimPersonElem.find("input, textarea, select").valid()) {
        let claimPerson = claimPersonToFormData(getClaimPerson(claimPersonElem));
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/ClaimPersons/SavePerson',
            data: claimPerson,
            processData: false,
            contentType: false,
            success: function (claimPerson) {
                if (claimPerson.idPerson > 0) {
                    claimPersonElem.find("input[name^='IdPerson']").val(claimPerson.idPerson);
                }
                showEditDelPanelClaimPerson(claimPersonElem);
            }
        });
    } else {
        $([document.documentElement, document.body]).animate({
            scrollTop: claimPersonElem.find(".input-validation-error").first().offset().top - 35
        }, 1000);
    }
    e.preventDefault();
}

function getClaimPerson(claimPersonElem) {
    return {
        IdPerson: claimPersonElem.find("[name^='IdPerson']").val(),
        IdClaim: claimPersonElem.closest("#ClaimPersons").data("id"),
        Surname: claimPersonElem.find("[name^='Surname']").val(),
        Name: claimPersonElem.find("[name^='Name']").val(),
        Patronymic: claimPersonElem.find("[name^='Patronymic']").val(),
        DateOfBirth: claimPersonElem.find("[name^='DateOfBirth']").val(),
        PlaceOfBirth: claimPersonElem.find("[name^='PlaceOfBirth']").val(),
        WorkPlace: claimPersonElem.find("[name^='WorkPlace']").val(),
        IsClaimer: claimPersonElem.find("[name^='IsClaimer']").is(":checked")
    };
}

function getClaimPersons() {
    var items = $("#ClaimPersons .list-group-item").filter(function (idx, elem) {
        return !$(elem).hasClass("rr-list-group-item-empty");
    });
    return items.map(function (idx, elem) { return getClaimPerson($(elem)); });
}

function claimPersonToFormData(claimPerson) {
    var formData = new FormData();
    formData.append("ClaimPerson.IdPerson", claimPerson.IdPerson);
    formData.append("ClaimPerson.IdClaim", claimPerson.IdClaim);
    formData.append("ClaimPerson.Surname", claimPerson.Surname);
    formData.append("ClaimPerson.Name", claimPerson.Name);
    formData.append("ClaimPerson.Patronymic", claimPerson.Patronymic);
    formData.append("ClaimPerson.DateOfBirth", claimPerson.DateOfBirth);
    formData.append("ClaimPerson.PlaceOfBirth", claimPerson.PlaceOfBirth);
    formData.append("ClaimPerson.WorkPlace", claimPerson.WorkPlace);
    formData.append("ClaimPerson.IsClaimer", claimPerson.IsClaimer);
    return formData;
}

function deleteClaimPerson(e) {
    let isOk = confirm("Вы уверены что хотите удалить члена семьи ответчика?");
    if (isOk) {
        let claimPersonElem = $(this).closest(".list-group-item");
        let idPerson = claimPersonElem.find("input[name^='IdPerson']").val();
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/ClaimPersons/DeletePerson',
            data: { idPerson: idPerson },
            success: function (ind) {
                if (ind === 1) {
                    claimPersonElem.remove();
                    if ($("#ClaimPersons .list-group-item").length === 1) {
                        $("#ClaimPersons .rr-list-group-item-empty").show();
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

let refreshValidationClaimPersonsForm = function () {
    var form = $("#ClaimPersonsForm")
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

$(function () {
    $("#ClaimPersonsForm").on("click", "#claimPersonAdd", addClaimPerson);
    $('#ClaimPersonsForm').on('click', '.claim-person-edit-btn', editClaimPerson);
    $('#ClaimPersonsForm').on('click', '.claim-person-cancel-btn', cancelEditClaimPerson);
    $('#ClaimPersonsForm').on('click', '.claim-person-save-btn', saveClaimPerson);
    $('#ClaimPersonsForm').on('click', '.claim-person-delete-btn', deleteClaimPerson);
});