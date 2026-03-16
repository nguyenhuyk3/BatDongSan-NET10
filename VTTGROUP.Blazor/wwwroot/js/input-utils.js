window.InputUtils = {
    makeNumericInputOnly: function (el, allowDecimal = true) {
        el.addEventListener('keydown', function (e) {
            const allowedKeys = [
                "Backspace", "Tab", "ArrowLeft", "ArrowRight", "Delete", "Home", "End"
            ];

            // Cho số (0-9) hoặc các phím điều hướng
            if (
                allowedKeys.includes(e.key) ||
                (e.key >= '0' && e.key <= '9')
            ) {
                return; // Cho phép
            }

            // Cho dấu chấm nếu allowDecimal
            if (allowDecimal && e.key === '.' && !el.value.includes('.')) {
                return;
            }

            // Nếu không nằm trong các key hợp lệ → ngăn lại
            e.preventDefault();
        });
    },
    makeNumericInputOnlyById: function (id, allowDecimal = true) {
        const el = document.getElementById(id);
        if (el) {
            InputUtils.makeNumericInputOnly(el, allowDecimal);
        }
    },
    makeCurrencyFormat: function (elementId, maxIntegerDigits = 18, decimalPlaces = 2) {
        const input = document.getElementById(elementId);
        if (!input) return;

        input.addEventListener('input', function () {
            const caretPos = input.selectionStart;
            let raw = input.value.replace(/,/g, '');

            // Nếu ký tự đầu là "." thì không cho
            if (raw.startsWith('.')) {
                raw = '';
            }

            // Phân tích phần nguyên và phần thập phân
            let parts = raw.split('.');
            let intPart = parts[0].replace(/\D/g, '').substring(0, maxIntegerDigits);
            let decPart = (parts[1] || '').replace(/\D/g, '').substring(0, decimalPlaces);

            // Nếu không có phần nguyên thì không cho nhập dấu .
            let formatted = '';
            if (intPart.length === 0) {
                formatted = ''; // xoá sạch luôn nếu chưa nhập gì
            } else {
                let formattedInt = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                formatted = formattedInt;
                if (raw.includes('.')) formatted += '.' + decPart;
            }

            // Gán lại giá trị
            const diff = formatted.length - input.value.length;
            input.value = formatted;
            input.setSelectionRange(caretPos + diff, caretPos + diff);
        });
        input.addEventListener('blur', function () {
            let raw = input.value.replace(/,/g, '');
            if (!raw) return;

            let parts = raw.split('.');
            let intPart = parts[0].replace(/\D/g, '');
            let decPart = (parts[1] || '').replace(/\D/g, '');

            // Nếu toàn phần thập phân là 0 → bỏ
            if (decPart.replace(/0/g, '') === '') {
                // Chỉ format phần nguyên lại bằng regex
                let formatted = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                input.value = formatted;
            } else {
                let formattedInt = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                input.value = formattedInt + '.' + decPart;
            }
        });
    },

    makeCurrencyFormatByClass: function (className = 'currency-input', maxIntegerDigits = 18, decimalPlaces = 2) {
        const inputs = document.querySelectorAll(`.${className}`);
        inputs.forEach(input => {
            if (!input) return;

            input.addEventListener('input', function () {
                const caretPos = input.selectionStart;
                let raw = input.value.replace(/,/g, '');

                if (raw.startsWith('.')) {
                    raw = '';
                }

                let parts = raw.split('.');
                let intPart = parts[0].replace(/\D/g, '').substring(0, maxIntegerDigits);
                let decPart = (parts[1] || '').replace(/\D/g, '').substring(0, decimalPlaces);

                let formatted = '';
                if (intPart.length === 0) {
                    formatted = '';
                } else {
                    let formattedInt = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                    formatted = formattedInt;
                    if (raw.includes('.')) formatted += '.' + decPart;
                }

                const diff = formatted.length - input.value.length;
                input.value = formatted;
                input.setSelectionRange(caretPos + diff, caretPos + diff);
            });

            input.addEventListener('blur', function () {
                let raw = input.value.replace(/,/g, '');
                if (!raw) return;

                let parts = raw.split('.');
                let intPart = parts[0].replace(/\D/g, '').substring(0, maxIntegerDigits);
                let decPart = (parts[1] || '').replace(/\D/g, '').substring(0, decimalPlaces);

                let formattedInt = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');

                if (decPart.length > 0) {
                    input.value = formattedInt + '.' + decPart;
                } else {
                    input.value = formattedInt;
                }
            });

            input.addEventListener('click', function (e) {
                this.select();
            });
            input.addEventListener('dblclick', function () {
                this.select();
            });
            input.addEventListener('focus', function () {
                this.select();
            });
        });
    },
    makeCurrencyFormatById : function (id, maxIntegerDigits = 18, decimalPlaces = 2) {
        const input = document.getElementById(id);
        if (!input) return;
        this.attachCurrencyFormatter(input, maxIntegerDigits, decimalPlaces);
    },
    setInputMaPhieu: function (elementId) {
        const input = document.getElementById(elementId);
        if (!input) return;

        input.addEventListener('input', function (e) {
            let newValue = input.value.replace(/[^a-zA-Z0-9]/g, '');
            if (input.value !== newValue) {
                input.value = newValue;
            }
        });
    },

    validateNumericInputWithDecimal: function (e, input) {
        const key = e.key;
        const allowedKeys = ['Backspace', 'Tab', 'ArrowLeft', 'ArrowRight', 'Delete'];

        if (!/[0-9.,]/.test(key) && !allowedKeys.includes(key)) {
            return false;
        }

        const value = input.value;
        const selectionStart = input.selectionStart;
        const selectionEnd = input.selectionEnd;

        const newValue =
            value.substring(0, selectionStart) + key + value.substring(selectionEnd);

        const parts = newValue.replace(',', '.').split('.');

        if (parts.length === 2 && parts[1].length > 4) {
            return false;
        }

        return true;
    },
    restrictToEmailChars: function (id) {
        const input = document.getElementById(id);
        if (!input) return;

        input.addEventListener("keypress", function (e) {
            const char = String.fromCharCode(e.which);
            const regex = /[a-zA-Z0-9@._-]/;

            if (!regex.test(char)) {
                e.preventDefault();
            }
        });
    },
    initFlatpickrById: function (elementId, dotnetHelper, optionsJson) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const options = JSON.parse(optionsJson || "{}");

        options.onReady = function (selectedDates, dateStr, instance) {
            if (options.todayButton) {
                const container_button = document.createElement("div");
                container_button.className = "wraper-flatpickr-today";
                // Tạo nút "Hôm nay"
                const btn = document.createElement("button");
                btn.textContent = "Hôm nay";
                btn.type = "button";
                btn.className = "flatpickr-today-btn";
                btn.onclick = function () {
                    instance.setDate(new Date());
                    instance.close();
                };

                // Thêm vào dưới calendar
                //instance.calendarContainer.appendChild(btn);
                container_button.appendChild(btn);
                instance.rContainer.appendChild(container_button);
            }
        };

        if (dotnetHelper) {
            options.onChange = function (selectedDates, dateStr) {
                dotnetHelper.invokeMethodAsync("OnDateChanged", dateStr);
            };
        }

        flatpickr(element, options);
    },
    initFlatpickrByClass: function (className, dotnetHelper, optionsJson) {
        const elements = document.getElementsByClassName(className);
        if (!elements.length) return;

        const options = JSON.parse(optionsJson || "{}");

        // Định nghĩa onReady chỉ 1 lần
        options.onReady = function (selectedDates, dateStr, instance) {
            if (options.todayButton) {
                const container_button = document.createElement("div");
                container_button.className = "wraper-flatpickr-today";

                const btn = document.createElement("button");
                btn.textContent = "Hôm nay";
                btn.type = "button";
                btn.className = "flatpickr-today-btn";
                btn.onclick = function () {
                    instance.setDate(new Date());
                    instance.close();
                };

                container_button.appendChild(btn);
                instance.rContainer.appendChild(container_button);
            }
        };

        // Định nghĩa onChange (nếu có .NET callback)
        if (dotnetHelper) {
            options.onChange = function (selectedDates, dateStr) {
                dotnetHelper.invokeMethodAsync("OnDateChanged", dateStr);
            };
        }

        // Áp dụng flatpickr cho từng phần tử có class
        Array.from(elements).forEach(el => {
            flatpickr(el, options);
        });
    },
    initFlatpickrClass: function (className, optionsJson) {
        const inputs = document.querySelectorAll(`.${className}`);
        const options = JSON.parse(optionsJson || "{}");
        inputs.forEach(el => {
            flatpickr(el, {
                dateFormat: format,
                locale: 'vn'
            });
        });
    },
    initFlatpickrMaskById: function (elementId, dotnetHelper, optionsJson) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const options = JSON.parse(optionsJson || "{}");

        // Đảm bảo cho phép gõ tay
        if (options.allowTypingDateMask) options.allowInput = true;
        if (!options.dateFormat) options.dateFormat = "d/m/Y"; // dd/MM/yyyy

        // onChange -> gọi .NET (giữ nguyên cách bạn đang làm)
        if (dotnetHelper) {
            options.onChange = function (selectedDates, dateStr) {
                dotnetHelper.invokeMethodAsync("OnDateChanged", dateStr);
            };
        }

        // onReady -> gắn mask
        const userOnReady = options.onReady;
        options.onReady = function (selectedDates, dateStr, instance) {
            if (options.todayButton) {
                const container_button = document.createElement("div");
                container_button.className = "wraper-flatpickr-today";

                const btn = document.createElement("button");
                btn.textContent = "Hôm nay";
                btn.type = "button";
                btn.className = "flatpickr-today-btn";
                btn.onclick = function () {
                    instance.setDate(new Date());
                    instance.close();
                };

                container_button.appendChild(btn);
                instance.rContainer.appendChild(container_button);
            }
            // lưu instance để mask sync được
            window.InputUtils._bindFlatpickrInstance(el, instance);

            if (typeof userOnReady === "function") userOnReady(selectedDates, dateStr, instance);

            if (options.allowTypingDateMask) {
                window.InputUtils.attachDateMask(el);
            }
        };

        const fp = flatpickr(el, options);
        // lưu luôn phòng trường hợp onReady không chạy trong vài theme/skin đặc biệt
        window.InputUtils._bindFlatpickrInstance(el, fp);
    },
    selectAllOnClickByClass: function (className) {
        const inputs = document.querySelectorAll('.' + className);
        inputs.forEach(input => {
            // Tránh select nếu đang double click hoặc tab key
            input.addEventListener('click', function (e) {
                this.select();
            });
            input.addEventListener('dblclick', function () {
                this.select();
            });
            input.addEventListener('focus', function () {
                this.select();
            });
        });
    },
    attachCurrencyFormatter: function (input, maxIntegerDigits = 18, decimalPlaces = 2) {
        if (!input) return;
        const onInput = () => {
            const caretPos = input.selectionStart ?? input.value.length;
            let raw = input.value.replace(/,/g, '');

            // Không cho bắt đầu bằng dấu chấm
            if (raw.startsWith('.')) raw = '';

            const parts = raw.split('.');
            const intPart = (parts[0] || '').replace(/\D/g, '').substring(0, maxIntegerDigits);
            const decPart = (parts[1] || '').replace(/\D/g, '').substring(0, decimalPlaces);

            let formatted = '';
            if (intPart.length > 0) {
                const formattedInt = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                formatted = formattedInt + (raw.includes('.') && decimalPlaces > 0 ? ('.' + decPart) : '');
            }

            const diff = formatted.length - input.value.length;
            input.value = formatted;
            // Giữ vị trí con trỏ tốt nhất có thể
            const newPos = Math.max(0, Math.min((caretPos ?? 0) + diff, input.value.length));
            input.setSelectionRange(newPos, newPos);
        };

        const onBlur = () => {
            let raw = input.value.replace(/,/g, '');
            if (!raw) return;

            const parts = raw.split('.');
            const intPart = (parts[0] || '').replace(/\D/g, '').substring(0, maxIntegerDigits);
            const decPart = (parts[1] || '').replace(/\D/g, '').substring(0, decimalPlaces);

            const formattedInt = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
            input.value = decPart.length > 0 ? `${formattedInt}.${decPart}` : formattedInt;
        };

        const selectAll = () => input.select();

        input.addEventListener('input', onInput);
        input.addEventListener('blur', onBlur);
        input.addEventListener('click', selectAll);
        input.addEventListener('dblclick', selectAll);
        input.addEventListener('focus', selectAll);

        // (Tuỳ chọn) trả về hàm cleanup nếu cần remove listener sau này
        return () => {
            input.removeEventListener('input', onInput);
            input.removeEventListener('blur', onBlur);
            input.removeEventListener('click', selectAll);
            input.removeEventListener('dblclick', selectAll);
            input.removeEventListener('focus', selectAll);
        };
    }
};

