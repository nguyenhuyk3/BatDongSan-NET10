window.treeSelectHelper = {
    registerOutsideClick: function (elementSelector, dotNetHelper) {
        const container = document.querySelector(elementSelector);

        const handler = (event) => {
            if (container && !container.contains(event.target)) {
                dotNetHelper.invokeMethodAsync("CloseDropdown");
            }
        };

        document.addEventListener("mousedown", handler);

        return {
            dispose: () => document.removeEventListener("mousedown", handler)
        };
    }
};