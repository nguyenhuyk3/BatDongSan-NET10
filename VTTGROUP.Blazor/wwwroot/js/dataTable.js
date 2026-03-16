function applyStickyColumnsDynamic(tableId, config) {
    const table = document.getElementById(tableId);
    if (!table || !config || !Array.isArray(config)) return;

    const theadRows = table.querySelectorAll("thead tr");
    const ths = table.querySelector("thead tr")?.children;
    const rows = table.querySelectorAll("tbody tr");
    if (!ths || rows.length === 0) return;

    const totalCols = Array.from(ths).reduce((sum, th) => sum + (parseInt(th.colSpan || 1)), 0);

    // B1. Lấy width từng cột
    const colWidths = [];
    let totalWidth = 0;
    for (let i = 0; i < ths.length; i++) {
        const width = parseInt(ths[i].style.width) || ths[i].offsetWidth || 100;
        const span = parseInt(ths[i].colSpan || 1);
        for (let s = 0; s < span; s++) {
            colWidths.push(width);
            totalWidth += width;
        }
        ths[i].style.minWidth = width + "px";
    }

    table.style.minWidth = totalWidth + "px";

    // B2. Build columnConfig từ config truyền vào
    const columnConfig = [];
    const left = config.find(c => c.left != null)?.left ?? 0;
    const right = config.find(c => c.right != null)?.right ?? 0;

    for (let i = 0; i < left; i++) {
        if (i < totalCols) columnConfig.push({ index: i, position: "left" });
    }
    for (let i = 0; i < right; i++) {
        const idx = totalCols - 1 - i;
        if (idx >= 0) columnConfig.push({ index: idx, position: "right" });
    }

    if (columnConfig.length === 0) return;

    const leftCols = columnConfig.filter(c => c.position === "left").sort((a, b) => a.index - b.index);
    const rightCols = columnConfig.filter(c => c.position === "right").sort((a, b) => b.index - a.index); // quan trọng: sort giảm dần

    // B3. Tính left/right offsets
    const leftOffsets = {};
    let accLeft = 0;
    for (const c of leftCols) {
        leftOffsets[c.index] = accLeft;
        accLeft += colWidths[c.index] || 0;
    }

    const rightOffsets = {};
    let accRight = 0;
    for (const c of rightCols) {
        rightOffsets[c.index] = accRight;
        accRight += colWidths[c.index] || 0;
    }

    const lastLeftIdx = leftCols.at(-1)?.index;
    const firstRightIdx = rightCols.at(0)?.index;

    // B4. Áp dụng sticky cho tbody
    const applyStickyToRows = (rows) => {
        rows.forEach(row => {
            const cells = Array.from(row.children);
            let logicCol = 0;

            for (let i = 0; i < cells.length; i++) {
                const cell = cells[i];
                const span = parseInt(cell.colSpan || 1);

                for (let offset = 0; offset < span; offset++) {
                    const curIdx = logicCol + offset;

                    // Sticky LEFT
                    if (leftOffsets[curIdx] != null) {
                        cell.style.position = "sticky";
                        cell.style.left = leftOffsets[curIdx] + "px";
                        cell.style.zIndex = "1";
                        cell.classList.add("sticky-left");
                        if (curIdx === lastLeftIdx) {
                            cell.classList.add("sticky-left-last");
                        }
                    }

                    // Sticky RIGHT
                    if (rightOffsets[curIdx] != null) {
                        cell.style.position = "sticky";
                        cell.style.right = rightOffsets[curIdx] + "px";
                        cell.style.zIndex = "1";
                        cell.classList.add("sticky-right");
                        if (curIdx === firstRightIdx) {
                            cell.classList.add("sticky-right-last");
                        }
                    }
                }

                logicCol += span;
            }
        });
    };

    // B5. Apply sticky cho cả thead và tbody
    applyStickyToRows(theadRows);
    applyStickyToRows(rows);
    enableRowSelection(tableId)
}

function enableRowSelection(tableId) {
    const table = document.getElementById(tableId);
    if (!table) return;

    const rows = table.querySelectorAll("tbody tr");
    rows.forEach(row => {
        row.addEventListener("click", () => {
            rows.forEach(r => r.classList.remove("active-row")); // Bỏ active cũ
            row.classList.add("active-row"); // Gán active dòng hiện tại
        });
    });
}

