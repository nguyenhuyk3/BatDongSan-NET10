window.modalHelper = {
    draggableModals: {},
    makeDraggable: function (modalId) {
        const modal = document.getElementById(modalId);
        if (!modal) return;

        const header = modal.querySelector('.modal-header');
        if (!header) return;

        let isMouseDown = false, offset = [0, 0];

        header.style.cursor = 'move';

        header.addEventListener('mousedown', function (e) {
            isMouseDown = true;
            offset = [
                modal.offsetLeft - e.clientX,
                modal.offsetTop - e.clientY
            ];
        });

        document.addEventListener('mouseup', function () {
            isMouseDown = false;
        });

        document.addEventListener('mousemove', function (e) {
            if (isMouseDown) {
                modal.style.margin = 0;
                modal.style.position = 'absolute';
                modal.style.left = (e.clientX + offset[0]) + 'px';
                modal.style.top = (e.clientY + offset[1]) + 'px';
            }
        });
    },
    makeAllDraggable: function () {
        const modals = document.querySelectorAll(".modal");

        modals.forEach(modal => {
            const header = modal.querySelector(".modal-header");
            if (!header) return;

            const modalId = modal.id || Math.random().toString(36).substring(2);
            modal.setAttribute("data-modal-id", modalId);

            let isMouseDown = false;
            let offset = [0, 0];

            header.style.cursor = "move";

            header.addEventListener("mousedown", function (e) {
                isMouseDown = true;
                offset = [
                    modal.offsetLeft - e.clientX,
                    modal.offsetTop - e.clientY
                ];
            });

            document.addEventListener("mouseup", function () {
                isMouseDown = false;
            });

            document.addEventListener("mousemove", function (e) {
                if (isMouseDown) {
                    const left = e.clientX + offset[0];
                    const top = e.clientY + offset[1];

                    modal.style.margin = 0;
                    modal.style.position = "absolute";
                    modal.style.left = left + "px";
                    modal.style.top = top + "px";

                    window.modalHelper.draggableModals[modalId] = { left, top };
                }
            });

            // Restore vị trí nếu đã kéo trước đó
            const saved = window.modalHelper.draggableModals[modalId];
            if (saved) {
                modal.style.position = "absolute";
                modal.style.left = saved.left + "px";
                modal.style.top = saved.top + "px";
                modal.style.margin = 0;
            }

            // Khi modal đóng → reset lại vị trí nếu muốn
            modal.addEventListener("hidden.bs.modal", function () {
                modal.style.position = "";
                modal.style.left = "";
                modal.style.top = "";
                modal.style.margin = "";

                delete window.modalHelper.draggableModals[modalId];
            });
        });
    }
};