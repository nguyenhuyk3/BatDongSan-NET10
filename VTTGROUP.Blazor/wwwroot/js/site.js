window.blazorSetCookie = (name, value, minutes) => {
    const expires = new Date(Date.now() + minutes * 60 * 1000).toUTCString();
    document.cookie = `${name}=${value}; expires=${expires}; path=/`;
};

window.blazorGetCookie = (name) => {
    const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    return match ? match[2] : null;
};

window.select2Helper = {
    debounceTimers: {},
    init: function (id, allowClear) {
        $('#' + id).select2();
    },
    initWithPlaceholder: function (id, placeholderText, allowClear) {
        $('#' + id).select2({
            placeholder: placeholderText,
            allowClear: allowClear,
            language: {
                noResults: () => 'Không có kết quả',
            }
        });
    },
    initSelectChange: function (id, placeholderText, allowClear) {
        const el = $("#" + id);
        const opts = {
            placeholder: placeholderText,
            allowClear: allowClear,
            language: {
                noResults: () => 'Không có kết quả',
            },
        };

        const $parent = $("#popupFilter");
        if ($parent.length) {
            opts.dropdownParent = $parent;
        }

        el.select2(opts);
        let internalChange = false;

        el.on('change', function () {
            if (internalChange) return;
            internalChange = true;
            el[0].dispatchEvent(new Event('change'));
            internalChange = false;
        });

        el.on('select2:open', function () {
            const input = document.querySelector(
                '.select2-container--open .select2-search__field'
            );
            if (input) input.placeholder = 'Tìm nhanh...';
        });
    },
    initSelectMultipleChange: function (id, placeholderText, allowClear, dotNetHelper) {
        const el = $("#" + id);
        const summaryThreshold = 3;
        el.off('.s2bridge');
        if (el.hasClass('select2-hidden-accessible')) {
            el.select2('destroy');
        }

        el.select2({
            placeholder: placeholderText,
            allowClear: allowClear,
            closeOnSelect: false
        });

        const updateRenderedSummary = () => {
            if (summaryThreshold == null) return;
            const data = el.select2('data') || [];
            const count = data.length;

            const $render = el.next('.select2').find('.select2-selection__rendered');
            $render.find('.s2-summary').remove();

            if (count > summaryThreshold) {
                // xoá các chip mặc định
                $render.find('.select2-selection__choice').remove();
                // thêm 1 chip tóm tắt
                $('<li>', {
                    class: 'select2-selection__choice s2-summary',
                    text: `Đã chọn ${count} giá trị`,
                    title: data.map(d => d.text).join(', ')
                }).appendTo($render);
            }
        };

        let debounceTimer = null;

        const fire = () => {
            const selectedValues = el.val() || []; // clear => null
            dotNetHelper.invokeMethodAsync('OnSelectMultiChanged', id, selectedValues);
        };

        el.on('change.s2bridge', function () {
            updateRenderedSummary();
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(fire, 250); // 200–300ms tuỳ bạn
        });

        el.on('select2:clear.s2bridge', function () {
            updateRenderedSummary();
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(fire, 0);
        });

        updateRenderedSummary();

    },
    //destroy: function (id) {
    //    const el = $("#" + id);
    //    el.off('.s2bridge');
    //    if (el.hasClass('select2-hidden-accessible')) {
    //        el.select2('destroy');
    //    }
    //},
    destroy: function (selector) {
        const el = $(selector);
        if (el.data('select2')) el.select2('destroy');
    },
    clear: function (id) {
        const el = $('#' + id);
        el.val(null).trigger('change');
    },
    resetAndInit: function (id, placeholderText, allowClear, dotNetHelper) {
        const el = $("#" + id);
        if (el.hasClass("select2-hidden-accessible")) {
            el.select2("destroy");
        }
        el.select2({
            placeholder: placeholderText,
            allowClear: allowClear,
            width: 'resolve'
        });
        el.val(null).trigger('change');
        el.on('change', function () {
            const selectedValues = $(this).val();
            dotNetHelper.invokeMethodAsync('OnSelectMultiChanged', id, selectedValues);
        });
    },
    resetMultipleSelects: function (ids) {
        // Nếu truyền nhầm kiểu string thì ép lại thành mảng
        if (typeof ids === 'string') {
            ids = [ids];
        }

        if (!Array.isArray(ids)) {
            console.warn("resetMultipleSelects: input is not array", ids);
            return;
        }

        ids.forEach(function (id) {
            const el = $("#" + id);
            if (el.length === 0 || !el.is(":visible")) return;
            if (el.data('select2')) {
                el.val(null).trigger('change');
            } else {
                el.val('');
            }
        });
    },
    bindSelectChange: function (selectId, dotNetObject, methodName, debounceMs = 300) {
        const select = document.getElementById(selectId);
        if (!select) {
            return;
        }
        const self = this;
        select.addEventListener("change", function () {
            const value = select.value;

            clearTimeout(self.debounceTimers[selectId]);
            self.debounceTimers[selectId] = setTimeout(() => {
                dotNetObject.invokeMethodAsync(methodName, value);
            }, debounceMs);
        });
    },
    setValue: function (elementId, value) {
        const el = document.getElementById(elementId);
        if (!el) return;
        $(el).val(value).trigger('change');
    },
    getValue: function (id) {
        const el = document.getElementById(id);
        return el ? el.value : null;
    },
    resetSelect2: function (id, placeholderText = '', allowClear = true) {
        const el = $('#' + id);
        if (el.length === 0) return;

        // 👇 Destroy cũ nếu đã init
        if (el.hasClass("select2-hidden-accessible")) {
            el.select2('destroy');
        }

        // 👇 Reset value
        el.val(null).trigger('change');

        // 👇 Re-init Select2 đúng chuẩn với placeholder và width
        el.select2({
            placeholder: placeholderText,
            allowClear: allowClear,
            width: '100%'
        });
    },
    initMany: function (configs) {
        configs.forEach(c => {
            const el = $("#" + c.id);
            if (el.length === 0) return;

            const opts = {
                placeholder: c.placeholder,
                allowClear: c.allowClear ?? true,
                width: '100%'
            };

            const $modalParent = el.closest('.modal');
            if ($modalParent.length) {
                opts.dropdownParent = $modalParent;
            }

            if (el.hasClass('select2-hidden-accessible')) {
                el.select2('destroy');
            }

            el.select2(opts);

            let internalChange = false;
            el.off('change.s2many').on('change.s2many', function () {
                if (internalChange) return;
                internalChange = true;
                el[0].dispatchEvent(new Event('change'));
                internalChange = false;
            });

            el.off('select2:open.s2many').on('select2:open.s2many', function () {
                const input = document.querySelector('.select2-container--open .select2-search__field');
                if (input) {
                    input.placeholder = 'Tìm nhanh...';
                    input.focus();
                }
            });

            if (typeof c.value === 'string' && c.value.trim() !== '') {
                el.val(c.value).trigger('change');
            }
        });
    },
    initBySelector: function (selector, placeholderText, allowClear) {
        const $items = $(selector);
        if (!$items.length) return;

        const opts = {
            placeholder: placeholderText,
            allowClear: allowClear,
            language: {
                noResults: () => 'Không có kết quả',
            }
        };

        const $parent = $("#popupFilter");
        if ($parent.length) {
            opts.dropdownParent = $parent;
        }

        $items.each(function () {
            const el = $(this);

            // Destroy cũ nếu đã init
            if (el.hasClass('select2-hidden-accessible')) {
                el.select2('destroy');
            }

            el.select2(opts);

            let internalChange = false;
            el.off('change.s2single').on('change.s2single', function () {
                if (internalChange) return;
                internalChange = true;
                el[0].dispatchEvent(new Event('change'));
                internalChange = false;
            });

            el.off('select2:open.s2single').on('select2:open.s2single', function () {
                const input = document.querySelector(
                    '.select2-container--open .select2-search__field'
                );
                if (input) input.placeholder = 'Tìm nhanh...';
            });

            // ❌ KHÔNG dùng el.data('value') nữa (bị cache)
            // ✅ Đọc trực tiếp từ attribute:
            let currentValue = el.attr('data-value');

            // Phòng trường hợp Blazor render "null"/"undefined"
            if (currentValue === undefined || currentValue === null ||
                currentValue === '' || currentValue === 'null' || currentValue === 'undefined') {
                // Dòng mới -> clear luôn
                el.val(null).trigger('change.select2');
            } else {
                el.val(currentValue).trigger('change.select2');
            }
        });
    }
};
window.tabHelper = {
    initTabs: function () {
        document.querySelectorAll(".tab-element").forEach(tabContainer => {
            const tabs = tabContainer.querySelectorAll(".tab-item");
            const line = tabContainer.querySelector(".line-bot-nav");
            const parent = tabContainer.closest(".container-tabs");
            const contents = parent.querySelectorAll(".tab-content-item");

            tabs.forEach((tab, index) => {
                tab.addEventListener("click", function () {
                    const id = tab.getAttribute("data-id");
                    let isValid = true;
                    //Check dữ liệu các tab trước nó
                    for (let i = 0; i < index; i++) {
                        const contentId = tabs[i].getAttribute("data-id");
                        const content = parent.querySelector(`.tab-content-item[data-id="${contentId}"]`);
                        if (content) {
                            const $form = $(content);
                            $form.find('.input-validation').each(function () {
                                const $input = $(this);
                                const value = $input.val().trim();
                                const $errorSpan = $input.parent().find('.error-validation');

                                let isNumber = false;
                                if ($input.hasClass('is-numberic')) {
                                    isNumber = true;
                                }

                                if (!value || (isNumber && value == "0")) {
                                    $input.addClass('validate');
                                    $input.closest('.form-group-input').addClass('has-validate');
                                    $errorSpan.show();
                                    isValid = false;
                                } else {
                                    $input.removeClass('validate');
                                    $input.closest('.form-group-input').removeClass('has-validate');
                                    $errorSpan.hide();
                                }
                            });
                        }
                    }

                    if (!isValid) {
                        $('.container-mess-error').show();
                        $('#mess-error').text("Có trường dữ liệu không chính xác hoặc không hợp lệ");
                        return;
                    }
                    // Remove class actived-tab khỏi tất cả tab & content
                    tabs.forEach(t => t.classList.remove("actived-tab"));
                    contents.forEach(c => c.classList.remove("actived-tab"));

                    // Add class actived-tab vào tab và content được chọn
                    tab.classList.add("actived-tab");
                    parent.querySelector(`.tab-content-item[data-id="${id}"]`)?.classList.add("actived-tab");

                    // Di chuyển line-bot-nav nếu có
                    if (line) {
                        line.style.width = `${tab.offsetWidth}px`;
                        line.style.left = `${tab.offsetLeft}px`;
                    }
                });
            });

            // Gọi mặc định lần đầu để set line
            const firstTab = tabContainer.querySelector(".tab-item.actived-tab");
            if (firstTab && line) {
                line.style.width = `${firstTab.offsetWidth}px`;
                line.style.left = `${firstTab.offsetLeft}px`;
            }
        });
    }
};