window.autoScaleSoDoCanHo = () => {
    const header = document.querySelector("header.header-layout");
    const title = document.querySelector("h1.legend-title");
    const filter = document.querySelector(".container-search-home");
    const table = document.querySelector(".floor-table");
    const trangThai = document.querySelector(".trangthai-canho");
    let topHeight = 0;
    let tt = 0;
    if (trangThai) {
        tt = trangThai.offsetHeight + 5;
    }

    if (header) {
        topHeight += header.offsetHeight;
    }
    if (title) {
        topHeight += title.offsetHeight;
    }
    if (filter) {
        topHeight += filter.offsetHeight;
    }
    if (header) {
        topHeight += header.offsetHeight;
    }

    if (!table) return;

    topHeight =
        topHeight +
        tt +
        150; // thêm khoảng đệm

    const screenHeight = window.innerHeight;
    const availableHeight = screenHeight - topHeight;

    const tableHeight = table.offsetHeight;

    //if (tableHeight > availableHeight) {
    //    const scale = availableHeight / tableHeight;
    //    table.style.transformOrigin = "top left";
    //    table.style.transform = `scaleY(${scale})`;
    //} else {
    //    table.style.transform = ""; // reset nếu không cần scale
    //}
    const zoom = Math.min(availableHeight / tableHeight, 1);
    table.style.zoom = zoom;

    if (window.customZoomSlider?.dotNetRef) {
        const zoomPercent = Math.round(zoom * 100);
        window.customZoomSlider.dotNetRef.invokeMethodAsync("OnZoomChangedFromJs", zoomPercent);
    }
};
window.customZoomSlider = {
    dotNetRef: null,
    min: 10,
    max: 200,
    value: 100,
    thumb: null,
    valueDisplay: null,

    init: function (elementId, dotNetRef, min, max, initialValue) {
        this.dotNetRef = dotNetRef;
        this.min = min;
        this.max = max;
        this.value = initialValue;

        const container = document.getElementById(elementId);
        const track = container.querySelector('.custom-slider-track');
        this.thumb = container.querySelector('.custom-slider-thumb');
        this.marker = container.querySelector('.custom-slider-marker');
        this.valueDisplay = container.querySelector('.custom-slider-value');
        const btnMinus = container.querySelector('.btn-minus');
        const btnPlus = container.querySelector('.btn-plus');

        const setValue = (newValue, notify = true) => {
            newValue = Math.min(this.max, Math.max(this.min, newValue));
            if (this.value !== newValue) {
                this.value = newValue;
                this.updateUI();
                if (notify) this.dotNetRef.invokeMethodAsync("OnZoomChangedFromJs", this.value);
            }
        };

        const getNextStep = (val, dir) => {
            const remainder = val % 10;
            const base = val - remainder;
            if (dir === 'up') {
                return remainder === 0 ? base + 10 : base + 10;
            } else {
                return remainder === 0 ? base - 10 : base;
            }
        };

        let isDragging = false;

        const onMouseMove = (e) => {
            if (!isDragging) return;
            const rect = track.getBoundingClientRect();
            const percent = (e.clientX - rect.left) / rect.width;
            const newVal = Math.round(this.min + percent * (this.max - this.min));
            setValue(newVal);
        };

        track.addEventListener('mousedown', e => {
            isDragging = true;
            onMouseMove(e);
        });

        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup', () => isDragging = false);

        this.thumb.addEventListener('mousedown', e => {
            e.stopPropagation();
            isDragging = true;
        });

        btnMinus.addEventListener('click', () => {
            const newVal = getNextStep(this.value, 'down');
            setValue(newVal);
        });

        btnPlus.addEventListener('click', () => {
            const newVal = getNextStep(this.value, 'up');
            setValue(newVal);
        });

        this.marker.addEventListener('click', () => {
            setValue(100);
        });

        // marker ở giữa
        this.marker.style.left = `calc(50% - 1px)`;

        this.updateUI(); // Lần đầu
    },

    updateUI: function () {
        const percent = (this.value - this.min) / (this.max - this.min);
        const leftPercent = percent * 100;
        const extend = 1;

        if (leftPercent <= 1) {
            this.thumb.style.left = `calc(${leftPercent}% - ${extend}px)`;
        } else if (leftPercent >= 99) {
            this.thumb.style.left = `calc(${leftPercent}% - ${extend}px)`;
        } else if (Math.abs(leftPercent - 50) <= 3) {
            this.thumb.style.left = `calc(${leftPercent}% + ${extend}px)`;
        } else {
            this.thumb.style.left = `${leftPercent}%`;
        }

        this.valueDisplay.textContent = `${this.value}%`;
    },

    setTableZoom: function (value) {
        const zoomFactor = value / 100;
        const table = document.querySelector("table.floor-table");
        if (table) {
            table.style.zoom = zoomFactor;
        }
        this.value = value;
        this.updateUI(); // cập nhật lại thumb + text
    }
};
//window.customZoomSlider = {
//    dotNetRef: null,
//    init: function (elementId, dotNetRef, min, max, initialValue) {
//        this.dotNetRef = dotNetRef;
//        const container = document.getElementById(elementId);
//        const track = container.querySelector('.custom-slider-track');
//        const thumb = container.querySelector('.custom-slider-thumb');
//        const marker = container.querySelector('.custom-slider-marker');
//        const btnMinus = container.querySelector('.btn-minus');
//        const btnPlus = container.querySelector('.btn-plus');
//        const valueDisplay = container.querySelector('.custom-slider-value');

