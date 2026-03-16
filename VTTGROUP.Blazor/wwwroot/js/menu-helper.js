window.menuHelper = {
    updateSubMenuPosition: function () {
        const sidebar = document.getElementById("jqueryslidemenu");
        if (!sidebar) return;

        const rect = sidebar.getBoundingClientRect();
        //const sidebarTop = rect.top + window.scrollY - 20;
        //const sidebarLeft = rect.left + window.scrollX;
        //const sidebarWidth = rect.width;

        // Tính phần còn lại của màn hình
        //const remainingWidth = window.innerWidth - sidebarWidth;

        // Cập nhật tất cả submenu đang hiển thị
        //document.querySelectorAll(".mega-menu.wapper-nav-child.not-parent").forEach(el => {
        //    el.style.top = `0px`;
        //    el.style.left = `${237 - 20}px`;
        //    el.style.width = `${355 + 20}px`;
        //    //el.style.top = `${sidebarTop}px`;
        //    //el.style.left = `${sidebarLeft + sidebarWidth - 20}px`;
        //    //el.style.width = `${remainingWidth + 20}px`;
        //});
    },

    initResizeListener: function () {
        window.addEventListener("resize", () => {
            window.menuHelper.updateSubMenuPosition();
        });
    },
    closeSidebarOverlay: function () {
        document.querySelectorAll(".sidebar-overlay").forEach(el => {
            if (el && el.classList) {
                el.classList.add("hide"); // Thêm class ẩn mượt
            }
        });

        document.querySelectorAll(".mega-menu.not-parent").forEach(el => {
            if (el && el.classList) {
                el.classList.remove("not-parent");
            }
        });
        //setTimeout(() => {
        //    document.querySelectorAll(".sidebar-overlay").forEach(el => {
        //        if (el && el.parentNode) {
        //            el.parentNode.removeChild(el);
        //        }
        //    });
        //    document.querySelectorAll(".mega-menu.not-parent").forEach(el => {
        //        if (el && el.classList) {
        //            el.classList.remove("not-parent");
        //        }
        //    });
        //}, 50); // delay 50ms giúp DOM ổn định trước khi remove
    },
    toggleBodyClass: function (isShow) {
        const body = document.body;
        if (!body) return;

        if (isShow) {
            body.classList.add("show");
        } else {
            body.classList.remove("show");
        }
    },
};
