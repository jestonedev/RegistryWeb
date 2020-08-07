function getTenancyAgreements() {
    var items = $("#TenancyProcessAgreements .list-group-item").filter(function (idx, elem) {
        return !$(elem).hasClass("rr-list-group-item-empty");
    });
    return items.map(function (idx, elem) {
        var data = {};
        var fields = $(elem).find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            data[name] = $(elem).val();
        });
        return data;
    });
}

$(function () {

    function tenancyAgreementFillModal(tenancyAgreementElem, action) {
        var modal = $("#agreementModal");
        var fields = tenancyAgreementElem.find("input, select, textarea");
        var modalFields = modal.find("input, select, textarea");
        modalFields.prop("disabled", "");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            modal.find("[name='Agreement." + name + "']").val($(elem).val());
        });
        if (action === "Details" || action === "Delete")
            modalFields.prop("disabled", "disabled");
        else {
            modalFields.prop("disabled", "");
            $("#Agreement_Type_TenancyPersonsWithoutTenant option[value]").remove();
            $("#Agreement_Type_TenancyPersons option[value]").remove();
            $("#Agreement_Type_Tenant").val("");
            $("#Agreement_Type_Tenant").prop("disabled", "disabled");
            var personsElems = $("#TenancyProcessPersons .list-group-item").filter(function (idx, elem) {
                return !$(elem).hasClass("rr-list-group-item-empty");
            });
            personsElems.each(function (idx, elem) {
                var excludeElem = $(elem).find("input[id^='ExcludeDate']");
                if (excludeElem.val() !== "" && excludeElem.val() !== null) return;
                $("#Agreement_Type_TenancyPersons").append(createPersonOptionByElem($(elem)));
                var kinshipElem = $(elem).find("select[id^='IdKinship']");
                if (kinshipElem.val() === "1") {
                    var surname = $(elem).find("input[id^='Surname']").val();
                    var name = $(elem).find("input[id^='Name']").val();
                    var patronymic = $(elem).find("input[id^='Patronymic']").val();
                    $("#Agreement_Type_Tenant").val(surname + " " + name + (patronymic !== "" ? " " + patronymic : ""));
                } else {
                    $("#Agreement_Type_TenancyPersonsWithoutTenant").append(createPersonOptionByElem($(elem)));
                }
            });
        }
    }

    function createPersonOptionByElem(personElem) {
        var idPerson = personElem.find("input[id^='IdPerson']").val();
        var guid = personElem.find("input[id^='IdPerson']").prop("id").split("_")[1];
        var surname = personElem.find("input[id^='Surname']").val();
        var name = personElem.find("input[id^='Name']").val();
        var patronymic = personElem.find("input[id^='Patronymic']").val();
        var birthDate = formatDate(personElem.find("input[id^='DateOfBirth']").val());
        var kinshipElem = personElem.find("select[id^='IdKinship']");
        var kinship = kinshipElem.find("option[value='" + kinshipElem.val() + "']").text();
        return "<option data-surname='" + surname + "' data-name='" + name + "'" +
            " data-patronymic='" + patronymic + "' data-birthdate='" + birthDate + "'" +
            " data-kinship='" + kinship + "' data-id='" + idPerson + "' value='" + guid + "'>" +
            surname + " " + name + (patronymic !== "" ? " " + patronymic : "") +
            ((birthDate !== "" && birthDate !== null) ? " (" + birthDate + " г.р.)" : "") +
            "</option>";
    }

    function tenancyAgreementFillElem(tenancyAgreementElem) {
        var modal = $("#agreementModal");
        var fields = tenancyAgreementElem.find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name").split("_")[0];
            var value = modal.find("[name='Agreement." + name + "']").val();
            $(elem).val(value);
        });
    }

    function getTenancyAgreement(form) {
        var data = {};
        var fields = form.find("input, select, textarea");
        fields.each(function (idx, elem) {
            var name = $(elem).attr("name");
            data[name] = $(elem).val();
        });
        data["Agreement.IdProcess"] = $("#TenancyProcessForm #TenancyProcess_IdProcess").val();
        return data;
    }

    function tenancyAgreementToFormData(agreement) {
        var formData = new FormData();
        for (var field in agreement) {
            formData.append(field, agreement[field]);
        }
        return formData;
    }

    $("#agreementModal").on("show.bs.modal", function () {
        $(this).find("select").selectpicker("refresh");
    });

    $("#agreementModal").on("hide.bs.modal", function () {

        $(this).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
        $(this).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");

        var tenancyAgreementElem = $('#TenancyProcessAgreements .list-group-item[data-processing]');
        tenancyAgreementElem.removeAttr("data-processing");
        addingTenancyAgreementElem = undefined;
    });

    function updateInsertTenancyAgreementElem() {
        var modal = $("#agreementModal");
        let tenancyAgreementElem = $('#TenancyProcessAgreements .list-group-item[data-processing]');
        if (tenancyAgreementElem.length > 0) {
            tenancyAgreementFillElem(tenancyAgreementElem);
            modal.modal('hide');
        } else {
            let list = $('#TenancyProcessAgreements');
            list.find(".rr-list-group-item-empty").hide();
            let tenancyAgreementToggle = $('#TenancyProcessAgreementsForm .tenancy-process-toggler');
            if (!isExpandElemntArrow(tenancyAgreementToggle)) // развернуть при добавлении, если было свернуто 
                tenancyAgreementToggle.click();
            list.append(addingTenancyAgreementElem);
            let tenancyAgreementElem = $('#TenancyProcessAgreements .list-group-item').last();
            tenancyAgreementFillElem(tenancyAgreementElem);
            modal.modal('hide');
            $([document.documentElement, document.body]).animate({
                scrollTop: $(tenancyAgreementElem).offset().top
            }, 1000);
        }
    }

    $("#agreementModal").on("click", "#saveAgreementModalBtn", function (e) {
        let action = $('#TenancyProcessAgreements').data('action');
        var form = $("#TenancyProcessAgreementsModalForm");
        form.find("#Agreement_Type").val("").selectpicker("refresh").change();
        var isValid = form.valid();

        if (isValid) {
            if (action === "Create") {
                updateInsertTenancyAgreementElem();
                return;
            }
            let tenancyAgreement = tenancyAgreementToFormData(getTenancyAgreement(form));
            $.ajax({
                type: 'POST',
                url: window.location.origin + '/TenancyAgreements/SaveAgreement',
                data: tenancyAgreement,
                processData: false,
                contentType: false,
                success: function (tenancyAgreementReturn) {
                    if (tenancyAgreementReturn.idAgreement > 0) {
                        form.find("[name='Agreement.IdAgreement']").val(tenancyAgreementReturn.idAgreement);
                        updateInsertTenancyAgreementElem();
                    } else {
                        alert('Произошла ошибка при сохранении');
                    }
                }
            });

        } else {
            refreshSelectpickerValidationBorders(form);
            $([document.documentElement, document.body]).animate({
                scrollTop: form.find(".input-validation-error").first().offset().top - 35
            }, 1000);
        }
    });

    function refreshSelectpickerValidationBorders(form) {
        form.find("select").each(function (idx, elem) {
            var id = $(elem).prop("id");
            var name = $(elem).prop("name");
            var errorSpan = $("span[data-valmsg-for='" + name + "']");
            if (errorSpan.hasClass("field-validation-error")) {
                $("button[data-id='" + id + "']").addClass("input-validation-error");
            }
        });
    }

    function deleteTenancyAgreement(e) {
        let isOk = confirm("Вы уверены что хотите удалить дополнительное соглашение?");
        if (isOk) {
            let tenancyAgreementElem = $(this).closest(".list-group-item");
            let idAgreement = tenancyAgreementElem.find("input[name^='IdAgreement']").val();
            if (idAgreement === "0") {
                tenancyAgreementElem.remove();
                if ($("#TenancyProcessAgreements .list-group-item").length === 1) {
                    $("#TenancyProcessAgreements .rr-list-group-item-empty").show();
                }
            } else {
                $.ajax({
                    async: false,
                    type: 'POST',
                    url: window.location.origin + '/TenancyAgreements/DeleteAgreement',
                    data: { idAgreement: idAgreement },
                    success: function (ind) {
                        if (ind === 1) {
                            tenancyAgreementElem.remove();
                            if ($("#TenancyProcessAgreements .list-group-item").length === 1) {
                                $("#TenancyProcessAgreements .rr-list-group-item-empty").show();
                            }
                        }
                        else {
                            alert("Ошибка удаления!");
                        }
                    }
                });
            }
        }
        e.preventDefault();
    }

    var addingTenancyAgreementElem = undefined;

    function addTenancyAgreement(e) {
        let action = $('#TenancyProcessAgreements').data('action');

        $.ajax({
            type: 'POST',
            url: window.location.origin + '/TenancyProcesses/AddTenancyAgreement',
            data: { action },
            success: function (elem) {
                addingTenancyAgreementElem = elem;
                tenancyAgreementFillModal($(elem), action);
                var modal = $("#agreementModal");
                var agreementContent = modal.find("#Agreement_AgreementContent").val();
                modal.find("#Agreement_AgreementContent").val(reformAgreementContent(agreementContent));

                modal.modal('show');
            }
        });
        e.preventDefault();
    }

    function reformAgreementContent(agreementContent) {
        var tenancyBaseInfo = getTenancyBaseInfo();
        var rentObjects = getRentObjects();
        rentObjects = rentObjects.toArray().sort(sortRentObjects);
        var rentAddress = buildFullRentAddress(rentObjects);

        agreementContent = agreementContent.replace("{0}", tenancyBaseInfo.RegistrationNumber);
        agreementContent = agreementContent.replace("{1}", tenancyBaseInfo.RegistrationDate);
        agreementContent = agreementContent.replace("{2}", tenancyBaseInfo.RentTypeGenetive);
        agreementContent = agreementContent.replace("{3}", rentAddress);

        return agreementContent;
    }

    function getTenancyBaseInfo() {
        var idRentType = $('#TenancyProcessForm #TenancyProcess_IdRentType').val();
        var rentTypeGenetive = $('#TenancyProcessForm input[name="RentTypeGenetive"]').filter(function (idx, elem) { return $(elem).val() === idRentType; }).first().data("genetive");
        if (rentTypeGenetive === undefined) {
            rentTypeGenetive = "";
        }
        var regNumber = $("#TenancyProcessForm #TenancyProcess_RegistrationNum").val();
        var regDate = formatDate($("#TenancyProcessForm #TenancyProcess_RegistrationDate").val());
        return {
            IdRentType: idRentType,
            RentTypeGenetive: rentTypeGenetive,
            RegistrationNumber: regNumber,
            RegistrationDate: regDate
        };
    }

    function getRentObjects() {
        var rentObjects = $("#TenancyProcessRentObjects .list-group-item").filter(function (idx, elem) {
            return !$(elem).hasClass("rr-list-group-item-empty");
        }).map(function (idx, elem) {
            var streetElem = $(elem).find("select[name^='IdStreet']");
            var buildingElem = $(elem).find("select[name^='IdBuilding']");
            var premiseElem = $(elem).find("select[name^='IdPremises']");
            var subPremiseElem = $(elem).find("select[name^='IdSubPremises']");
            return {
                Street: streetElem.find("option[value='" + streetElem.val() + "']").text(),
                Building: buildingElem.find("option[value='" + buildingElem.val() + "']").text(),
                Premise: premiseElem.find("option[value='" + premiseElem.val() + "']").text(),
                SubPremise: subPremiseElem.find("option[value='" + subPremiseElem.val() + "']").text()
            };
            });
        return rentObjects;
    }

    function sortRentObjects(a, b) {
        if (a.Street < b.Street) return -1;
        if (a.Street > b.Street) return 1;
        if (a.Building < b.Building) return -1;
        if (a.Building > b.Building) return 1;
        if (a.Premise < b.Premise) return -1;
        if (a.Premise > b.Premise) return 1;
        if (a.SubPremise < b.SubPremise) return -1;
        if (a.SubPremise > b.SubPremise) return 1;
        return 0;
    }

    function groupRentObjects(rentObjects) {
        var currentStreet = undefined;
        var currentBuilding = undefined;
        var currentPremise = undefined;
        var currentSubPremise = undefined;
        var groupedRentObjects = [];
        for (var i = 0; i < rentObjects.length; i++) {
            var rentObject = rentObjects[i];
            var street = rentObject.Street;
            var building = rentObject.Building;
            var premise = rentObject.Premise;
            var subPremise = rentObject.SubPremise;
            if (street === "" || building === "") continue;
            if (street !== currentStreet) {
                currentStreet = street;
                currentBuilding = undefined;
                currentPremise = undefined;
                currentSubPremise = undefined;
                groupedRentObjects.push({ Street: currentStreet, Buildings: [] });
            }
            if (building !== currentBuilding) {
                currentBuilding = building;
                currentPremise = undefined;
                currentSubPremise = undefined;
                for (let sIndex in groupedRentObjects) {
                    if (groupedRentObjects[sIndex].Street === currentStreet) {
                        groupedRentObjects[sIndex].Buildings.push({ Building: currentBuilding, Premises: [] });
                    }
                }
            }
            if (premise !== currentPremise) {
                currentPremise = premise;
                currentSubPremise = undefined;
                if (currentPremise !== "") {
                    for (let sIndex in groupedRentObjects) {
                        if (groupedRentObjects[sIndex].Street === street) {
                            for (let bIndex in groupedRentObjects[sIndex].Buildings) {
                                if (groupedRentObjects[sIndex].Buildings[bIndex].Building === currentBuilding) {
                                    groupedRentObjects[sIndex].Buildings[bIndex].Premises.push({ Premise: currentPremise, SubPremises: [] });
                                }
                            }
                        }
                    }
                }
            }
            if (subPremise !== currentSubPremise) {
                currentSubPremise = subPremise;
                if (currentSubPremise !== "") {
                    for (let sIndex in groupedRentObjects) {
                        if (groupedRentObjects[sIndex].Street === street) {
                            for (let bIndex in groupedRentObjects[sIndex].Buildings) {
                                if (groupedRentObjects[sIndex].Buildings[bIndex].Building === currentBuilding) {
                                    for (let pIndex in groupedRentObjects[sIndex].Buildings[bIndex].Premises) {
                                        if (groupedRentObjects[sIndex].Buildings[bIndex].Premises[pIndex].Premise === currentPremise) {
                                            groupedRentObjects[sIndex].Buildings[bIndex]
                                                .Premises[pIndex].SubPremises.push({ SubPremise: currentSubPremise });
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }
        return groupedRentObjects;
    }

    function buildFullRentAddress(rentObjects) {
        var fullRentAddress = "Российская Федерация, Иркутская область, ";
        var groupedRentObjects = groupRentObjects(rentObjects);
        for (var i = 0; i < groupedRentObjects.length; i++) {
            var street = groupedRentObjects[i].Street;
            if (street.indexOf("Иркутская обл., ") !== -1) {
                street = street.replace("Иркутская обл., ", "");
            }
            street = street.replace("г.", "город").replace("ж.р.", "жилой район").replace("мкр.", "микрорайон").replace("ул.", "улица")
                .replace("пер.", "переулок").replace("б-р.", "бульвар").replace("пр-кт.", "проспект").replace("кв-л.", "квартал")
                .replace("проезд.", "проезд").replace("ст.", "станция").replace("тер.", "территория");
            fullRentAddress += street;
            if (groupedRentObjects[i].Buildings.length > 1) {
                fullRentAddress += ", дома ";
            } else if (groupedRentObjects[i].Buildings.length === 1) {
                fullRentAddress += ", дом ";
            }
            for (var j = 0; j < groupedRentObjects[i].Buildings.length; j++) {
                var building = groupedRentObjects[i].Buildings[j];
                fullRentAddress += building.Building;
                if (building.Premises.length > 1) {
                    fullRentAddress += ", квартиры ";
                } else if (building.Premises.length === 1) {
                    fullRentAddress += ", квартира ";
                }
                for (var k = 0; k < building.Premises.length; k++) {
                    var premise = building.Premises[k];
                    fullRentAddress += premise.Premise;

                    if (premise.SubPremises.length > 1) {
                        fullRentAddress += ", комнаты ";
                    } else if (premise.SubPremises.length === 1) {
                        fullRentAddress += ", комната ";
                    }
                    for (var l = 0; l < premise.SubPremises.length; l++) {
                        fullRentAddress += premise.SubPremises[l].SubPremise;
                        if (l < premise.SubPremises.length - 1) {
                            fullRentAddress += ", ";
                        }
                    }

                    if (k < building.Premises.length - 1) {
                        fullRentAddress += ", ";
                    }
                }

                if (j < groupedRentObjects[i].Buildings.length - 1) {
                    fullRentAddress += ", ";
                }
            }
            if (i < groupedRentObjects.length - 1) {
                fullRentAddress += ", ";
            }
        }
        return fullRentAddress;
    }

    $('body').on('click', '.tenancy-agreement-open-btn, .tenancy-agreement-edit-btn', function (e) {
        var tenancyAgreementElem = $(this).closest('.list-group-item');
        if (tenancyAgreementElem.find("input[name^='IdAgreement']").val() === "0") {
            tenancyAgreementElem.attr("data-processing", "create");
        } else {
            tenancyAgreementElem.attr("data-processing", "edit");
        }
        var action = $("#TenancyProcessAgreements").data("action");
        tenancyAgreementFillModal(tenancyAgreementElem, action);

        var modal = $("#agreementModal");
        modal.modal('show');

        e.preventDefault();
    });

    $("#agreementModal #Agreement_Type").on("change", function (e) {
        var id = $(this).val();
        if (id === "" || id === null)
            $("#Agreement_Type_Button").prop("disabled", "disabled");
        else
            $("#Agreement_Type_Button").prop("disabled", "");
        $("#agreementModal .rr-agreement-type-field").each(function (idx, elem) {
            $(elem).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
            $(elem).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
            var agreementTypeIds = $(elem).data("agreement-types").toString().split(",");
            if (agreementTypeIds.indexOf(id) !== -1) {
                $(elem).show();
            } else {
                $(elem).hide();
            }
        });
    });

    $("#agreementModal #Agreement_Type").change();

    $("#agreementModal #Agreement_Type_Button").on("click", function (e) {
        e.preventDefault();
        var isValid = $("#agreementModal .rr-agreement-type-field").find("input, select, textarea").valid();
        if (!isValid) {
            refreshSelectpickerValidationBorders($("#TenancyProcessAgreementsModalForm"));
            return;
        }
        var agreementType = $("#agreementModal #Agreement_Type").val();
        switch (agreementType) {
            case "0":
                var excludeTenantPersonInfo = getExcludeTenantPersonInfo();
                addExcludeTenantPersonInfoToContent(excludeTenantPersonInfo);
                addExcludeTenantPersonInfoToModifications(excludeTenantPersonInfo);
                break;
            case "1":
                var includeTenantPersonInfo = getIncludeTenantPersonInfo();
                addIncludeTenantPersonInfoToContent(includeTenantPersonInfo);
                addIncludeTenantPersonInfoToModifications(includeTenantPersonInfo);
                break;
            case "2":
                var explainPointInfo = getExplainPointInfo();
                addExplainPointInfoToContent(explainPointInfo);
                break;
            case "3":
                var terminateTenancyInfo = getTerminateTenancyInfo();
                addTerminateTenancyInfoToContent(terminateTenancyInfo);
                break;
            case "4":
                var prolongCommercialInfo = getProlongCommercialInfo();
                addProlongCommercialInfoToContent(prolongCommercialInfo);
                addProlongCommercialInfoToModifications(prolongCommercialInfo);
                break;
            case "5":
                var prolongSpecialInfo = getProlongSpecialInfo();
                addProlongSpecialInfoToContent(prolongSpecialInfo);
                addProlongSpecialInfoToModifications(prolongSpecialInfo);
                break;
            case "6":
                var changeTenantInfo = getChangeTenantInfo();
                addChangeTenantInfoToContent(changeTenantInfo);
                addChangeTenantInfoToModifications(changeTenantInfo);
                break;
        }
    });

    function getExcludeTenantPersonInfo() {
        var personElem = $("#agreementModal #Agreement_Type_TenancyPersons");
        var personGuid = personElem.val();
        var personOption = personElem.find("option[value='" + personGuid + "']");
        return {
            IdPerson: personOption.data("id"),
            Guid: personGuid,
            Surname: personOption.data("surname"),
            Name: personOption.data("name"),
            Patronymic: personOption.data("patronymic"),
            Kinship: personOption.data("kinship"),
            BirthDate: personOption.data("birthdate"),
            Point: $("#agreementModal #Agreement_Type_Point").val(),
            SubPoint: $("#agreementModal #Agreement_Type_SubPoint").val()
        };
    }

    function addExcludeTenantPersonInfoToContent(personInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var content = contentElem.val();
        var contentLines = content.split("\n");
        var headerWildcard = "^\u200B.*?из договора исключить";
        if (personInfo.Point !== "") {
            headerWildcard = "^\u200B.*?из пункта " + personInfo.Point + " договора исключить";
        }
        if (personInfo.SubPoint !== "") {
            headerWildcard += " подпункт " + personInfo.SubPoint + " следующего содержания";
        }
        if (!isHeaderInserted(contentLines, headerWildcard)) {
            var nextPoint = getNextHeaderPoint(contentLines);
            var header = "\u200B" + nextPoint + ") из договора исключить";
            if (personInfo.Point !== "") {
                header = "\u200B" + nextPoint + ") из пункта " + personInfo.Point + " договора исключить";
            }
            if (personInfo.SubPoint !== "") {
                header += " подпункт " + personInfo.SubPoint + " следующего содержания";
            }
            header += ":";
            contentLines.push(header);
        }

        var tenant = "";
        if (personInfo.SubPoint !== "")
            tenant += personInfo.SubPoint + ". ";
        tenant += personInfo.Surname + " " + personInfo.Name + (personInfo.Patronymic !== "" ? " " + personInfo.Patronymic : "");
        tenant += " - " + personInfo.Kinship;
        if (personInfo.BirthDate !== "") {
            tenant += ", " + personInfo.BirthDate + " г.р.";
        }
        tenant = "«" + tenant + "»";
        contentLines = insertPoint(contentLines, tenant, headerWildcard);
        contentElem.val(contentLines.join("\n"));
    }

    function addExcludeTenantPersonInfoToModifications(personInfo) {
        // TODO:
    }

    function getIncludeTenantPersonInfo() {
        var tenancyBaseInfo = getTenancyBaseInfo();
        var kinshipElem = $("#agreementModal #Agreement_Type_TenancyPersonIdKinship");
        var idKinship = kinshipElem.val();
        var kinshipOption = kinshipElem.find("option[value='" + idKinship + "']");
        return {
            Surname: $("#agreementModal #Agreement_Type_TenancyPersonSurname").val(),
            Name: $("#agreementModal #Agreement_Type_TenancyPersonName").val(),
            Patronymic: $("#agreementModal #Agreement_Type_TenancyPersonPatronymic").val(),
            Kinship: kinshipOption.text(),
            IdKinship: idKinship,
            BirthDate: formatDate($("#agreementModal #Agreement_Type_TenancyPersonBirthDate").val()),
            Point: $("#agreementModal #Agreement_Type_Point").val(),
            SubPoint: $("#agreementModal #Agreement_Type_SubPoint").val(),
            RegistrationNum: tenancyBaseInfo.RegistrationNumber,
            RegistrationDate: tenancyBaseInfo.RegistrationDate
        };
    }

    function addIncludeTenantPersonInfoToContent(personInfo) {
        let contentElem = $("#agreementModal #Agreement_AgreementContent");
        let content = contentElem.val();
        let contentLines = content.split("\n");
        if (personInfo.IdKinship === "1") {
            let nextPoint = getNextHeaderPoint(contentLines);
            let header = "\u200B" + nextPoint + ") считать по договору";
            if (personInfo.RegistrationNum !== "") {
                header += " № " + personInfo.RegistrationNum;
            }
            if (personInfo.RegistrationDate !== "") {
                header += " от " + personInfo.RegistrationDate;
            }
            let tenant = personInfo.Surname + " " + personInfo.Name + (personInfo.Patronymic !== "" ? " " + personInfo.Patronymic : "");
            if (personInfo.BirthDate !== "") {
                tenant += " - " + personInfo.BirthDate + " г.р.";
            }
            header += " нанимателем - «" + tenant + "»";
            contentLines.push(header);
        } else {
            var headerWildcard = "^\u200B.*?договор дополнить";
            if (personInfo.Point !== "") {
                headerWildcard = "^\u200B.*?пункт " + personInfo.Point + " договора дополнить";
            }
            if (personInfo.SubPoint !== "") {
                headerWildcard += " подпунктом " + personInfo.SubPoint + " следующего содержания";
            }
            if (!isHeaderInserted(contentLines, headerWildcard)) {
                let nextPoint = getNextHeaderPoint(contentLines);
                let header = "\u200B" + nextPoint + ") договор дополнить";
                if (personInfo.Point !== "") {
                    header = "\u200B" + nextPoint + ") пункт " + personInfo.Point + " договора дополнить";
                }
                if (personInfo.SubPoint !== "") {
                    header += " подпунктом " + personInfo.SubPoint + " следующего содержания";
                }
                header += ":";
                contentLines.push(header);
            }

            var tenant = "";
            if (personInfo.SubPoint !== "")
                tenant += personInfo.SubPoint + ". ";
            tenant += personInfo.Surname + " " + personInfo.Name + (personInfo.Patronymic !== "" ? " " + personInfo.Patronymic : "");
            tenant += " - " + personInfo.Kinship;
            if (personInfo.BirthDate !== "") {
                tenant += ", " + personInfo.BirthDate + " г.р.";
            }
            tenant = "«" + tenant + "»";
            contentLines = insertPoint(contentLines, tenant, headerWildcard);
        }
        contentElem.val(contentLines.join("\n"));
    }

    function addIncludeTenantPersonInfoToModifications(personInfo) {
        // TODO:
    }

    function getExplainPointInfo() {
        return {
            Content: $("#agreementModal #Agreement_Type_PointContent").val(),
            Point: $("#agreementModal #Agreement_Type_Point").val(),
            SubPoint: $("#agreementModal #Agreement_Type_SubPoint").val()
        };
    }

    function addExplainPointInfoToContent(explainPointInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var content = contentElem.val();
        var contentLines = content.split("\n");
        var nextPoint = getNextHeaderPoint(contentLines);
        var header = "\u200B" + nextPoint + ") изложить";
        var pointHeader = "";
        if (explainPointInfo.Point !== "" && explainPointInfo.SubPoint === "") {
            pointHeader = " пункт " + explainPointInfo.Point;
        }
        if (explainPointInfo.SubPoint !== "" && explainPointInfo.Point === "") {
            pointHeader = " подпункт " + explainPointInfo.SubPoint;
        }
        if (explainPointInfo.SubPoint !== "" && explainPointInfo.Point !== "") {
            pointHeader = " подпункт " + explainPointInfo.SubPoint + " пункта " + explainPointInfo.Point;
        }
        header += pointHeader+" в новой редакции:";
        contentLines.push(header);

        pointContent = "«" + explainPointInfo.Content + "»";
        contentLines.push(pointContent);
        contentElem.val(contentLines.join("\n"));
    }

    function getTerminateTenancyInfo() {
        var tenancyBaseInfo = getTenancyBaseInfo();
        var rentObjects = getRentObjects();
        rentObjects = rentObjects.toArray().sort(sortRentObjects);
        var rentAddress = buildFullRentAddress(rentObjects);
        return {
            RegistrationNum: tenancyBaseInfo.RegistrationNumber,
            RegistrationDate: tenancyBaseInfo.RegistrationDate,
            RentTypeGenetive: tenancyBaseInfo.RentTypeGenetive,
            RentAddress: rentAddress,
            TerminateDate: formatDate($("#agreementModal #Agreement_Type_TenancyEndDate").val()),
            TerminateReason: $("#agreementModal #Agreement_Type_TenancyEndReason").val()
        };
    }

    function addTerminateTenancyInfoToContent(terminateTenancyInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var contentLines = [];

        var text = "1.1. По настоящему Соглашению Стороны договорились расторгнуть с " + terminateTenancyInfo.TerminateDate + " договор";
        contract = "";
        if (terminateTenancyInfo.RegistrationNum !== "") {
            contract += " № " + terminateTenancyInfo.RegistrationNum;
        }
        if (terminateTenancyInfo.RegistrationDate !== "") {
            contract += " от " + terminateTenancyInfo.RegistrationDate;
        }
        text += contract;
        if (terminateTenancyInfo.RentTypeGenetive !== "") {
            text += " "+terminateTenancyInfo.RentTypeGenetive;
        }
        text += " найма жилого помещения";
        if (terminateTenancyInfo.RentAddress !== "") {
            text += ", расположенного по адресу: " + terminateTenancyInfo.RentAddress;
        }
        text += ", (далее - договор) по " + terminateTenancyInfo.TerminateReason + ".";
        contentLines.push(text);
        text = "1.2. Обязательства, возникшие из указанного договора до момента расторжения, подлежат исполнению в соответствии с указанным договором. Стороны не имеют взаимных претензий по исполнению условий договора";
        text += contract;
        text += ".";
        contentLines.push(text);
        contentElem.val(contentLines.join("\n"));
    }

    function getProlongCommercialInfo() {
        var tenancyBaseInfo = getTenancyBaseInfo();
        var reasonDocElem = $("#agreementModal #Agreement_Type_TenancyProlongRentReason");
        var reasonDocId = reasonDocElem.val();
        var reasonDocOption = reasonDocElem.find("option[value='" + reasonDocId + "']");
        return {
            RegistrationNum: tenancyBaseInfo.RegistrationNumber,
            RegistrationDate: tenancyBaseInfo.RegistrationDate,
            RentTypeGenetive: tenancyBaseInfo.RentTypeGenetive,
            ReasonDoc: reasonDocOption.text(),
            ReasonDocGenetive: reasonDocOption.data("genetive"),
            ReasonDocDate: formatDate($("#agreementModal #Agreement_Type_TenancyProlongRentReasonDate").val()),
            StartPeriod: formatDate($("#agreementModal #Agreement_Type_ProlongBeginDate").val()),
            EndPeriod: formatDate($("#agreementModal #Agreement_Type_ProlongEndDate").val()),
            UntilDismissal: $("#agreementModal #Agreement_Type_ProlongUntilDismissal").is(":checked"),
            PointExclude: $("#agreementModal #Agreement_Type_PointExclude").val()
        };
    }

    function addProlongCommercialInfoToContent(prolongInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var contentLines = [];

        contentLines.push(getDefaultAgreementText());

        var text = "1) на основании " + prolongInfo.ReasonDocGenetive + " от " + prolongInfo.ReasonDocDate + " продлить срок действия договора";
        contract = "";
        if (prolongInfo.RegistrationNum !== "") {
            contract += " № " + prolongInfo.RegistrationNum;
        }
        if (prolongInfo.RegistrationDate !== "") {
            contract += " от " + prolongInfo.RegistrationDate;
        }
        text += contract;
        if (prolongInfo.RentTypeGenetive !== "") {
            text += " " + prolongInfo.RentTypeGenetive;
        }
        text += " найма жилого помещения";

        var period = "";
        if (prolongInfo.StartPeriod !== "") {
            period += " с " + prolongInfo.StartPeriod;
        }
        if (prolongInfo.EndPeriod !== "" && !prolongInfo.UntilDismissal) {
            period += " по " + prolongInfo.EndPeriod;
        }
        if (prolongInfo.UntilDismissal) {
            if (prolongInfo.StartPeriod === "")
                period = "";
            period += " на период трудовых отношений";
        }

        text += period + ".";
        contentLines.push(text);
        if (prolongInfo.PointExclude !== "") {
            text = "2) пункт " + prolongInfo.PointExclude + " исключить.";
            contentLines.push(text);
        }
        contentElem.val(contentLines.join("\n"));
    }

    function addProlongCommercialInfoToModifications(prolongInfo) {
        // TODO:
    }

    function getProlongSpecialInfo() {
        return {
            Point: $("#agreementModal #Agreement_Type_Point").val(),
            SubPoint: $("#agreementModal #Agreement_Type_SubPoint").val(),
            StartPeriod: formatDate($("#agreementModal #Agreement_Type_ProlongBeginDate").val()),
            EndPeriod: formatDate($("#agreementModal #Agreement_Type_ProlongEndDate").val()),
            UntilDismissal: $("#agreementModal #Agreement_Type_ProlongUntilDismissal").is(":checked")
        };
    }

    function addProlongSpecialInfoToContent(prolongInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var content = contentElem.val();
        var contentLines = content.split("\n");

        var nextPoint = getNextHeaderPoint(contentLines);
        var headerWildcard = "^\u200B.*?изложить в новой редакции:";
        if (!isHeaderInserted(contentLines, headerWildcard)) {
            var header = "\u200B" + nextPoint + ") изложить в новой редакции:";
            contentLines.push(header);
        }

        var text = "";
        var pointHeader = "";
        if (prolongInfo.Point !== "" && prolongInfo.SubPoint === "") {
            pointHeader = "пункт " + prolongInfo.Point;
        }
        if (prolongInfo.SubPoint !== "" && prolongInfo.Point === "") {
            pointHeader = "подпункт " + prolongInfo.SubPoint;
        }
        if (prolongInfo.SubPoint !== "" && prolongInfo.Point !== "") {
            pointHeader = "подпункт " + prolongInfo.SubPoint + " пункта " + prolongInfo.Point;
        }
        if (pointHeader !== "")
            pointHeader = pointHeader + ": ";
        text += pointHeader;
        text += "«Срок найма жилого помещения устанавливается";
        var period = " на неопределенный период";
        if (prolongInfo.StartPeriod !== "" || prolongInfo.EndPeriod !== "" || prolongInfo.UntilDismissal) {
            period = "";
        } 
        if (prolongInfo.StartPeriod !== "") {
            period += " с " + prolongInfo.StartPeriod;
        }
        if (prolongInfo.EndPeriod !== "" && !prolongInfo.UntilDismissal) {
            period += " по " + prolongInfo.EndPeriod;
        }
        if (prolongInfo.UntilDismissal) {
            if (prolongInfo.StartPeriod === "")
                period = "";
            period += " на период трудовых отношений";
        }
        text += period + "».";

        contentLines = insertPoint(contentLines, text, headerWildcard);

        contentElem.val(contentLines.join("\n"));
    }

    function addProlongSpecialInfoToModifications(prolongInfo) {
        // TODO:
    }


    function getChangeTenantInfo() {
        var kinshipElem = $("#agreementModal #Agreement_Type_TenantNewIdKinship");
        var idKinship = kinshipElem.val();
        var kinshipOption = kinshipElem.find("option[value='" + idKinship + "']");
        var personsElem = $("#agreementModal #Agreement_Type_TenancyPersonsWithoutTenant");
        var idPerson = personsElem.val();
        var personOption = personsElem.find("option[value='" + idPerson + "']");
        return {
            CurrentTenant: $("#agreementModal #Agreement_Type_Tenant").val(),
            CurrentTenantNewIdKinship: idKinship,
            CurrentTenantNewKinship: kinshipOption.text(),
            NetTenantSurname: personOption.data("surname"),
            NetTenantName: personOption.data("name"),
            NetTenantPatronymic: personOption.data("patronymic"),
            NetTenantBirthDate: personOption.data("birthdate")
        };
    }

    function addChangeTenantInfoToContent(changeTenantInfo) {
        var contentElem = $("#agreementModal #Agreement_AgreementContent");
        var contentLines = [];

        var agreementDefaultText = getDefaultAgreementText();
        var oldTenant = "";
        if (changeTenantInfo.CurrentTenant !== "") {
            oldTenant = " «" + changeTenantInfo.CurrentTenant + "»";
        }
        agreementDefaultText = agreementDefaultText.replace("договорились:",
            "в связи c ________________________________________ нанимателя" + oldTenant + ", договорились:");
        contentLines.push(agreementDefaultText);

        var text = "1) считать стороной по договору - нанимателем - ";

        var tenant = changeTenantInfo.NetTenantSurname + " " + changeTenantInfo.NetTenantName +
            (changeTenantInfo.NetTenantPatronymic !== "" ? " " + changeTenantInfo.NetTenantPatronymic : "");
        if (changeTenantInfo.NetTenantBirthDate !== "") {
            tenant += ", " + changeTenantInfo.NetTenantBirthDate + " г.р.";
        }
        tenant = "«" + tenant + "»";
        text += tenant;
        contentLines.push(text);
        contentElem.val(contentLines.join("\n"));
    }

    function addChangeTenantInfoToModifications(changeTenantInfo) {
        // TODO:
    }

    function getDefaultAgreementText() {
        return reformAgreementContent("1.1. По настоящему Соглашению Стороны по договору № {0} от {1} {2} найма жилого помещения, расположенного по адресу: {3}, договорились:");
    }

    function insertPoint(contentLines, point, headerWildcard) {
        var newContentLines = [];
        var customHeaderRegex = new RegExp(headerWildcard);
        var commonHeaderRegex = new RegExp("^\u200B?\\s*([0-9]+)\\s*[)]");
        var headerFounded = false;
        var pointInserted = false;
        for (var i = 0; i < contentLines.length; i++) {
            if (headerFounded && commonHeaderRegex.test(contentLines[i])) {
                newContentLines.push(point);
                pointInserted = true;
            }
            if (!headerFounded && customHeaderRegex.test(contentLines[i])) {
                headerFounded = true;
            }
            newContentLines.push(contentLines[i]);
        }
        if (!pointInserted) newContentLines.push(point);
        return newContentLines;
    }

    function isHeaderInserted(contentLines, headerWildcard) {
        var regex = new RegExp(headerWildcard);
        for (var i = 0; i < contentLines.length; i++) {
            if (regex.test(contentLines[i]))
                return true;
        }
        return false;
    }

    function getNextHeaderPoint(contentLines) {
        var regex = new RegExp("^\u200B?\\s*([0-9]+)\\s*[)]");
        var index = 1;
        for(var i = 0; i < contentLines.length; i++) {
            if (regex.test(contentLines[i]))
                index++;
        }
        return index;
    }

    function formatDate(date) {
        if (date !== "" && date !== null) {
            var dateParts = date.split('-');
            if (dateParts.length === 3)
                return dateParts[2] + "." + dateParts[1] + "." + dateParts[0];
            else
                return date;
        }
        return date;
    }

    var lastAgreementEndDateBeforeDismissal = undefined;

    $("#agreementModal #Agreement_Type_ProlongUntilDismissal").on("change", function (e) {
        var endDateElem = $("#Agreement_Type_ProlongEndDate");
        if ($(this).is(":checked")) {
            lastAgreementEndDateBeforeDismissal = endDateElem.val();
            endDateElem.val("");
            endDateElem.prop("disabled", "disabled");
        } else {
            endDateElem.prop("disabled", "");
            if (lastAgreementEndDateBeforeDismissal !== undefined) {
                endDateElem.val(lastAgreementEndDateBeforeDismissal);
            }
        }
        e.preventDefault();
    });

    $('#TenancyProcessAgreementsForm').on('click', '.tenancy-agreement-delete-btn', deleteTenancyAgreement);
    $("#TenancyProcessAgreementsForm").on("click", "#tenancyAgreementAdd", addTenancyAgreement);
});