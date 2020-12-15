$(document).ready(function () {
    $('.premise-toggler').each(function (idx, e) {
        $(e).on('click', $('#' + $(e).data("for")), elementToogleHide);
    });

    $("#IdStreet").on('change', function () {
        var idStreet = $(this).val();
        if (idStreet === "") return;
        $.getJSON('/Premises/GetHouse/?' + "streetId=" + idStreet, function (data) {
            var options = "<option></option>";
            $(data).each(function (idx, elem) {
                options += "<option value=" + elem.idBuilding + ">" + elem.house + "</option>";
            });

            var buildingElem = $("select[name$='Premise.IdBuilding']");
            var idBuilding = $("input[name='IdBuildingPrev']").val();
            buildingElem.html(options).selectpicker('refresh').val(idBuilding).selectpicker('render');
        });
    });

    $("form#r-premises-form").on("submit", function (e) {
        if ($("form#r-premises-form input[type='submit']").hasClass("disabled")) {
            e.preventDefault();
            return;
        }
        $("input.decimal").each(function (idx, elem) {
            $(elem).val($(elem).val().replace(".", ","));
        });
        $("button[data-id], .bootstrap-select").removeClass("input-validation-error");
        var isFormValid = $(this).valid();
        var isOwnershipsValid = $("#ownershipRightsForm").valid();
        var isRestrictionsValid = $("#restrictionsForm").valid();
        var isSubPremisesValid = $("#subpremisesForm").valid();
        var isResettlesValid = $("#resettlesForm").valid();
        var isLitigationsValid = $("#litigationsForm").valid();

        var itemsInEditMode = $("ul.list-group .yes-no-panel").filter(function (idx, elem) {
            return $(elem).css("display") !== "none";
        });

        if (!isFormValid || !isOwnershipsValid || !isRestrictionsValid || !isSubPremisesValid || !isResettlesValid || !isLitigationsValid) {
            $("select").each(function (idx, elem) {
                var id = $(elem).prop("id");
                var name = $(elem).prop("name");
                var errorSpan = $("span[data-valmsg-for='" + name + "']");
                if (errorSpan.hasClass("field-validation-error")) {
                    $("button[data-id='" + id + "']").addClass("input-validation-error");
                }
            });

            $(".toggle-hide").each(function (idx, elem) {
                if ($(elem).find(".field-validation-error").length > 0) {
                    var toggler = $(elem).closest(".card").find("[id$='Toggle']");
                    if (!isExpandElemntArrow(toggler)) {
                        toggler.click();
                    }
                }
            });
            $([document.documentElement, document.body]).animate({
                scrollTop: $(".input-validation-error").first().offset().top - 35
            }, 1000);

            e.preventDefault();
        } else
        if (itemsInEditMode.length > 0 && action === "Edit") {
            itemsInEditMode.each(function (idx, elem) {
                if ($(elem).closest("ul.list-group").hasClass("toggle-hide")) {
                    var toggler = $(elem).closest(".card").find("[id$='Toggle']").first();
                    toggler.click();
                }
                var listGroupItem = $(elem).closest(".list-group-item");
                if (!listGroupItem.hasClass("list-group-item-warning")) {
                    listGroupItem.addClass("list-group-item-warning");
                }
            });
            $([document.documentElement, document.body]).animate({
                scrollTop: itemsInEditMode.first().closest(".list-group-item").offset().top
            }, 1000);

            e.preventDefault();
        }
        else if ($(this).data("action") === "Create") {
            var inputTemplate = "<input type='hidden' name='{0}' value='{1}'>";
            var ownerships = CreateOwnershipPremisesAssoc();
            for (var i = 0; i < ownerships.length; i++) {
                var opa = "Premise.OwnershipPremisesAssoc[" + i + "].";
                var orn = opa + "OwnershipRightNavigation.";
                $(this).append(inputTemplate.replace('{0}', opa + "IdOwnershipRight").replace('{1}', ownerships[i].IdOwnershipRight));
                $(this).append(inputTemplate.replace('{0}', opa + "IdPremises").replace('{1}', ownerships[i].IdPremises));
                $(this).append(inputTemplate.replace('{0}', orn + "Date").replace('{1}', ownerships[i].OwnershipRightNavigation.Date));
                $(this).append(inputTemplate.replace('{0}', orn + "Number").replace('{1}', ownerships[i].OwnershipRightNavigation.Number));
                $(this).append(inputTemplate.replace('{0}', orn + "Description").replace('{1}', ownerships[i].OwnershipRightNavigation.Description));
                $(this).append(inputTemplate.replace('{0}', orn + "IdOwnershipRightType").replace('{1}', ownerships[i].OwnershipRightNavigation.IdOwnershipRightType));
                $(this).append(inputTemplate.replace('{0}', orn + "DemolishPlanDate").replace('{1}', ownerships[i].OwnershipRightNavigation.DemolishPlanDate));
                $(this).append(inputTemplate.replace('{0}', orn + "ResettlePlanDate").replace('{1}', ownerships[i].OwnershipRightNavigation.ResettlePlanDate));
                var owrFile = $(ownerships[i].OwnershipRightNavigation.OwnershipRightFile).clone();
                owrFile.attr("name", "OwnershipRightFiles[" + i + "]");
                $(this).append(owrFile);
            }
            var restrictions = CreateRestrictionPremisesAssoc();
            for (var j = 0; j < restrictions.length; j++) {
                var rpa = "Premise.RestrictionPremisesAssoc[" + j + "].";
                var rrn = rpa + "RestrictionNavigation.";
                $(this).append(inputTemplate.replace('{0}', rpa + "IdRestriction").replace('{1}', restrictions[j].IdRestriction));
                $(this).append(inputTemplate.replace('{0}', rpa + "IdPremises").replace('{1}', restrictions[j].IdPremises));
                $(this).append(inputTemplate.replace('{0}', rrn + "Date").replace('{1}', restrictions[j].RestrictionNavigation.Date));
                $(this).append(inputTemplate.replace('{0}', rrn + "DateStateReg").replace('{1}', restrictions[j].RestrictionNavigation.DateStateReg));
                $(this).append(inputTemplate.replace('{0}', rrn + "Number").replace('{1}', restrictions[j].RestrictionNavigation.Number));
                $(this).append(inputTemplate.replace('{0}', rrn + "Description").replace('{1}', restrictions[j].RestrictionNavigation.Description));
                $(this).append(inputTemplate.replace('{0}', rrn + "IdRestrictionType").replace('{1}', restrictions[j].RestrictionNavigation.IdRestrictionType));
                var restrictionFile = $(restrictions[j].RestrictionNavigation.RestrictionFile).clone();
                restrictionFile.attr("name", "RestrictionFiles[" + j + "]");
                $(this).append(restrictionFile);
            }
            var subPremises = getSubPremises();
            for (var k = 0; k < subPremises.length; k++) {
                var sp = "Premise.SubPremises[" + k + "].";
                $(this).append(inputTemplate.replace('{0}', sp + "IdSubPremises").replace('{1}', subPremises[k].IdSubPremises));
                $(this).append(inputTemplate.replace('{0}', sp + "IdPremises").replace('{1}', subPremises[k].IdPremises));
                $(this).append(inputTemplate.replace('{0}', sp + "SubPremisesNum").replace('{1}', subPremises[k].SubPremisesNum));
                $(this).append(inputTemplate.replace('{0}', sp + "TotalArea").replace('{1}', subPremises[k].TotalArea));
                $(this).append(inputTemplate.replace('{0}', sp + "LivingArea").replace('{1}', subPremises[k].LivingArea));
                $(this).append(inputTemplate.replace('{0}', sp + "IdState").replace('{1}', subPremises[k].IdState));
                $(this).append(inputTemplate.replace('{0}', sp + "Description").replace('{1}', subPremises[k].Description));
                $(this).append(inputTemplate.replace('{0}', sp + "CadastralNum").replace('{1}', subPremises[k].CadastralNum));
                $(this).append(inputTemplate.replace('{0}', sp + "CadastralCost").replace('{1}', subPremises[k].CadastralCost));
                $(this).append(inputTemplate.replace('{0}', sp + "BalanceCost").replace('{1}', subPremises[k].BalanceCost));
                $(this).append(inputTemplate.replace('{0}', sp + "Account").replace('{1}', subPremises[k].Account));
                $(this).append(inputTemplate.replace('{0}', "SubPremisesFundTypes[" + k + "]").replace('{1}', subPremises[k].IdFundType));
            }

            var resettles = CreateResettlePremisesAssoc();
            for (var l = 0; l < resettles.length; l++) {
                var rspa = "Premise.ResettlePremisesAssoc[" + l + "].";
                var rin = rspa + "ResettleInfoNavigation.";
                $(this).append(inputTemplate.replace('{0}', rspa + "IdResettleInfo").replace('{1}', resettles[l].IdResettleInfo));
                $(this).append(inputTemplate.replace('{0}', rspa + "IdPremises").replace('{1}', resettles[l].IdPremises));
                $(this).append(inputTemplate.replace('{0}', rin + "ResettleDate").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleDate));
                $(this).append(inputTemplate.replace('{0}', rin + "IdResettleKind").replace('{1}', resettles[l].ResettleInfoNavigation.IdResettleKind));
                $(this).append(inputTemplate.replace('{0}', rin + "IdResettleKindFact").replace('{1}', resettles[l].ResettleInfoNavigation.IdResettleKindFact));
                $(this).append(inputTemplate.replace('{0}', rin + "IdResettleStage").replace('{1}', resettles[l].ResettleInfoNavigation.IdResettleStage));
                $(this).append(inputTemplate.replace('{0}', rin + "FinanceSource1").replace('{1}', resettles[l].ResettleInfoNavigation.FinanceSource1));
                $(this).append(inputTemplate.replace('{0}', rin + "FinanceSource2").replace('{1}', resettles[l].ResettleInfoNavigation.FinanceSource2));
                $(this).append(inputTemplate.replace('{0}', rin + "FinanceSource3").replace('{1}', resettles[l].ResettleInfoNavigation.FinanceSource3));
                $(this).append(inputTemplate.replace('{0}', rin + "FinanceSource4").replace('{1}', resettles[l].ResettleInfoNavigation.FinanceSource4));
                $(this).append(inputTemplate.replace('{0}', rin + "Description").replace('{1}', resettles[l].ResettleInfoNavigation.Description));
                if (resettles[l].ResettleInfoNavigation.ResettleInfoTo !== null) {
                    for (let s = 0; s < resettles[l].ResettleInfoNavigation.ResettleInfoTo.length; s++) {
                        let rit = rin + "ResettleInfoTo[" + s + "].";
                        $(this).append(inputTemplate.replace('{0}', rit + "IdResettleInfo").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleInfoTo[s].IdResettleInfo));
                        $(this).append(inputTemplate.replace('{0}', rit + "IdObject").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleInfoTo[s].IdObject));
                        $(this).append(inputTemplate.replace('{0}', rit + "ObjectType").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleInfoTo[s].ObjectType));
                    }
                }
                if (resettles[l].ResettleInfoNavigation.ResettleInfoToFact !== null) {
                    for (let s = 0; s < resettles[l].ResettleInfoNavigation.ResettleInfoToFact.length; s++) {
                        let rit = rin + "ResettleInfoToFact[" + s + "].";
                        $(this).append(inputTemplate.replace('{0}', rit + "IdResettleInfo").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleInfoToFact[s].IdResettleInfo));
                        $(this).append(inputTemplate.replace('{0}', rit + "IdObject").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleInfoToFact[s].IdObject));
                        $(this).append(inputTemplate.replace('{0}', rit + "ObjectType").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleInfoToFact[s].ObjectType));
                    }
                }
                if (resettles[l].ResettleInfoNavigation.ResettleDocuments !== null) {
                    for (var t = 0; t < resettles[l].ResettleInfoNavigation.ResettleDocuments.length; t++) {
                        rit = rin + "ResettleDocuments[" + t + "].";
                        $(this).append(inputTemplate.replace('{0}', rit + "IdResettleInfo").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleDocuments[t].IdResettleInfo));
                        $(this).append(inputTemplate.replace('{0}', rit + "IdDocument").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleDocuments[t].IdDocument));
                        $(this).append(inputTemplate.replace('{0}', rit + "Number").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleDocuments[t].Number));
                        $(this).append(inputTemplate.replace('{0}', rit + "Date").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleDocuments[t].Date));
                        $(this).append(inputTemplate.replace('{0}', rit + "Description").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleDocuments[t].Description));
                        $(this).append(inputTemplate.replace('{0}', rit + "IdDocumentType").replace('{1}', resettles[l].ResettleInfoNavigation.ResettleDocuments[t].IdDocumentType));
                        var resettleDocumentFile = $(resettles[l].ResettleInfoNavigation.ResettleDocuments[t].ResettleDocumentFile).clone();
                        resettleDocumentFile.attr("name", "Premise.ResettlePremisesAssoc[" + l + "].ResettleDocumentFiles[" + t + "]");
                        $(this).append(resettleDocumentFile);
                    }
                }
            }

            var litigations = CreateLitigationPremisesAssoc();
            for (var m = 0; m < litigations.length; m++) {
                var lpa = "Premise.LitigationPremisesAssoc[" + m + "].";
                var ln = lpa + "LitigationNavigation.";
                $(this).append(inputTemplate.replace('{0}', lpa + "IdLitigation").replace('{1}', litigations[m].IdLitigation));
                $(this).append(inputTemplate.replace('{0}', lpa + "IdPremises").replace('{1}', litigations[m].IdPremises));
                $(this).append(inputTemplate.replace('{0}', ln + "Date").replace('{1}', litigations[m].LitigationNavigation.Date));
                $(this).append(inputTemplate.replace('{0}', ln + "Number").replace('{1}', litigations[m].LitigationNavigation.Number));
                $(this).append(inputTemplate.replace('{0}', ln + "Description").replace('{1}', litigations[m].LitigationNavigation.Description));
                $(this).append(inputTemplate.replace('{0}', ln + "IdLitigationType").replace('{1}', litigations[m].LitigationNavigation.IdLitigationType));
                var litigationFile = $(litigations[m].LitigationNavigation.LitigationFile).clone();
                litigationFile.attr("name", "LitigationFiles[" + m + "]");
                $(this).append(litigationFile);
            }

        }
    });

    $("form").on("change", "select", function () {
        var isValid = $(this).valid();
        var id = $(this).prop("id");
        if (!isValid) {
            $("button[data-id='" + id + "']").addClass("input-validation-error");
        } else {

            $("button[data-id='" + id + "']").removeClass("input-validation-error");
        } 
    });

    $("select#IdStreet").val($("input[name='IdStreetPrev']").val()).selectpicker('refresh').change();

    var action = $('#r-premises-form').data("action");
    if (action === "Details" || action === "Delete" || $("form#r-premises-form input[type='submit']").hasClass("disabled")) {
        $('select').prop('disabled', true);
        $('input').prop('disabled', true);
        $('textarea').prop('disabled', true);
        $('input[type="hidden"]').prop('disabled', false);
        $('input[type="submit"]').prop('disabled', false);
    }

});