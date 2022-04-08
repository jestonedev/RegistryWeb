if ($.validator !== undefined) {
    $.extend($.validator.methods, {
        number: function (value, element) {
            return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
        },
        range: function (value, element, param) {
            return this.optional(element) || (Number(value.replace(",", ".")) >= param[0] && Number(value.replace(",", ".")) <= param[1]);
        },
        required: function (b, c, d) {
            return $.trim(b).length > 0;
        }
    });
    $(document).ready(function () {
        $("[data-val-number]").attr("data-val-number", "Введите числовое значение");
        $("input[type='number']").each(function (idx, elem) {
            $(elem).inputSpinner();
            var formGroup = $(elem).closest(".form-group");
            var input = formGroup.find("[inputmode='decimal']");
            input.attr("data-val", $(elem).attr("data-val"));
            input.attr("data-val-required", $(elem).attr("data-val-required"));
            input.attr("id", $(elem).attr("id") + "_decimal");
            input.attr("name", $(elem).attr("name") + "_decimal");
            var span = formGroup.find("[data-valmsg-for='" + $(elem).attr("name") + "']");
            span.attr("data-valmsg-for", $(elem).attr("name") + "_decimal");
        });

        if ($.validator !== undefined) {
            refreshValidationForm($("form"));
        }
    });
}