//        let value = initialValue;

//        const updateUI = () => {
//            const percent = (value - min) / (max - min);
//            const leftPercent = percent * 100;
//            const extend = 1;
//            if (leftPercent <= 1) {
//                thumb.style.left = `calc(${leftPercent}% - ${extend}px)`;
//            }
//            else if (leftPercent >= 99) {
//                thumb.style.left = `calc(${leftPercent}% - ${extend}px)`;
//            }
//            else if (Math.abs(leftPercent - 50) <= 3) {
//                thumb.style.left = `calc(${leftPercent}% + ${extend}px)`;
//            }
//            else {
//                thumb.style.left = `${leftPercent}%`;
//            }


//            valueDisplay.textContent = `${value}%`;
//        };

//        const setValue = (newValue, notify = true) => {
//            newValue = Math.min(max, Math.max(min, newValue));
//            if (value !== newValue) {
//                value = newValue;
//                updateUI();
//                if (notify) dotNetRef.invokeMethodAsync("OnZoomChangedFromJs", value);
//            }
//        };

//        const getNextStep = (val, dir) => {
//            const remainder = val % 10;
//            const base = val - remainder;

//            if (dir === 'up') {
//                return remainder === 0 ? base + 10 : base + 10;
//            } else {
//                return remainder === 0 ? base - 10 : base;
//            }
//        };

//        let isDragging = false;

//        const onMouseMove = (e) => {
//            if (!isDragging) return;
//            const rect = track.getBoundingClientRect();
//            const percent = (e.clientX - rect.left) / rect.width;
//            const newVal = Math.round(min + percent * (max - min));
//            setValue(newVal);
//        };

//        track.addEventListener('mousedown', e => {
//            isDragging = true;
//            onMouseMove(e);
//        });

//        document.addEventListener('mousemove', onMouseMove);
//        document.addEventListener('mouseup', () => isDragging = false);

//        thumb.addEventListener('mousedown', e => {
//            e.stopPropagation();
//            isDragging = true;
//        });

//        btnMinus.addEventListener('click', () => {
//            const newVal = getNextStep(value, 'down');
//            setValue(newVal);
//        });

//        btnPlus.addEventListener('click', () => {
//            const newVal = getNextStep(value, 'up');
//            setValue(newVal);
//        });

//        marker.addEventListener('click', () => {
//            setValue(100);
//        });

//        // Center the 100% marker
//        const markerPercent = (100 - min) / (max - min) * 100;
//        marker.style.left = `calc(${50}% - 1px)`;

//        updateUI();
//    },
//    setTableZoom: function (value) {
//        const zoomFactor = value / 100;
//        const table = document.querySelector("table.floor-table");
//        if (table) {
//            table.style.zoom = zoomFactor;
//            customZoomSlider.value = value;
//            customZoomSlider.updateUI();
//        }
//    }
//};

