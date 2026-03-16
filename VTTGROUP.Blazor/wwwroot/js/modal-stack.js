// wwwroot/js/modal-stack.js
$(document).on('show.bs.modal', '.modal', function () {
    var openModals = $('.modal.show').length;
    var zIndex = 1040 + (10 * openModals);
    $(this).css('z-index', zIndex);

    setTimeout(function () {
        $('.modal-backdrop')
            .not('.modal-stack')
            .css('z-index', zIndex - 1)
            .addClass('modal-stack');
    }, 0);
});

$(document).on('hidden.bs.modal', '.modal', function () {
    if ($('.modal.show').length) {
        $('body').addClass('modal-open');
    }
});