window.bootstrapModal = {
    show: function (selector) {
        const modal = new bootstrap.Modal(document.querySelector(selector));
        modal.show();
    },
    hide: function (selector) {
        const el = document.querySelector(selector);
        const modal = bootstrap.Modal.getInstance(el);
        modal?.hide();
    }
};

window.registerClickOutside = (elementId, dotNetHelper) => {
    const handler = (event) => {
        const el = document.getElementById(elementId);
        if (el && !el.contains(event.target)) {
            dotNetHelper.invokeMethodAsync("CloseDropdown");
        }
    };
    document.addEventListener("mousedown", handler);

    // Return a disposer function
    return {
        dispose: () => document.removeEventListener("mousedown", handler)
    };
};

window.registerAllClickOutside = (elementId, dotNetHelper) => {
    const handler = (event) => {
        const el = document.getElementById(elementId);
        if (el && !el.contains(event.target)) {
            dotNetHelper.invokeMethodAsync("CloseAllDropdowns");
        }
    };
    document.addEventListener("mousedown", handler);
};

let outsideClickHandler = null;
window.popupMoreFilter = {
    initClickOutside: function (wrapperRef, dotnetObj) {
        if (outsideClickHandler) {
            document.removeEventListener('click', outsideClickHandler);
        }

        outsideClickHandler = function (e) {
            if (!wrapperRef || !dotnetObj) return;

            const popup = wrapperRef.querySelector('.content-search-more');
            const toggleBtn = wrapperRef.querySelector('.btn-toggle-popup-more');

            if (
                popup &&
                !popup.contains(e.target) &&
                (!toggleBtn || !toggleBtn.contains(e.target))
            ) {
                dotnetObj.invokeMethodAsync('HidePopupMore');
            }
        };

        document.addEventListener('click', outsideClickHandler);
    },

    removeClickOutside: function () {
        if (outsideClickHandler) {
            document.removeEventListener('click', outsideClickHandler);
            outsideClickHandler = null;
        }
    },
    removeShowClass: function (id) {
        document.getElementById(id)?.classList.remove("show");
    }
};

