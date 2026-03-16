//function validateFormBlazor(formId, dotNetHelper) {
//    const $form = $('#' + formId);
//    if ($form.length === 0) return false;

//    let isValid = true;

//    $form.find('.input-validation').each(function () {
//        const $input = $(this);
//        const value = $input.val().trim();
//        const $errorSpan = $input.parent().find('.error-validation');

//        let isNumber = false;
//        if ($input.hasClass('is-numberic')) {
//            isNumber = true;
//        }

//        if (!value || (isNumber && value == "0")) {
//            $input.addClass('validate');
//            $input.closest('.form-group-input').addClass('has-validate');
//            $errorSpan.show();
//            isValid = false;
//        } else {
//            $input.removeClass('validate');
//            $input.closest('.form-group-input').removeClass('has-validate');
//            $errorSpan.hide();
//        }
//    });

//    if (!isValid) {
//        $('.container-mess-error').show();
//        $('#mess-error').text("Có trường dữ liệu không chính xác hoặc không hợp lệ");
//        return false;
//    }
//    // Nếu hợp lệ, gọi method C# Blazor
//    if (isValid && dotNetHelper) {
//        dotNetHelper.invokeMethodAsync("SubmitForm");
//    }
//    return false;
//}

function validateFormBlazor(formId, dotNetHelper) {
    const $form = $('#' + formId);
    if ($form.length === 0) return false;

    let isValid = true;

    $form.find('.input-validation').each(function () {
        const $input = $(this);
        const value = ($input.val() || '').toString().trim();

        // Lấy form-group bao quanh input
        const $group = $input.closest('.form-group-input');
        const $errorSpan = $group.find('.error-validation');

        let isNumber = $input.hasClass('is-numberic');

        if (!value || (isNumber && value === "0")) {
            $input.addClass('validate');
            $group.addClass('has-validate');
            $errorSpan.show();
            isValid = false;
        } else {
            $input.removeClass('validate');
            $group.removeClass('has-validate');
            $errorSpan.hide();
        }
    });

    if (!isValid) {
        $('.container-mess-error').show();
        $('#mess-error').text("Có trường dữ liệu không chính xác hoặc không hợp lệ");

        return false;
    }

    // Nếu hợp lệ, gọi method C# Blazor
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync("SubmitForm");
    }
    return false;
}

