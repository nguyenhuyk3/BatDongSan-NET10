window.UiHelper = {
    hideByClass: function (className) {
        const elements = document.querySelectorAll("." + className);
        elements.forEach(el => el.classList.add("d-none"));
    },

    showByClass: function (className) {
        const elements = document.querySelectorAll("." + className);
        elements.forEach(el => el.classList.remove("d-none"));
    },

    hideByElement: function (el) {
        if (el) el.classList.add("d-none");
    },

    showByElement: function (el) {
        if (el) el.classList.remove("d-none");
    },
    // ✅ SỬA Ở ĐÂY: toggle: function ...
    toggle: function (className, show, inputIds, requiredWhenShow) {
        const wraps = document.getElementsByClassName(className);
        Array.from(wraps).forEach(w => {
            if (show) {
                w.classList.remove("d-none");
                if (w.classList.contains("validation-group")) {
                    w.querySelectorAll(".form-control").forEach(ctrl => {
                        ctrl.classList.add("input-validation");
                    });
                }
            }
            else {
                w.classList.add("d-none");
                if (w.classList.contains("validation-group")) {
                    w.querySelectorAll(".form-control").forEach(ctrl => {
                        ctrl.classList.remove("input-validation");
                    });
                }
            }
        });

        (inputIds || []).forEach(id => {
            const el = document.getElementById(id);
            if (!el) return;

            if (show) {
                el.disabled = false;
                if (requiredWhenShow) el.setAttribute("required", "required");
            } else {
                el.value = "";
                el.removeAttribute("required");
                el.disabled = true;
            }
        });
    }
};