window.fullscreenHelper = {
    enableFullScreen: function (elementId) {
        const className = 'full-screen';
        let el = document.getElementById(elementId);
        if (el) {
            el.classList.add(className);
            const handler = function (e) {
                if (e.key === "Escape") {
                    el.classList.remove(className);
                    document.removeEventListener("keydown", handler);
                }
            };
            document.addEventListener("keydown", handler);
        }
    }
};

window.setSmartMinHeightPrecise = function (targetId, rootSelector = "body", extraHeight = 0) {
    const target = document.getElementById(targetId);
    const root = document.querySelector(rootSelector);
    if (!target || !root || !root.contains(target)) return;

    const targetOffset = target.offsetTop;
    const rootOffset = root.offsetTop;
    const spaceUsedAbove = targetOffset - rootOffset;

    const rootHeight = root.clientHeight || window.innerHeight;
    const remaining = Math.max(rootHeight - spaceUsedAbove - extraHeight, 0);

    //target.style.minHeight = `${remaining}px`;
    target.style.setProperty("max-height", `${remaining}px`, "important");
};
window.setSmartMaxHeightDetail = function (targetId, extra = 0) {
    const target = document.getElementById(targetId);
    if (!target) return;

    const targetTop = target.getBoundingClientRect().top;
    const windowHeight = window.innerHeight;

    const button = document.querySelector('.container-button');
    const buttonHeight = button?.offsetHeight || 0;

    const remaining = Math.max(windowHeight - targetTop - buttonHeight - extra, 0);
    target.style.setProperty("max-height", `${remaining}px`, "important");

};