function validateFormBlazorCSBH(formId, dotNetHelper) {
    const $form = $('#' + formId);
    if ($form.length === 0) return false;

    let isValid = true;
    let firstInvalidFocus = null; // { scrollEl: HTMLElement, focus: ()=>void }

    $form.find('.input-validation').each(function () {
        const $input = $(this);
        const value = ($input.val() ?? '').toString().trim();
        const isNumber = $input.hasClass('is-numberic');
        const invalid = (!value || (isNumber && value === "0"));

        // fallback group (vì trong table không có .form-group-input)
        let $group = $input.closest('.form-group-input');
        if ($group.length === 0) $group = $input.closest('td');

        const $errorSpan = $group.find('.error-validation');

        // --- SELECT2 HANDLING ---
        const isSelect2 = $input.hasClass('select2-hidden-accessible');
        const $select2Box = isSelect2
            ? ($input.next('.select2').length ? $input.next('.select2') : $input.parent().find('.select2').first())
            : null;

        // --- FLATPICKR HANDLING ---
        const el = $input[0];
        const fp = el?._flatpickr;

        // Tìm input hiển thị của flatpickr (altInput) nếu có,
        // hoặc fallback: input visible kế bên (trường hợp input gốc bị hidden)
        const $fpVisible = (() => {
            if (fp?.altInput) return $(fp.altInput);
            // nếu input gốc bị hidden/không visible -> tìm input visible kế tiếp
            if (!$input.is(':visible') || ($input.attr('type') || '').toLowerCase() === 'hidden') {
                const $n = $input.nextAll('input:visible').first();
                if ($n.length) return $n;
            }
            return null;
        })();

        if (invalid) {
            $input.addClass('validate');
            $group.addClass('has-validate');
            if ($errorSpan.length) $errorSpan.show();

            // đỏ đúng select2 UI
            if (isSelect2 && $select2Box && $select2Box.length) {
                $select2Box.find('.select2-selection').addClass('validate');
            }

            // đỏ đúng flatpickr input hiển thị
            if ($fpVisible && $fpVisible.length) {
                $fpVisible.addClass('validate');
            }

            // set first invalid (ưu tiên focus đúng cái user nhìn thấy)
            if (!firstInvalidFocus) {
                // focus action theo loại control
                if (isSelect2) {
                    const selectionEl = ($select2Box && $select2Box.length)
                        ? $select2Box.find('.select2-selection')[0]
                        : $input[0];

                    firstInvalidFocus = {
                        scrollEl: selectionEl || $input[0],
                        focus: () => {
                            try { $input.select2('open'); } catch { }
                            try { $(selectionEl).focus(); } catch { }
                        }
                    };
                }
                else if (fp || ($fpVisible && $fpVisible.length)) {
                    const targetEl = ($fpVisible && $fpVisible.length) ? $fpVisible[0] : $input[0];

                    firstInvalidFocus = {
                        scrollEl: targetEl || $input[0],
                        focus: () => {
                            // nếu có instance thì open chuẩn
                            if (fp) {
                                try { fp.open(); } catch { }
                                try { (fp.altInput || fp.input || targetEl).focus(); } catch { }
                            } else {
                                // fallback: focus/click vào input visible để flatpickr mở
                                try { $(targetEl).focus(); } catch { }
                                try { $(targetEl).trigger('click'); } catch { }
                            }
                        }
                    };
                }
                else {
                    firstInvalidFocus = {
                        scrollEl: $input[0],
                        focus: () => { try { $input.focus(); } catch { } }
                    };
                }
            }

            isValid = false;
        } else {
            $input.removeClass('validate');
            $group.removeClass('has-validate');
            if ($errorSpan.length) $errorSpan.hide();

            if (isSelect2 && $select2Box && $select2Box.length) {
                $select2Box.find('.select2-selection').removeClass('validate');
            }

            if ($fpVisible && $fpVisible.length) {
                $fpVisible.removeClass('validate');
            }
        }
    });

    if (!isValid) {
        $('.container-mess-error').show();
        $('#mess-error').text("Có trường dữ liệu không chính xác hoặc không hợp lệ");

        if (firstInvalidFocus?.scrollEl) {
            try {
                firstInvalidFocus.scrollEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
            } catch { }
        }

        if (firstInvalidFocus?.focus) {
            setTimeout(() => firstInvalidFocus.focus(), 150);
        }

        return false;
    }

    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync("SubmitForm");
    }
    return false;
}


