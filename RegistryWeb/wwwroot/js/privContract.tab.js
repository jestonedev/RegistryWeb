$(function () {
    $('body').on('keydown', 'input,a,select,button', function (e) {
        var self = $(this);
        if (self.attr("id") === "FilterOptions_FrontSideRegNumber") return true;
        if (e.key === " " && this.tagName === "A") {
            self.click();
            return false;
        } else
            if (e.key === "Enter") {
                var form = self.parents('form:eq(0)'), focusable, next;
                if (self.closest(".dropdown-menu").length > 0) return true;
                focusable = form.find('input,a,select,button,textarea').filter(':visible:not(:disabled)');
                next = focusable.eq(focusable.index(this) + 1);
                if (next.length) {
                    next.focus();
                }
                if (this.tagName !== "TEXTAREA")
                    return false;
            }
    });
});