$(document).ready(
    function () {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
);

$.fn.inputFilter = function (inputFilter) {
    return this.on("input keydown keyup mousedown mouseup select contextmenu drop", function () {
        if (inputFilter(this.value)) {
            this.oldValue = this.value;
            this.oldSelectionStart = this.selectionStart;
            this.oldSelectionEnd = this.selectionEnd;
        } else if (this.hasOwnProperty("oldValue")) {
            this.value = this.oldValue;
            this.setSelectionRange(this.oldSelectionStart, this.oldSelectionEnd);
        } else {
            this.value = "";
        }
    });
};

//Формат вызова:
//$('1').on('click', 2, elementToogle);
//  1 - элемент по которому щелкают (стрелочка)
//  2 - Элемент который надо тоглить
let elementToogle = function (event) {
    let elementArrow = $(this);
    let isDisplay = isExpandElemntArrow(elementArrow);
    //свернуть
    if (isDisplay) {
        elementArrow.html('∨');
    }
    //развернуть
    else {
        elementArrow.html('∧');
    }
    event.data.toggle(!isDisplay);
    event.preventDefault();
};

// Переделанная версия функции elementToogle. elementToogle оставлена, для обратной совместимости
let elementToogleHide = function (event) {
    let elementArrow = $(this);
    let isDisplay = isExpandElemntArrow(elementArrow);
    //свернуть
    if (isDisplay) {
        elementArrow.html('∨');
        event.data.addClass("toggle-hide");
    }
    //развернуть
    else {
        elementArrow.html('∧');
        event.data.removeClass("toggle-hide");
    }
    //event.data.toggle(!isDisplay);
    event.preventDefault();
};

let arrowAnimation = function (elemntArrow) {
    //свернуть
    if (elemntArrow.html() === '∧') {
        elemntArrow.html('∨');
        return false;
    }
    //развернуть
    else {
        elemntArrow.html('∧');
        return true;
    }
};
//Если развернуто, то истина
let isExpandElemntArrow = function (elemntArrow) {
    if (elemntArrow.html() === '∧')
        return true;
    return false;
};

function uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function downloadFile(url) {
    var link = document.createElement('a');
    link.href = url;
    link.target = "_blank";
    link.style.display = "none";
    document.getElementsByTagName("body")[0].appendChild(link);
    link.click();
}

function fixBootstrapSelectHighlight(form) {
    form.find("select").each(function (idx, elem) {
        var id = $(elem).prop("id");
        var name = $(elem).prop("name");
        var errorSpan = form.find("span[data-valmsg-for='" + name + "']");
        if (errorSpan.hasClass("field-validation-error")) {
            form.find("button[data-id='" + id + "']").addClass("input-validation-error");
        } else {
            form.find("button[data-id='" + id + "']").removeClass("input-validation-error");
        }
    });
}

function fixBootstrapSelectHighlightOnChange(select) {
    var isValid = select.valid();
    var id = select.prop("id");
    if (!isValid) {
        select.closest("form").find("button[data-id='" + id + "']").addClass("input-validation-error");
    } else {

        select.closest("form").find("button[data-id='" + id + "']").removeClass("input-validation-error");
    }
}

function refreshValidationForm(form) {
    form
        .removeData("validator")
        .removeData("unobtrusiveValidation");
    $.validator.unobtrusive.parse(form);
    form.validate();
};

function clearValidationError(elem) {
    var spanError = $("span[data-valmsg-for='" + elem.attr("name") + "']");
    spanError.empty().removeClass("field-validation-error").addClass("field-validation-valid");
    elem.removeClass("input-validation-error");
}

function removeErrorFromValidator(validator, elem) {
    validator.errorList = $(validator.errorList)
        .filter(function (idx, error) {
            return $(error.element).prop("name") !== elem.attr("name");
        });

    delete validator.errorMap[elem.attr("name")];
}

function resetModalForm(jQElem, isForm = true) {
    var form = null;
    if (isForm) {
        form = jQElem;
    }
    else {
        form = jQElem.closest("form");
    }
    form[0].reset();
    var inputs = form.find("input");
    inputs.val(null);
    var selects = form.find("select");
    selects.val(null);
    selects.selectpicker("deselectAll");
    selects.selectpicker("refresh");
}

//Функция вычисления количества элементов для компонентов
function countBadges(idNameComponentForm, flag) {
    var count = flag ? $(idNameComponentForm).find('.list-group-item').length-1 : $(idNameComponentForm).find('.list-group-item').length;
    if (count > 0) {
        $(idNameComponentForm).find(".rr-count-badge").text(count);
        $(idNameComponentForm).find(".rr-count-badge").css("display", "inline-block");
    }
    else {
        $(idNameComponentForm).find(".rr-count-badge").text('');
        $(idNameComponentForm).find(".rr-count-badge").css("display", "none");
    }
}

$(function () {
    var timeout = undefined;

    $(".btn-group .dropdown-toggle, .btn-group .dropdown-menu").hover(function () {
        if (timeout !== undefined) {
            clearTimeout(timeout);
            timeout = undefined;
        }
        if ($(this).closest(".btn-group").hasClass("show")) return;
        $(this).closest(".btn-group").find(".dropdown-toggle").dropdown('toggle');
    }, function () {
        if (!$(this).closest(".btn-group").hasClass("show")) return;
        var self = this;
        timeout = setTimeout(function () {
            if (timeout !== undefined) {
                clearTimeout(timeout);
                timeout = undefined;
            }
            if (!$(self).closest(".btn-group").hasClass("show")) return;
            $(self).closest(".btn-group").find(".dropdown-toggle").dropdown('toggle');
        }, 200);
    });
    

    $(".modal").on("show.bs.modal", function () {
        $(this).find('select').selectpicker('refresh');
    });

    $(".modal").on("hide.bs.modal", function () {
        $(this).find(".input-validation-error").removeClass("input-validation-error").addClass("valid");
        $(this).find(".field-validation-error").removeClass("field-validation-error").addClass("field-validation-valid").text("");
    });

    $('.input-filter-numbers, .input-numbers').inputFilter(function (value) {
        return /^\d*$/.test(value);
    });

    $('.input-filter-chars, .input-chars').inputFilter(function (value) {
        return /^[а-яА-ЯёЁ]*$/.test(value);
    });

    $('.input-filter-snp, .input-snp').inputFilter(function (value) {
        return /^([а-яА-ЯёЁ]+[ ]?)*$/.test(value);
    });

    $('.input-filter-cadastral-num, .input-cadastral-num').inputFilter(function (value) {
        return /^(\d+:?)*$/.test(value) || "нет данных".startsWith(value);
    });

    $('.input-cadastral-num').on('blur', function () {
        if ("нет данных".startsWith($(this).val()) && $(this).val() !== "" && $(this).val() !== undefined) {
            $(this).val("нет данных");
        }
    });

    $('.input-filter-decimal, .input-decimal').inputFilter(function (value) {
        return /^-?\d*[.,]?\d{0,2}$/.test(value);
    });

    $('.input-filter-premise-num, .input-premise-num').inputFilter(function (value) {
        return /^[0-9\\/а-яА-Я,-]*$/.test(value);
    });

    $('.input-filter-house, .input-house').inputFilter(function (value) {
        return /^[0-9\\/а-яА-Я]*$/.test(value);
    });

    $('.input-filter-account-zkh, .input-account-zkh').inputFilter(function (value) {
        return /^[0-9а-яА-Я]*$/.test(value);
    });


    var minDate = undefined;
    var currentCost = undefined;

    $("#areaAvgCostModalBtn").on("click", function (e) {
        if ($("#areaAvgCostForm").length > 0) {
            $("#areaAvgCostForm").remove();
        }
        $.ajax({
            async: false,
            type: 'POST',
            url: window.location.origin + '/Premises/GetAreaAvgCostView',
            success: function (modal) {
                $("body").append(modal);
                $("#areaAvgCostForm [data-val-number]").attr("data-val-number", "Введите числовое значение");
                refreshValidationForm($("#areaAvgCostForm"));
                minDate = $("#areaAvgCostForm").find("#Date").val();
                currentCost = $("#areaAvgCostForm").find("#Cost").val();
                $("#areaAvgCostModal").modal("show");
            }
        });
        e.preventDefault();
    });

    $("body").on("click", "#areaAvgCostFormSubmit", function (e) {
        var form = $(this).closest("form");
        var isValid = form.valid();

        var validator = form.validate();
        if (new Date($("#areaAvgCostForm #Date").val()) <= new Date(minDate)) {
            let error = {};
            error["Date"] = "Дата должна быть больше ранее внесенной";
            validator.showErrors(error);
            isValid = false;
        }
        if ($("#areaAvgCostForm #Cost").val() === currentCost) {
            let error = {};
            error["Cost"] = "Стоимость квадратного метра не изменена";
            validator.showErrors(error);
            isValid = false;
        }

        if (isValid) {
            $.ajax({
                async: false,
                type: 'POST',
                url: window.location.origin + '/Premises/UpdateAreaAvgCost',
                data: { id: $("#areaAvgCostForm #Id").val(), cost: $("#areaAvgCostForm #Cost").val(), date: $("#areaAvgCostForm #Date").val() },
                success: function (result) {
                    if (result === 1) {
                        $("#areaAvgCostModal").modal("hide");
                    } else {
                        alert("У вас нет прав на изменение цены 1 кв. м. жилья");
                    }
                }
            });
        }
    });
});