window.validateContainer = function (containerSelector, errorBoxSelector, messageText) {
    const $roots = $(containerSelector);
    if ($roots.length === 0) return true;

    let isValid = true;
    let $firstInvalid = null;

    $roots.each(function () {
        const $root = $(this);

        $root.find('.input-validation').each(function () {
            const $input = $(this);

            // Bỏ qua disabled / readonly
            if ($input.is(':disabled') || $input.is('[readonly]')) return;

            // Có phải select2 không? (dựa vào .select2 nằm kế bên)
            const $s2Wrapper = $input.next('.select2');
            const isSelect2 = $s2Wrapper.length > 0;

            const raw = ($input.val() ?? '').toString();
            const value = raw.trim();

            const isNumber = $input.hasClass('is-numberic');
            const invalid = !value || (isNumber && value === '0');

            if (invalid) {
                $input.addClass('validate');

                // tô đỏ khung select2 nếu có
                if (isSelect2) {
                    $s2Wrapper.find('.select2-selection').addClass('validate');
                }

                if (!$firstInvalid) {
                    $firstInvalid = $input;
                }

                isValid = false;
            } else {
                $input.removeClass('validate');

                if (isSelect2) {
                    $s2Wrapper.find('.select2-selection').removeClass('validate');
                }
            }
        });
    });

    if (!isValid) {
        if ($firstInvalid) {
            let $focusTarget = $firstInvalid;
            const $s2Wrapper = $firstInvalid.next('.select2');

            // Nếu là select2 thì focus vào .select2-selection
            if ($s2Wrapper.length > 0) {
                $focusTarget = $s2Wrapper.find('.select2-selection');
            }

            const top = Math.max(0, $focusTarget.offset().top - 120);
            $('html, body').animate({ scrollTop: top }, 250);
            $focusTarget.trigger('focus');

            // mở dropdown luôn cho dễ thấy
            if ($firstInvalid.data('select2')) {
                $firstInvalid.select2('open');
            }
        }

        if (errorBoxSelector) {
            const $box = $(errorBoxSelector);
            if ($box.length) {
                $box.show();
                const $msg = $box.find('#mess-error');
                if ($msg.length) {
                    $msg.text(messageText || 'Có trường dữ liệu không chính xác hoặc không hợp lệ');
                }
            }
        }

        return false;
    }

    // Hợp lệ: ẩn hộp lỗi (nếu có) rồi trả về true
    if (errorBoxSelector) {
        const $box = $(errorBoxSelector);
        if ($box.length) $box.hide();
    }

    return true;
};

window.clearValidateById = function (id) {
    const $el = $('#' + id);
    if (!$el.length) return;

    let $group = $el.closest('.form-group-input');
    if ($group.length === 0) $group = $el.closest('td');

    $el.removeClass('validate');
    $group.removeClass('has-validate');
    $group.find('.error-validation').hide();

    // flatpickr altInput cũng gỡ đỏ nếu có
    const fp = $el[0]?._flatpickr;
    if (fp?.altInput) $(fp.altInput).removeClass('validate');
};

function handleInputValidationChange() {
    const $input = $(this);

    if ($input.is(':disabled') || $input.is('[readonly]')) return;

    const raw = ($input.val() ?? '').toString();
    const value = raw.trim();
    const isNumber = $input.hasClass('is-numberic');
    const invalid = !value || (isNumber && value === '0');

    // group + span lỗi (nếu có, form khác có dùng)
    const $group = $input.closest('.form-group-input');
    const $errorSpan = $group.find('.error-validation');

    // nếu là select2 thì wrapper nằm ngay sau select
    const $s2Wrapper = $input.next('.select2');

    if (invalid) {
        $input.addClass('validate');
        if ($s2Wrapper.length) {
            $s2Wrapper.find('.select2-selection').addClass('validate');
        }
        $group.addClass('has-validate');
        $errorSpan.show();
    } else {
        // 👉 user đã sửa đúng: gỡ lỗi ngay lập tức
        $input.removeClass('validate');
        if ($s2Wrapper.length) {
            $s2Wrapper.find('.select2-selection').removeClass('validate');
        }
        $group.removeClass('has-validate');
        $errorSpan.hide();
    }
}


window.liveValidationHelper = {
    init: function (containerSelector) {
        const $roots = $(containerSelector);
        if ($roots.length === 0) return;

        // Gỡ handler cũ (nếu có) rồi gắn lại
        $roots.off('input.livevalidate change.livevalidate')
            .on('input.livevalidate change.livevalidate', '.input-validation', function () {
                const $input = $(this);

                // bỏ qua disabled / readonly
                if ($input.is(':disabled') || $input.is('[readonly]')) return;

                const raw = ($input.val() ?? '').toString();
                const value = raw.trim();
                const isNumber = $input.hasClass('is-numberic');
                const invalid = !value || (isNumber && value === '0');

                // group + span lỗi (nếu có)
                const $group = $input.closest('.form-group-input');
                const $errorSpan = $group.find('.error-validation');

                // nếu là select2 thì wrapper nằm ngay sau select
                const $s2Wrapper = $input.next('.select2');

                if (invalid) {
                    // nếu user xoá sạch → bật lỗi lại
                    $input.addClass('validate');
                    if ($s2Wrapper.length) {
                        $s2Wrapper.find('.select2-selection').addClass('validate');
                    }
                    $group.addClass('has-validate');
                    $errorSpan.show();
                } else {
                    // user chọn/nhập lại đúng → gỡ lỗi
                    $input.removeClass('validate');
                    if ($s2Wrapper.length) {
                        $s2Wrapper.find('.select2-selection').removeClass('validate');
                    }
                    $group.removeClass('has-validate');
                    $errorSpan.hide();
                }
            });
    }
};

