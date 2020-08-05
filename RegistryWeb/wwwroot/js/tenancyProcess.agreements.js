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
        var birthDate = personElem.find("input[id^='DateOfBirth']").val();
        if (birthDate !== "" && birthDate !== null) {
            var birthDateParts = birthDate.split("-");
            birthDate = birthDateParts[2] + "." + birthDateParts[1] + "." + birthDateParts[0];
        }
        return "<option data-guid='" + guid + "' value='" + idPerson + "'>" +
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
        var idRentType = $('#TenancyProcessForm #TenancyProcess_IdRentType').val();
        var rentTypeGenetive = $('#TenancyProcessForm input[name="RentTypeGenetive"]').filter(function (idx, elem) { return $(elem).val() === idRentType; }).first().data("genetive");
        if (rentTypeGenetive === undefined) {
            rentTypeGenetive = "";
        }
        var regNumber = $("#TenancyProcessForm #TenancyProcess_RegistrationNum").val();
        var regDate = $("#TenancyProcessForm #TenancyProcess_RegistrationDate").val();
        if (regDate !== "" && regDate !== null) {
            var regDateParts = regDate.split("-");
            regDate = regDateParts[2] + "." + regDateParts[1] + "." + regDateParts[0];
        }
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
        rentObjects = rentObjects.toArray().sort(sortRentObjects);

        var rentAddress = buildFullRentAddress(rentObjects);

        agreementContent = agreementContent.replace("{0}", regNumber);
        agreementContent = agreementContent.replace("{1}", regDate);
        agreementContent = agreementContent.replace("{2}", rentTypeGenetive);
        agreementContent = agreementContent.replace("{3}", rentAddress);

        return agreementContent;
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
        var isValid = $("#agreementModal .rr-agreement-type-field").find("input, select, textarea").valid();
        if (!isValid) {
            refreshSelectpickerValidationBorders($("#TenancyProcessAgreementsModalForm"));
        }
        e.preventDefault();
    });

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