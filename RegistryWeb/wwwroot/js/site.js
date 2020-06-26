if ($.validator !== undefined) {
    $.extend($.validator.methods, {
        number: function (value, element) {
            return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
        },
        range: function (value, element, param) {
            return this.optional(element) || (Number(value.replace(",", ".")) >= param[0] && Number(value.replace(",", ".")) <= param[1]);
        }
    });
    $(document).ready(function () {
        $("[data-val-number]").attr("data-val-number", "Введите числовое значение");
        var form = $("form")
            .removeData("validator")
            .removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(form);
        form.validate();
    });
}
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
}
$(function () {
    $("input[type='number']").inputSpinner();
});

function uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
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