function validateForm(formId, dotNetHelper) {
    const $form = $('#' + formId);
    if ($form.length === 0) return false;

    let isValid = true;

    //$form.find('.input-validation').each(function () {
    //    const $input = $(this);
    //    const value = $input.val().trim();
    //    const $errorSpan = $input.parent().find('.error-validation');

    //    if (!value) {
    //        $input.addClass('validate');
    //        $input.closest('.form-group-input').addClass('has-validate');
    //        $errorSpan.show();
    //        isValid = false;
    //    } else {
    //        $input.removeClass('validate');
    //        $input.closest('.form-group-input').removeClass('has-validate');
    //        $errorSpan.hide();
    //    }
    //});
    $form.find('.input-validation').each(function () {
        const $input = $(this);
        const val = $input.val();
        const $errorSpan = $input.parent().find('.error-validation');

        let isEmpty = false;

        if (Array.isArray(val)) {
            // Với select multiple
            isEmpty = val.length === 0;
        } else {
            // Với input text hoặc select thường
            isEmpty = !val || val.trim() === "";
        }

        if (isEmpty) {
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

    if (!isValid) {
        $('.container-mess-error').show();
        $('#mess-error').text("Có trường dữ liệu không chính xác hoặc không hợp lệ");
        return false;
    }

    // Nếu hợp lệ, gọi method C# Blazor
    if (isValid && dotNetHelper) {
        collectFormData(formId).then(data => {
            if (data && Object.keys(data).length > 0) {
                dotNetHelper.invokeMethodAsync("SubmitForm", data);
            } else {
                console.warn("❌ Dữ liệu form rỗng, không post lên Blazor");
            }
        });
        //const data = collectFormData(formId);
        //if (!data) return;

        //try {
        //    dotNetHelper.invokeMethodAsync("SubmitForm", data);
        //} catch (err) {
        //}

        //dotNetHelper.invokeMethodAsync("SubmitForm", data);
    }

    return false; // chặn form submit mặc định
}

function collectFormData(formId) {
    const $form = $('#' + formId);
    if ($form.length === 0) return null;

    let formData = {};
    let checkGroups = {};
    let filePromises = [];

    $form.find('input:enabled, select:enabled, textarea:enabled').each(function () {
        const $field = $(this);
        const name = $field.attr('name');
        const type = $field.attr('type');

        //if (type === 'file') {
        //    const files = $field[0].files;
        //    if (!files || files.length === 0) return;

        //    for (let file of files) {
        //        filePromises.push(readFileAsBase64(file));
        //    }
        //}
        //else
        if (name) {
            if (type === 'checkbox') {
                const checkboxesWithSameName = $form.find(`input[name="${name}"][type=checkbox]`);
                // Nếu nhiều checkbox cùng name → gom thành array
                if (checkboxesWithSameName.length > 1) {
                    if ($field.prop('checked')) {
                        if (!checkGroups[name]) checkGroups[name] = [];
                        checkGroups[name].push($field.val());
                    }
                }
                // Nếu chỉ có 1 checkbox duy nhất → lấy true/false
                else {
                    formData[name] = $field.prop('checked');
                }
            } else if (type === 'radio') {
                if ($field.prop('checked')) {
                    formData[name] = $field.val();
                }
            } else {
                formData[name] = $field.val();
            }
        }
    });

    //Object.assign(formData, checkGroups);
    return Promise.all(filePromises).then(fileArray => {
        if (fileArray.length > 0) {
            formData["Files"] = fileArray;
        }

        Object.assign(formData, checkGroups);
        return formData;
    });

    return formData;
}

function readFileAsBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = e => {
            resolve({
                fileName: file.name,
                contentType: file.type,
                base64: e.target.result
            });
        };
        reader.onerror = reject;
        reader.readAsDataURL(file);
    });
}

