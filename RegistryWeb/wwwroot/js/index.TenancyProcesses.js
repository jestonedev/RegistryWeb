var searchModal = function () {
    addressClear();
    $("form.filterForm").submit();
};


var filterClearModal = function () {
    $("#filterModal input[type='text'], #filterModal input[type='date'], #filterModal input[type='hidden'], #filterModal select").val("");
    $('#FilterOptions_IdStreet, #FilterOptions_IdPreset').selectpicker('render');
    $('#FilterOptions_IdsRentType').selectpicker('deselectAll');
    $('#FilterOptions_IdsObjectState').selectpicker("deselectAll");
    $('#FilterOptions_IdsOwnershipRightType').selectpicker("deselectAll");
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

var selectRegNumState = function(e) {
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

var banLetters = function()
{
    if ((event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 95 || event.keyCode > 105) || event.keyCode == 190 || event.keyCode == 188) // Запрещаем все, кроме цифр на основной клавиатуре, а так же Num-клавиатуре        
        event.preventDefault();
};

$(function () {

    /*if ($("input").hasClass("input-validation-error"))
        return false;*/

    $('#searchModalBtn').click(searchModal);
    $('#filterClearModalBtn').click(filterClearModal);
    $('#filterClearBtn').click(filterClear);
    initRegNumState();
    $(".rr-registration-num-dropdown a.dropdown-item").click(selectRegNumState);


    $("#filterModalShow").on("click", function (e) {
        e.preventDefault();
        var modal = $("#filterModal");
        modal.modal('show');
    });

    $('body').on('keydown', '.only_letter', function (event) {
        console.log(event.keyCode + "   " + event.key);
        if (event.keyCode == 46 || event.keyCode == 8)// || event.keyCode == 110 || event.keyCode == 190 || event.keyCode == 188 || event.keyCode == 37 || event.keyCode == 39)
        {
            return;
        } else if ((event.keyCode > 47 && event.keyCode < 58) || (event.keyCode > 95 && event.keyCode < 108) || (event.keyCode >= 110 && event.keyCode <= 111) ) // Запрещаем цифры на основной клавиатуре, а также Num-клавиатуре
        {
            event.preventDefault();
        }
    });

    $('body').on('keydown', '.only_number', function (event) // Запрещаем все, кроме цифр на основной клавиатуре, а так же Num-клавиатуре
    {
        //console.log(event.keyCode + "   " + event.key);

        if (event.keyCode == 46 || event.keyCode == 8)
            return;
        else if (event.key.match(/([а-яА-Я]+)/) || event.key.match(/([a-zA-Z]+)/) || event.key != "Backspace" || event.key != "Delete")
            event.preventDefault();
        banLetters();        
    });

    $('body').on('keydown', '.homecadastral', function (event) // Запрещаем все, кроме цифр на основной клавиатуре, а так же Num-клавиатуре
    {
        //console.log(event.keyCode + "   " + event.key);

        if (event.keyCode == 46 || event.keyCode == 8 || (event.shiftKey && event.keyCode == 54) || event.keyCode == 191 || (event.shiftKey && event.keyCode == 220) || event.keyCode == 220 || (event.shiftKey && (event.keyCode == 54 || event.keyCode == 186)))
            return;
        else if (event.key.match(/([а-яА-Я]+)/) || event.key.match(/([a-zA-Z]+)/) || event.key != "Backspace" || event.key != "Delete")
            event.preventDefault();
        banLetters();        
    });

});