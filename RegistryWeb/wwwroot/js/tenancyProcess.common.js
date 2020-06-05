
$(function () {
    $('.tenancy-process-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogle);
    });

    $('body').on('click', '.tenancy-agreement-open', function (e) {
        var wrapper = $(this).closest('.tenancy-agreement-item');
        var idExecutor = wrapper.find('[id$="IdExecutor"]').val();
        var agreementDate = wrapper.find('[id$="AgreementDate"]').val();
        var agreementContent = wrapper.find('[id$="AgreementContent"]').val();
        var modal = $("#agreementModal");
        modal.find("#Agreement_Date").prop("disabled", "").val(agreementDate).prop("disabled", "disabled");
        modal.find("select#Agreement_IdExecutor").val(idExecutor).selectpicker('render');
        modal.find("#Agreement_Content").val(agreementContent);
        modal.on('hide.bs.modal', function () {
            modal.find("#Agreement_Content").scrollTop(0);
        });
        modal.modal('show');
        e.preventDefault();
    });

    $('body').on('click', '.tenancy-person-open', function (e) {
        var wrapper = $(this).closest('.tenancy-person-item');

        var surname = wrapper.find('[id$="Surname"]').val();
        var name = wrapper.find('[id$="Name"]').val();
        var patronimyc = wrapper.find('[id$="Patronymic"]').val();
        var dateOfBirth = wrapper.find('[id$="DateOfBirth"]').val();
        var idKinship = wrapper.find('[id$="IdKinship"]').val();
        var personalAccount = wrapper.find('[id$="PersonalAccount"]').val();
        var includeDate = wrapper.find('[id$="IncludeDate"]').val();
        var excludeDate = wrapper.find('[id$="ExcludeDate"]').val();
        var idDocType = wrapper.find('[id$="IdDocumentType"]').val();
        var docSeria = wrapper.find('[id$="DocumentSeria"]').val();
        var docNum = wrapper.find('[id$="DocumentNum"]').val();
        var idDocIssuedBy = wrapper.find('[id$="IdDocumentIssuedBy"]').val();
        var dateOfDocIssue = wrapper.find('[id$="DateOfDocumentIssue"]').val();
        var snils = wrapper.find('[id$="Snils"]').val();

        var regIdStreet = wrapper.find('[id$="RegistrationIdStreet"]').val();
        var regHouse = wrapper.find('[id$="RegistrationHouse"]').val();
        var regFlat = wrapper.find('[id$="RegistrationFlat"]').val();
        var regRoom = wrapper.find('[id$="RegistrationRoom"]').val();

        var resIdStreet = wrapper.find('[id$="ResidenceIdStreet"]').val();
        var resHouse = wrapper.find('[id$="ResidenceHouse"]').val();
        var resFlat = wrapper.find('[id$="ResidenceFlat"]').val();
        var resRoom = wrapper.find('[id$="ResidenceRoom"]').val();

        var modal = $("#personModal");

        modal.find("#Person_Surname").val(surname);
        modal.find("#Person_Name").val(name);
        modal.find("#Person_Patronymic").val(patronimyc);
        modal.find("#Person_DateOfBirth").prop("disabled", "").val(dateOfBirth).prop("disabled", "disabled");
        modal.find("select#Person_IdKinship").val(idKinship).selectpicker('render');
        modal.find("#Person_Phone").val(personalAccount);
        modal.find("#Person_IncludeDate").prop("disabled", "").val(includeDate).prop("disabled", "disabled");
        modal.find("#Person_ExcludeDate").prop("disabled", "").val(excludeDate).prop("disabled", "disabled");

        modal.find("select#Person_IdDocType").val(idDocType).selectpicker('render');
        modal.find("#Person_DocSeria").val(docSeria);
        modal.find("#Person_DocNumber").val(docNum);
        modal.find("select#Person_IdDocIssuer").val(idDocIssuedBy).selectpicker('render');
        modal.find("#Person_IssueDate").prop("disabled", "").val(dateOfDocIssue).prop("disabled", "disabled");
        modal.find("#Person_Snils").val(snils);

        modal.find("select#Person_IdRegStreet").val(regIdStreet).selectpicker('render');
        modal.find("#Person_RegHouse").val(regHouse);
        modal.find("#Person_RegPremise").val(regFlat);
        modal.find("#Person_RegSubPremise").val(regRoom);

        modal.find("select#Person_IdLivigStreet").val(resIdStreet).selectpicker('render');
        modal.find("#Person_LivingHouse").val(resHouse);
        modal.find("#Person_LivingPremise").val(resFlat);
        modal.find("#Person_LivingSubPremise").val(resRoom);

        modal.modal('show');
        e.preventDefault();
    });
});