function clearValidateForElement($el) {
    // fallback group (table td)
    let $group = $el.closest('.form-group-input');
    if ($group.length === 0) $group = $el.closest('td');

    const $errorSpan = $group.find('.error-validation');

    // remove class ở input/select gốc
    $el.removeClass('validate');
    $group.removeClass('has-validate');
    if ($errorSpan.length) $errorSpan.hide();

    // nếu là select2 thì remove luôn trên UI box
    if ($el.hasClass('select2-hidden-accessible')) {
        const $box = $el.next('.select2');
        if ($box.length) $box.find('.select2-selection').removeClass('validate');
    }
}


// Xóa lỗi khi focus hoặc click
$(document).ready(function () {
    $(document).on('focus click change', '.input-validation', function () {
        $(this).removeClass('validate');
        $(this).parent().find('.error-validation').hide();
        $(this).closest('.form-group-input').removeClass('has-validate');
    });
    $(document).on('click', '.icon-choose', function () {
        var $obj = $(this);
        var $parent = $obj.closest('.input-wraper-choose');
        $parent.find('.error-validation').hide();
        $parent.find('.input-validation').removeClass('validate');
        $parent.closest('.form-group-input').removeClass('has-validate');
    });

    $(document).on('click', '.close-mess-error', function () {
        $('.container-mess-error').hide();
    });
    $(document)
        .on('input', '.input-validation', handleInputValidationChange)
        .on('change', '.input-validation', handleInputValidationChange);
    // 2) select2 (event riêng)
    $(document).on('select2:select select2:clear', 'select.input-validation', function () {
        const $el = $(this);
        const value = ($el.val() ?? '').toString().trim();
        if (value) clearValidateForElement($el);
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const helpInit = () => {
        document.querySelectorAll('.form-error-inline').forEach(function (box) {
            const mess = box.querySelector('#mess-error');
            const btnClose = box.querySelector('.close-mess-error');

            if (!mess || !mess.textContent.trim()) {
                box.classList.add('hidden');
            } else {
                box.classList.remove('hidden');
            }

            if (btnClose) {
                btnClose.addEventListener('click', function (e) {
                    e.preventDefault();
                    box.classList.add('hidden');
                    if (mess) mess.textContent = '';
                    const ev = new CustomEvent('khachhang:errorClosed', { detail: { boxId: box.id || null } });
                    window.dispatchEvent(ev);
                });
            }
        });
    };

    helpInit();

    const observer = new MutationObserver(function () {
        helpInit();
    });
    observer.observe(document.body, { childList: true, subtree: true, characterData: true });
});

/**
 * Xóa toàn bộ trạng thái validation trong một container (dùng cho modal/popup).
 * @param {string} containerId - id của element chứa form (không có dấu #)
 */
window.clearValidationState = function (containerId) {
    const $container = $('#' + containerId);
    if ($container.length === 0) return;

    // Gỡ class validate khỏi tất cả input/select
    $container.find('.input-validation').each(function () {
        const $input = $(this);
        $input.removeClass('validate');

        // select2: gỡ đỏ trên UI box
        const $s2Box = $input.next('.select2');
        if ($s2Box.length) {
            $s2Box.find('.select2-selection').removeClass('validate');
        }

        // flatpickr: gỡ đỏ trên altInput nếu có
        const fp = this._flatpickr;
        if (fp && fp.altInput) {
            $(fp.altInput).removeClass('validate');
        }
    });

    // Gỡ class has-validate khỏi tất cả form-group
    $container.find('.form-group-input').removeClass('has-validate');

    // Ẩn tất cả span lỗi
    $container.find('.error-validation').hide();
};