//window.addEventListener("resize", () => {
//    window.autoScaleSoDoCanHo();
//});
window.contextHelper = {
    registerRightClick: function (elementId, dotnetObj, type, dataJson) {
        document.getElementById(elementId)?.addEventListener('contextmenu', function (e) {
            e.preventDefault();
            dotnetObj.invokeMethodAsync("OnRightClick", {
                x: e.clientX,
                y: e.clientY,
                type: type,
                data: dataJson
            });
        });
    }
};

window.localGioHangHelper = {
    addSanPhamToLocal: function (dataJson) {
        const data = JSON.parse(dataJson);
        const key = "GioHang_" + data.MaGioHang + "_" + data.MaSanPham;

        if (!localStorage.getItem(key)) {
            localStorage.setItem(key, JSON.stringify(data));
        }

        localStorage.setItem("LastSelectedMaSanPham", data.MaSanPham);
        return true;
    },
    getSanPhamFromLocal: function (maSanPham, maGioHang) {
        const key = "GioHang_" + maGioHang + "_" + maSanPham;
        const value = localStorage.getItem(key);
        return value ?? null;
    },
    removeSanPhamFromLocal: function (maSanPham, maGioHang) {
        const key = "GioHang_" + maGioHang + "_" + maSanPham;
        localStorage.removeItem(key);
    },
    //Truyền đội tượng là Model
    addSoDoCanHoToLocal: function (dataJson) {
        try {
            const data = JSON.parse(dataJson || "{}");
            const maGioHang = (data.MaGioHang || "").trim();
            const maSanPham = (data.MaSanPham || "").trim();

            if (!maGioHang || !maSanPham) return false;

            const key = "SoDoCanHo_" + maGioHang + "_" + maSanPham;

            // Lưu nguyên object SoDoCanHoModel
            localStorage.setItem(key, JSON.stringify(data));

            // Lưu “last selected” để form tab mới đọc đúng item
            localStorage.setItem(
                "LastSelectedSoDoCanHo",
                JSON.stringify({
                    MaGioHang: maGioHang,
                    MaSanPham: maSanPham,
                    SavedAt: new Date().toISOString()
                })
            );

            return true;
        } catch (e) {
            console.error("addSoDoCanHoToLocal error:", e);
            return false;
        }
    },

    getLastSelected: function () {
        return localStorage.getItem("LastSelectedSoDoCanHo") ?? null;
    },

    getSoDoCanHoFromLocal: function (maGioHang, maSanPham) {
        const key = "SoDoCanHo_" + maGioHang + "_" + maSanPham;
        return localStorage.getItem(key) ?? null;
    },

    removeSoDoCanHoFromLocal: function (maGioHang, maSanPham) {
        const key = "SoDoCanHo_" + maGioHang + "_" + maSanPham;
        localStorage.removeItem(key);
    },

    clearLastSelected: function () {
        localStorage.removeItem("LastSelectedSoDoCanHo");
    }

};

window.soDoCanHo = (function () {

    let currentTruc = null;
    let currentTang = null;

    function clearHighlight() {
        document.querySelectorAll('.hl-tang, .hl-truc').forEach(function (el) {
            el.classList.remove('hl-tang');
            el.classList.remove('hl-truc');
        });
        currentTruc = null;
        currentTang = null;
    }

    function highlight(maTruc, tenTang, block) {
        if (maTruc === currentTruc && tenTang === currentTang) return;
        clearHighlight();

        if (tenTang) {
            var classTang = block + '-' + tenTang;
            document.querySelectorAll('.' + classTang).forEach(function (el) {
                el.classList.add('hl-tang');
            });
        }

        if (maTruc) {
            var classTruc = block + '-' + maTruc;
            document.querySelectorAll('.' + classTruc).forEach(function (el) {
                el.classList.add('hl-truc');
            });
        }

        currentTruc = maTruc;
        currentTang = tenTang;
    }

    function init(containerSelector) {
        const container = document.querySelector(containerSelector || 'body');
        if (!container) return;

        const apartments = container.querySelectorAll('.apartment');
        apartments.forEach(ap => {
            ap.addEventListener('mouseenter', () => {
                const maTruc = ap.getAttribute('id-truc');
                const tenTang = ap.getAttribute('id-tang');
                const block = ap.getAttribute('id-block');
                highlight(maTruc, tenTang, block);
            });

            ap.addEventListener('mouseleave', () => {
                clearHighlight();
            });
        });
    }

    return {
        init,
        highlight,
        clearHighlight
    };

})();
