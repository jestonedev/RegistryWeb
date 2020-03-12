var arrowAnimation = function (arrow) {
    if (arrow.html() === '∧') {
        arrow.html('∨');
    }
    else {
        arrow.html('∧');
    }
}