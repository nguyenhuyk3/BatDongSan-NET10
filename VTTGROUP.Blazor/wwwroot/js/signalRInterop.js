window.signalRInterop = {
    connection: null,
    dotNetRef: null,

    init: function (dotNetHelper) {
        this.dotNetRef = dotNetHelper;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationhub", {
                accessTokenFactory: () => {
                    return localStorage.getItem("accessToken"); // đảm bảo đã có token trong localStorage
                }
            })
            .build();

        this.connection.on("ReceiveMessage", function (user, message) {

            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('ReloadListCongViec', message);
            }

            if (typeof showToast === "function") {
                showToast("Thông báo", message, "info");
            }
        });

        this.connection.on("ForceLogout", () => {
            try {
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync("OnForceLogout");
                }
            } catch (_) { }
        });

        this.connection.on("TinhTrangCanHoUpdated", function () {
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('ReloadCanHoList');
            }
            //window.tableRowHelper?.markLater('.floor-table', 50);
        });

        this.connection.on("DangKyCountdowns", function (payload) {
            if (window.countdownHelper) {
                window.countdownHelper.upsertMany(payload);
            }
            //window.tableRowHelper?.markLater('.floor-table', 0);
        });

        this.connection.start().catch(function (err) {
        });
    }
};
window.countdownHelper = (function () {
    const map = new Map(); // key: MaCanHo -> {expireAtMs}
    const elCache = new Map();
    let tickTimer = null;
    let serverOffsetMs = 0; // client-now - server-now

    function getEls(key) {
        let hit = elCache.get(key);
        if (hit && document.body.contains(hit.span) && document.body.contains(hit.container)) return hit;

        const span = document.querySelector(`[data-canho="${CSS.escape(key)}"]`);
        const container = span ? span.closest('.count-down-ch') : null; // ⭐ quan trọng
        if (!span || !container) return null;

        hit = { span, container };
        elCache.set(key, hit);
        return hit;
    }

    function setStateClass(container, left) {
        container.classList.remove("cd-danger", "cd-warning", "cd-ok", "cd-danger-fade");
        if (left <= 15) container.classList.add("cd-danger-fade");
        if (left <= 60) container.classList.add("cd-danger");
        else if (left <= 180) container.classList.add("cd-warning");
        else container.classList.add("cd-ok");
    }

    function startTick() {
        if (tickTimer) return;
        tickTimer = setInterval(() => {
            const nowMs = Date.now() - serverOffsetMs;
            map.forEach((st, key) => {
                const leftSec = Math.max(0, Math.floor((st.expireAtMs - nowMs) / 1000));
                const els = getEls(key);
                if (!els) return;

                if (leftSec === st.lastShownSec) return; // không đổi -> khỏi đụng DOM

                if (leftSec <= 0) {
                    els.span.textContent = "";
                    els.container.classList.remove("cd-danger", "cd-warning", "cd-ok", "cd-danger-fade");
                    els.container.style.display = "none";
                    //els.container.remove();
                    map.delete(key);
                    //window.tableRowHelper?.markLater('.floor-table', 0);
                    return;
                }

                const mm = String(Math.floor(leftSec / 60)).padStart(2, "0");
                const ss = String(leftSec % 60).padStart(2, "0");
                els.span.textContent = `${mm}:${ss}`;
                setStateClass(els.container, leftSec);
                st.lastShownSec = leftSec;
            });
        }, 1000);
    }

    function upsertMany(payload) {
        // Re-sync offset mỗi lần server broadcast
        const serverNowMs = Date.parse(payload.serverUtcNow);
        serverOffsetMs = Date.now() - serverNowMs;

        (payload.items || []).forEach(i => {
            const key = i.maCanHo || i.MaCanHo;
            const exp = Date.parse(i.expireAtUtc || i.ExpireAtUtc);
            const st = map.get(key);
            if (st) { st.expireAtMs = exp; return; }
            map.set(key, { expireAtMs: exp, lastShownSec: -1 });
        });

        startTick();
    }

    return { upsertMany };
})();
window.tableRowHelper = {
    markRowsWithCountdown(rootSelector) {
        const root = rootSelector ? document.querySelector(rootSelector) : document;
        if (!root) return;
        //root.querySelectorAll('table.floor-table tr').forEach(tr => {
        //    const has = tr.querySelector('.count-down-ch'); // có badge countdown trong bất kỳ td nào
        //    tr.classList.toggle('row-has-countdown', !!has);
        //});
    },
    markLater(rootSelector, delay) {
        setTimeout(() => window.tableRowHelper.markRowsWithCountdown(rootSelector), delay || 0);
    }
};

//window.signalRInterop = {
//    connection: null,
//    refs: {},

//    init: function () {
//        if (this.connection) return; // đã init rồi thì thôi

//        this.connection = new signalR.HubConnectionBuilder()
//            .withUrl("/notificationhub", {
//                accessTokenFactory: () => localStorage.getItem("accessToken") || ""
//            })
//            .withAutomaticReconnect()
//            .build();

//        // === Handlers từ server ===
//        this.connection.on("ReceiveMessage", (user, message) => {
//            // Thông báo/công việc -> gọi vào MainLayout
//            this.invokeOn("main", "ReloadListCongViec", message);
//            if (typeof showToast === "function") showToast("Thông báo", message, "info");
//        });

//        this.connection.on("ForceLogout", () => {
//            this.invokeOn("main", "OnForceLogout");
//        });

//        this.connection.on("TinhTrangCanHoUpdated", () => {
//            // Reload sơ đồ căn hộ ở trang giỏ hàng nhóm (đổi key nếu bạn dùng tên khác)
//            this.invokeOn("giohanggroup", "ReloadCanHoList");
//            // nếu còn trang khác cũng cần reload:
//            // this.invokeOn("giohang", "ReloadCanHoList");
//        });

//        this.connection.start().catch(() => { /* swallow */ });
//    },

//    // Đăng ký/huỷ đăng ký ref theo key
//    register: function (key, dotNetHelper) {
//        this.refs[key] = dotNetHelper;
//    },
//    unregister: function (key) {
//        delete this.refs[key];
//    },

//    // Gọi sang .NET instance theo key
//    invokeOn: function (key, method, ...args) {
//        const ref = this.refs[key];
//        if (ref) return ref.invokeMethodAsync(method, ...args);
//    }
//};
