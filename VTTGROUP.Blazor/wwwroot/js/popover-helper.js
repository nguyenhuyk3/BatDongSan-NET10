window.popoverHelper = {
    initPopover(element) {
        if (!element) return;
        const pop = bootstrap.Popover.getInstance(element);
        if (pop) { pop.dispose(); }
        new bootstrap.Popover(element);
    },
    disposePopover(element) {
        if (!element) return;
        const pop = bootstrap.Popover.getInstance(element);
        if (pop) { pop.dispose(); }
    }
};