window.getInputValueById = function (id) {
    const el = document.getElementById(id);
    return el ? el.value : null;
};

window.flatpickrInitRange = (startId, endId, startDate, endDate, dotNetRef) => {

    const startInput = document.getElementById(startId);
    const endInput = document.getElementById(endId);

    if (!startInput || !endInput) return;

    flatpickr(startInput, {
        dateFormat: "d/m/Y",
        defaultDate: startDate,
        onChange: function (selectedDates, dateStr) {
            dotNetRef.invokeMethodAsync('UpdateDateRange', dateStr, endInput.value);
        }
    });

    flatpickr(endInput, {
        dateFormat: "d/m/Y",
        defaultDate: endDate,
        onChange: function (selectedDates, dateStr) {
            dotNetRef.invokeMethodAsync('UpdateDateRange', startInput.value, dateStr);
        }
    });

    // CLICK OUTSIDE
    document.addEventListener("mousedown", function (e) {
        const dropdown = document.querySelector('.daterange-dropdown');
        const trigger = document.querySelector('.daterange-display');

        if (dropdown && !dropdown.contains(e.target) && !trigger.contains(e.target)) {
            dotNetRef.invokeMethodAsync('CloseFromJs');
        }
    });
};

// ============ DATE MASK & SYNC WITH FLATPICKR (dd/MM/yyyy) ============
(function () {
    // Lưu instance flatpickr theo element để đồng bộ
    if (!window.InputUtils) window.InputUtils = {};
    const _fpMap = new WeakMap();
    window.InputUtils._bindFlatpickrInstance = function (el, fp) { _fpMap.set(el, fp); };
    function _getFp(el) { return _fpMap.get(el) || null; }

    // Cờ chống tái nhập (chống lặp oninput <-> setDate)
    const _reentry = new WeakMap();

    function _autoSlash(raw) {
        const digits = (raw || "").replace(/\D/g, "").slice(0, 8); // ddMMYYYY
        if (digits.length <= 2) return digits;
        if (digits.length <= 4) return digits.slice(0, 2) + "/" + digits.slice(2);
        return digits.slice(0, 2) + "/" + digits.slice(2, 4) + "/" + digits.slice(4);
    }

    function _daysInMonth(y, m) {
        return new Date(y, m, 0).getDate(); // m: 1..12
    }

    function _parseDdMMyyyy(s) {
        if (!/^\d{2}\/\d{2}\/\d{4}$/.test(s)) return { valid: false };
        const d = parseInt(s.slice(0, 2), 10);
        const m = parseInt(s.slice(3, 5), 10);
        const y = parseInt(s.slice(6, 10), 10);
        if (m < 1 || m > 12) return { valid: false };
        const dim = _daysInMonth(y, m);
        if (d < 1 || d > dim) return { valid: false };
        const jsDate = new Date(y, m - 1, d);
        // Double check round-trip
        if (jsDate.getFullYear() !== y || (jsDate.getMonth() + 1) !== m || jsDate.getDate() !== d) {
            return { valid: false };
        }
        return { valid: true, date: jsDate, d, m, y };
    }

    function _stripTime(dt) {
        const t = new Date(dt.getTime());
        t.setHours(0, 0, 0, 0);
        return t;
    }

    function _withinRange(dt, fp) {
        if (!fp) return true;
        const min = fp.config.minDate ? _stripTime(fp.config.minDate) : null;
        const max = fp.config.maxDate ? _stripTime(fp.config.maxDate) : null;
        const d0 = _stripTime(dt);
        if (min && d0 < min) return false;
        if (max && d0 > max) return false;
        return true;
    }

    function _setInvalid(el, on) {
        if (!el) return;
        if (on) el.classList.add("is-invalid");
        else el.classList.remove("is-invalid");
    }

    function _onKeyDown(e) {
        const allowed = ["Backspace", "Delete", "ArrowLeft", "ArrowRight", "Tab", "Home", "End"];
        if (allowed.includes(e.key)) return;
        if (/^\d$/.test(e.key)) return;
        e.preventDefault(); // chặn mọi ký tự khác (kể cả '/')
    }

    function _onInput(e) {
        const el = e.target;

        // Nếu đang trong phase setDate (do mình kích hoạt) -> bỏ qua input này
        if (_reentry.get(el)) return;

        const before = el.value || "";
        const masked = _autoSlash(before);
        if (masked !== before) {
            const pos = masked.length;
            el.value = masked;
            if (el.setSelectionRange) el.setSelectionRange(pos, pos);
        }
        if (el.value.length > 10) el.value = el.value.slice(0, 10);

        // Khi đủ 10 ký tự: validate + sync FP
        if (el.value.length === 10) {
            const parsed = _parseDdMMyyyy(el.value);
            const fp = _getFp(el);
            if (parsed.valid && _withinRange(parsed.date, fp)) {
                _setInvalid(el, false);

                if (fp) {
                    // KHÓA: tránh vòng lặp khi flatpickr.setDate làm phát input/change
                    _reentry.set(el, true);

                    // true = trigger onChange (để .NET OnDateChanged chạy). Nếu không cần, dùng false.
                    fp.setDate(parsed.date, true);

                    if (fp.isOpen) fp.jumpToDate(parsed.date);

                    // Mở khóa ở tick tiếp theo sau khi flatpickr xử lý xong
                    requestAnimationFrame(() => _reentry.set(el, false));
                }
            } else {
                _setInvalid(el, true);
            }
        } else {
            // chưa đủ 10 -> tạm bỏ invalid
            _setInvalid(el, false);
        }
    }

    function _attachDateMask(el) {
        if (!el || el._dateMaskAttached) return;
        el.addEventListener("keydown", _onKeyDown);
        el.addEventListener("input", _onInput, { passive: true });
        // chuẩn hóa giá trị sẵn có
        el.value = _autoSlash(el.value || "");
        el._dateMaskAttached = true;
    }

    window.InputUtils.attachDateMask = function (el) { _attachDateMask(el); };
    window.InputUtils.attachDateMaskById = function (id) {
        const el = document.getElementById(id);
        if (el) _attachDateMask(el);
    };

})();
