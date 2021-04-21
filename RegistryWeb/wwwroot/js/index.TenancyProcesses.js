var searchModal = function () {
    addressClear();
    $("form.filterForm").submit();
};

var filterClearModal = function () {
    resetModalForm($("form.filterForm"));
    filterIdRegionChange();
    $("#FilterOptions_RegistrationDateSign, #FilterOptions_IssuedDateSign, #FilterOptions_BeginDateSign, #FilterOptions_EndDateSign")
        .val("=")
        .closest(".input-group")
        .find("button.dropdown-toggle")
        .text("=");
};
var filterClear = function () {
    filterClearModal();
    $("form.filterForm").submit();
};

var initRegNumState = function () {
    var inputRegNum = $("#FilterOptions_RegistrationNum");
    var regNumIsEmpty = $("#FilterOptions_RegistrationNumIsEmpty").val();
    if (regNumIsEmpty === "True") {
        inputRegNum.val("");
        inputRegNum.prop("disabled", "disabled");
        var wrapperDropDown = inputRegNum.closest(".input-group").find(".rr-registration-num-dropdown");
        wrapperDropDown.find("button.dropdown-toggle").text("Нет");
    }
};

var selectRegNumState = function (e) {
    var value = $(this).text();
    var wrapperDropDown = $(this).closest(".rr-registration-num-dropdown");
    var wrapper = wrapperDropDown.closest(".input-group");
    var inputRegNum = wrapper.find("#FilterOptions_RegistrationNum");
    var inputRegNumIsEmpty = wrapper.find("#FilterOptions_RegistrationNumIsEmpty");
    wrapperDropDown.find("button.dropdown-toggle").text(value);
    if (value === "Нет") {
        inputRegNum.val("");
        inputRegNum.prop("disabled", "disabled");
        inputRegNumIsEmpty.val("True");
    } else {
        inputRegNum.prop("disabled", "");
        inputRegNumIsEmpty.val("False");
    }
};
var selectRegistrationDateState = function (e) {
    var value = $(this).text();
    var wrapperDropDown = $(this).closest(".rr-registration-date-dropdown");
    wrapperDropDown.find("button.dropdown-toggle").text(value);
    wrapperDropDown
        .closest(".input-group")
        .find("#FilterOptions_RegistrationDateSign")
        .val(value);
};
var selectIssuedDateState = function (e) {
    var value = $(this).text();
    var wrapperDropDown = $(this).closest(".rr-issued-date-dropdown");
    wrapperDropDown.find("button.dropdown-toggle").text(value);
    wrapperDropDown
        .closest(".input-group")
        .find("#FilterOptions_IssuedDateSign")
        .val(value);
};
var selectBeginDateState = function (e) {
    var value = $(this).text();
    var wrapperDropDown = $(this).closest(".rr-begin-date-dropdown");
    wrapperDropDown.find("button.dropdown-toggle").text(value);
    wrapperDropDown
        .closest(".input-group")
        .find("#FilterOptions_BeginDateSign")
        .val(value);
};
var selectEndDateState = function (e) {
    var value = $(this).text();
    var wrapperDropDown = $(this).closest(".rr-end-date-dropdown");
    wrapperDropDown.find("button.dropdown-toggle").text(value);
    wrapperDropDown
        .closest(".input-group")
        .find("#FilterOptions_EndDateSign")
        .val(value);
};
var filterIdRegionChange = function () {
    var idRegion = $('#FilterOptions_IdRegion').selectpicker('val');
    $.ajax({
        type: 'POST',
        url: window.location.origin + '/Address/GetKladrStreets',
        dataType: 'json',
        data: { idRegion },
        success: function (data) {
            var select = $('#filterModal #FilterOptions_IdStreet');
            select.selectpicker('destroy');
            select.find('option[value]').remove();
            $.each(data, function (i, d) {
                select.append('<option value="' + d.idStreet + '">' + d.streetName + '</option>');
            });
            select.selectpicker();
        }
    });
}
$(function () {
    $('#searchModalBtn').click(searchModal);
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);
    initRegNumState();
    $('#filterModal #FilterOptions_IdRegion').on('change', function (e) {
        filterIdRegionChange();
        e.preventDefault();
    });
    $(".rr-registration-num-dropdown a.dropdown-item").click(selectRegNumState);
    $(".rr-registration-date-dropdown a.dropdown-item").click(selectRegistrationDateState);
    $(".rr-issued-date-dropdown a.dropdown-item").click(selectIssuedDateState);
    $(".rr-begin-date-dropdown a.dropdown-item").click(selectBeginDateState);
    $(".rr-end-date-dropdown a.dropdown-item").click(selectEndDateState);

    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        modal.modal('show');
    });
});