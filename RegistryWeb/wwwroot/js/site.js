﻿let guid = function () {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
        s4() + '-' + s4() + s4() + s4();
}
if ($.validator !== undefined) {
    $.extend($.validator.methods, {
        number: function (value, element) {
            return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
        }
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
}
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