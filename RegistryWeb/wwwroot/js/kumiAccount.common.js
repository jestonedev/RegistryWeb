$(function () {
    var form = $('#accountForm');

    var action = form.attr('data-action');

    if (action === 'Details' || action === 'Delete') {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
        $('input[type="hidden"]').prop('disabled', false);
        $('input[type="submit"]').prop('disabled', false);
    }

    $('.account-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogleHide);
    });

    function accountCustomValidations(validator) {
        var isValid = true;

        var account = $("#Account");
        if (account.valid()) {
            var accountExist = false;
            var idAccount = $('#IdAccount').val();
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/KumiAccounts/AccountExist',
                data: { account: account.val(), idAccount },
                async: false,
                success: function (data) {
                    accountExist = data;
                }
            });
            if (accountExist) {
                let error = {};
                error[account.attr("name")] = "Лицевой счет уже существует";
                validator.showErrors(error);
                isValid = false;
            } else {
                clearValidationError(account);
                removeErrorFromValidator(validator, account);
            }
        }

        return isValid;
    }

    $('#accountCreate, #accountEdit').click(function (e) {
        e.preventDefault();

        var tenancyInfo = $('.rr-tenancy-info');
        tenancyInfo.each(function (ind, elem) {
            var idProcessElem = $(elem).find('input[id^="IdProcess"]');
            idProcessElem.attr('name', "TenancyProcesses[" + ind + "].IdProcess");
            if (idProcessElem.val() === "") idProcessElem.remove();
        });

        var isValid = form.valid();
        var validator = form.validate();

        if (!accountCustomValidations(validator)) {
            isValid = false;
        }

        if (isValid) {
            form.find('input, select, textarea').attr('disabled', false);
            form.submit();
        } else {
            fixBootstrapSelectHighlight(form);
        }
    });
    $('#accountDelete').click(function (e) {
        e.preventDefault();
        form.submit();
    });

    var firstIdStateChange = true;

    $('#IdState').on('change', function (e) {
        var val = $(this).val();
        if (val === '2') {

            if (action === 'Create' || action === 'Edit') {
                $('#AnnualDate').removeAttr('disabled');
                if (!firstIdStateChange) {
                    var now = new Date();
                    var month = now.getMonth() + 1;
                    var day = now.getDate();
                    var nowStr = now.getFullYear() + '-' + (month < 10 ? '0' : '') + month + '-' + (day < 10 ? '0' : '') + day;
                    $('#AnnualDate').val(nowStr);
                }
            }
        }
        else {
            $('#AnnualDate').val('').attr('disabled', 'disabled');
        }
        e.preventDefault();
    });

    $('#IdState').change();
    firstIdStateChange = false;

    var tenancyModal = $('#TenancyModal');
    var tenancies = $("#TenancyInfo .rr-tenancy-info");
    var tenancyBlankItem = tenancies.last();
    var tenancyTemplate = tenancyBlankItem.clone(true);
    var tenancyAddBtn = $("[id^='tenancyAddBtn']").clone(true);
    if (tenancies.length > 1)
        tenancyBlankItem.remove();

    $("#TenancyInfo").on('click', '#tenancyAddBtn', tenancyAdd);

    function tenancyAdd(e) {
        var lastInserted = $(this).closest(".rr-tenancy-info");
        var idProcess = lastInserted.find("[id^='IdProcess_']").val();
        var lastInsertedIndex = 0;
        if (idProcess !== "") {
            $("#TenancyInfo").append(tenancyTemplate.clone(true));
            lastInserted = $(".rr-tenancy-info").last();
            lastInsertedIndex = lastInserted.index();
            lastInserted.removeClass("d-none");
            var addBtn = lastInserted.find("[id^='tenancyAddBtn']");
            addBtn.remove();
        }
        var changeBtn = lastInserted.find("[id^='tenancyChangeBtn_']");
        var deleteBtn = lastInserted.find("[id^='tenancyDeleteBtn_']");
        changeBtn.css("display", "block");
        deleteBtn.css("display", "block");

        tenancyModal.data('tenancy-index', lastInsertedIndex);
        tenancyModal.modal('show');
        e.preventDefault();
    }

    $("#Tenancy_IdRegion").on('change', idRegionTenancyModalChange);

    function idRegionTenancyModalChange(e) {
        var idRegion = $('#Tenancy_IdRegion').selectpicker('val');
        $.ajax({
            type: 'POST',
            url: window.location.origin + '/Address/GetKladrStreets',
            dataType: 'json',
            data: { idRegion },
            success: function (data) {
                var select = $('#Tenancy_IdStreet');
                select.selectpicker('destroy');
                select.find('option[value]').remove();
                $.each(data, function (i, d) {
                    select.append('<option value="' + d.idStreet + '">' + d.streetName + '</option>');
                });
                select.selectpicker();
            }
        });
        e.preventDefault();
    }

    $("#TenancyInfo").on('click', "[id^='tenancyDeleteBtn_']", tenancyDelete);

    function tenancyDelete(e) {
        var item = $(this).closest(".rr-tenancy-info");
        tenancyDeleteItem(item, true);
        e.preventDefault();
    }

    function tenancyDeleteItem(item, forceClear) {
        if ($(".rr-tenancy-info").length === 1) {
            if (forceClear || item.find("input[id^='IdProcess_']").val() == "") {
                item.find(".input-group-prepend button.disabled").addClass("disabled");
                item.find("input").val("");
                var changeBtn = item.find("[id^='tenancyChangeBtn_']");
                var deleteBtn = item.find("[id^='tenancyDeleteBtn_']");
                changeBtn.css("display", "none");
                deleteBtn.css("display", "none");
                item.find(".rr-tenancy-address.offset-4").remove();
            }
        } else {
            var itemIndex = item.index();
            item.remove();
            if (itemIndex === 0) {
                $(".rr-tenancy-info").first().find(".rr-tenancy-address").first().find(".input-group-append").append(tenancyAddBtn.clone(true));
            }
        }
    }

    $("#TenancyInfo").on('click', "[id^='tenancyChangeBtn_']", tenancyChange);

    function tenancyChange(e) {
        var currentTenancy = $(this).closest(".rr-tenancy-info");
        var index = currentTenancy.index();
        tenancyModal.data('tenancy-index', index);
        tenancyModal.modal('show');
        e.preventDefault();
    }

    tenancyModal.on("hide.bs.modal", function () {
        var index = tenancyModal.data("tenancy-index");
        if (index !== undefined) {
            tenancyModal.removeData("tenancye-index");
            if (tenancyModal.data("tenancy-success") !== "true") {
                tenancyDeleteItem($($(".rr-tenancy-info")[index]), false);
            }
        }
        tenancyModalClear();
    });

    $("#clearTenancyModalBtn").on('click', tenancyModalClear);

    function tenancyModalClear(e) {
        $('#resultTenancyModal').html('');
        resetModalForm($("#TenancyModalForm"));
        $("#Tenancy_IdRegion").change();
        $('#setTenancyModalBtn').attr('disabled', true);
    }

    $("#searchTenancyModalBtn").on('click', tenancyModalSearch);

    function tenancyModalSearch(e) {
        var div = $('#resultTenancyModal');
        div.html("");
        $('#setTenancyModalBtn').attr('disabled', true);
        $('#searchTenancyModalBtn').text('Ищем...').attr('disabled', true);
        $.ajax({
            async: true,
            type: 'POST',
            url: window.location.origin + '/KumiAccounts/GetTenancyInfo',
            data: {
                "FilterOptions.IdRegion": $('#Tenancy_IdRegion').selectpicker('val'),
                "FilterOptions.IdStreet": $('#Tenancy_IdStreet').selectpicker('val'),
                "FilterOptions.House": $('#Tenancy_House').val(),
                "FilterOptions.PremisesNum": $('#Tenancy_PremisesNum').val(),
                "FilterOptions.SubPremisesNum": $('#Tenancy_SubPremisesNum').val(),
                "FilterOptions.IdProcess": $('#Tenancy_IdProcess').val(),
                "FilterOptions.RegistrationNum": $('#Tenancy_RegNumber').val(),
                "FilterOptions.RegistrationDate": $('#Tenancy_RegDate').val(),
                "FilterOptions.IssuedDate": $('#Tenancy_IssueDate').val()
            },
            success: function (result) {
                var table = "<table class='table table-bordered'>";
                for (var i = 0; i < result.tenancies.length; i++) {
                    var tenancy = result.tenancies[i];
                    var idProcess = tenancy.idProcess;
                    var regNum = tenancy.registrationNum;
                    var regDateStr = null;
                    if (tenancy.registrationDate !== null) {
                        var regDate = new Date(tenancy.registrationDate);
                        var year = regDate.getFullYear();
                        var month = regDate.getMonth() + 1;
                        var day = regDate.getDate();
                        regDateStr = (day < 10 ? "0" + day : day) + "." + (month < 10 ? "0" + month : month) + "." + year;
                    }
                    var tenancyRequisits = "Рег. №: " + idProcess;
                    if (regNum !== null && regNum !== "") {
                        tenancyRequisits = "№ " + regNum + " " +
                            (regDateStr !== null ? "от " + regDateStr : "");
                    }

                    var radioButton = "<div class='form-check'><input style='margin-top: -7px' name='tenancySelected' value='" + idProcess + "' type='radio' class='form-check-input'></div>";

                    var account = tenancy.account;
                    var accountInfo = "";
                    var tenancyLink = "<a class='btn oi oi-eye p-0 ml-1 text-primary rr-account-list-eye-btn' href='/KumiAccounts/Details?idAccount=" + tenancy.idAccount + "' target='_blank'></a>";
                    if (account !== null)
                        accountInfo = "<br/><span class='text-danger'><i>Привязан к ЛС № " + account + "</i></span>";

                    var rentObjects = result.rentObjects[tenancy.idProcess];
                    if (rentObjects === undefined || rentObjects.length === 0) {
                        rentObjects = [{ address: { text: '' } }];
                    }
                    for (var j = 0; j < rentObjects.length; j++) {
                        var idBuilding = null;
                        var idPremises = null;
                        switch (rentObjects[j].address.addressType) {
                            case 2:
                                idBuilding = rentObjects[j].address.id;
                                break;
                            case 3:
                                idBuilding = rentObjects[j].address.idParents.Building;
                                idPremises = rentObjects[j].address.id;
                                break;
                            case 4:
                                idBuilding = rentObjects[j].address.idParents.Building;
                                idPremises = rentObjects[j].address.idParents.Premise;
                                break;
                        }
                        table += "<tr data-id-process='" + idProcess + "' data-id-building='" + idBuilding + "' data-id-premise='" + idPremises + "'>";

                        table += "<td style='vertical-align: middle'>" + (j === 0 ? radioButton : "") + "</td>";
                        table += "<td>" + (j === 0 ? tenancyRequisits + tenancyLink : "") + (j === 0 ? accountInfo : "") + "</td><td>" + rentObjects[j].address.text + "</td>";
                        table += "</tr>";
                    }
                }
                if (result.tenancies.length < result.count) {
                    table += "<tr><td colspan='3' class='text-center'><i class='text-danger'>Всего найдено " + result.count + " совпадений. Уточните запрос</i></td></tr>";
                }
                if (result.count === 0) {
                    table += "<tr><td colspan='3' class='text-center'><i class='text-danger'>Записи не найдены</i></td></tr>";
                }
                table += "</table>";
                div.html(table);
                $('#searchTenancyModalBtn').text('Найти').attr('disabled', false);
            }
        });
        e.preventDefault();
    }

    $("#resultTenancyModal").on('click', "[name='tenancySelected'][type='radio']", function (e) {
        if ($(this).is(":checked")) {
            $("#setTenancyModalBtn").attr('disabled', false);
        }
    });

    $("#setTenancyModalBtn").on('click', tenancyModalSet);

    function tenancyModalSet(e) {
        var tenancyIndex = tenancyModal.data("tenancy-index");
        if (tenancyIndex === undefined) {
            alert('Ошибка привязки найма. Обратитесь к администратору');
            return;
        } else {
            tenancyModal.data("tenancy-success", "true");

            var tenancyInfo = getSelectedTenancyModal();
            console.log(tenancyInfo);

            var tenancyElem = $($(".rr-tenancy-info")[tenancyIndex]);
            tenancyElem.find("[name$='.IdProcess']").val(tenancyInfo.idProcess);
            tenancyElem.find("[id^='TenancyRequisits_']").val(tenancyInfo.tenancyRequisits);

            var tenancyDetails = tenancyElem.find('[href^="/TenancyProcesses/Details"]');
            tenancyDetails.attr('href', '/TenancyProcesses/Details?idProcess=' + tenancyInfo.idProcess);
            tenancyDetails.closest(".input-group-prepend").find("button.dropdown-toggle").removeClass("disabled");

            var firstRentAddressElem = tenancyElem.find(".rr-tenancy-address");
            for (var i = 0; i < tenancyInfo.rentObjects.length; i++) {
                var rentObject = tenancyInfo.rentObjects[i];
                var currentRentElem = firstRentAddressElem;
                if (i > 0) {
                    tenancyElem.append(firstRentAddressElem.clone(true));
                    currentRentElem = tenancyElem.find(".rr-tenancy-address").last();
                    currentRentElem.addClass("offset-4");
                    currentRentElem.find(".input-group-append").remove();
                }

                currentRentElem.find("[id^='TenancyAddress_']").val(rentObject.addressText);

                
                var addressDetails = currentRentElem.find('[href^="/Buildings/Details"]')
                    .attr('href', '/Buildings/Details?idBuilding=' + rentObject.idBuilding);
                var premiseHref = currentRentElem.find('[href^="/Premises/Details"]');
                if (rentObject.idPremises === null) {
                    premiseHref.addClass("d-none");
                } else {
                    premiseHref.attr('href', '/Premises/Details?idPremises=' + rentObject.idPremises);
                    premiseHref.removeClass("d-none");
                }
                var addresToggler = addressDetails.closest(".input-group-prepend").find("button.dropdown-toggle");
                if (rentObject.idBuilding === null)
                    addresToggler.addClass("disabled");
                else
                    addresToggler.removeClass("disabled");
            }
        }
        tenancyModal.modal('hide');
        e.preventDefault();
    }

    function getSelectedTenancyModal() {
        var idProcess = $("#resultTenancyModal input[name='tenancySelected']").filter(function (idx, elem) {
            return $(elem).is(':checked');
        }).val();
        var rows = $("#resultTenancyModal tr[data-id-process='" + idProcess + "']");
        var result = { idProcess: null, tenancyRequisits: null, rentObjects: [] };
        for (var i = 0; i < rows.length; i++) {
            var row = rows[i];
            if (i === 0) {
                result.tenancyRequisits = $($(row).find("td")[1]).text();
                result.idProcess = idProcess;
            }
            result.rentObjects.push({
                addressText: $($(row).find("td")[2]).text(),
                idBuilding: $(row).data("idBuilding"),
                idPremises: $(row).data("idPremise")
           });
        }
        return result;
    }

    var toggleChargeArchive = function (e) {
        var archive = $(this).closest("#Charges").find("tr.rr-charge-archive");
        var icon = $(this).find(".oi");
        if (icon.hasClass("oi-chevron-bottom")) {
            icon.removeClass("oi-chevron-bottom");
            icon.addClass("oi-chevron-top");
            archive.show();
        } else {
            icon.addClass("oi-chevron-bottom");
            icon.removeClass("oi-chevron-top");
            archive.hide();
        }
        e.preventDefault();
    };

    $(".rr-charge-archive-btn").on("click", toggleChargeArchive);

    $("#AccountRecalcBtn").on("click", function (e) {
        var idAccount = $("#accountForm #IdAccount").val();
        var modal = $("#accountRecalcModal");
        modal.find("input[name='AccountKumiRecalc.IdAccount']").val(idAccount);
        modal.find("select, input").prop('disabled', false);
        modal.modal('show');
        e.preventDefault();
    });
});