//window.tooltipHelper = {
//    init: function () {
//        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
//        tooltipTriggerList.forEach(function (tooltipTriggerEl) {
//            new bootstrap.Tooltip(tooltipTriggerEl);
//        });
//    }
//};
window.tooltipHelper = {
    init: function (selector = '[data-bs-toggle="tooltip"]') {
        document.querySelectorAll(selector).forEach(el => {
            if (bootstrap.Tooltip.getInstance(el)) return;

            new bootstrap.Tooltip(el, {
                container: 'body',
                trigger: 'hover',
                boundary: 'window',
                offset: [0, 10],
                delay: { show: 200, hide: 50 }
            });
        });
    },

    refresh: function (selector = '[data-bs-toggle="tooltip"]') {
        document.querySelectorAll(selector).forEach(el => {
            const inst = bootstrap.Tooltip.getInstance(el);
            if (inst) inst.dispose();

            new bootstrap.Tooltip(el, {
                container: 'body',
                trigger: 'hover',
                boundary: 'window',
                offset: [0, 10],
                delay: { show: 200, hide: 50 }
            });
        });
    }
};

window.openInNewTab = function (url) {
    window.open(url, '_blank');
};
window.enableHorizontalScrollDrag = function (elementId) {
    const container = document.getElementById(elementId);
    let isDown = false;
    let startX;
    let scrollLeft;

    container.addEventListener('mousedown', (e) => {
        isDown = true;
        startX = e.pageX - container.offsetLeft;
        scrollLeft = container.scrollLeft;
    });

    container.addEventListener('mouseleave', () => {
        isDown = false;
    });

    container.addEventListener('mouseup', () => {
        isDown = false;
    });

    container.addEventListener('mousemove', (e) => {
        if (!isDown) return;
        e.preventDefault();
        const x = e.pageX - container.offsetLeft;
        const walk = (x - startX) * 1; // tốc độ kéo
        container.scrollLeft = scrollLeft - walk;
    });
}
window.downloadFileFromBytes = (fileName, bytes) => {
    const blob = new Blob([new Uint8Array(bytes)], { type: "application/zip" });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
};