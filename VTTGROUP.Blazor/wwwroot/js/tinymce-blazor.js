window.tinymceBlazor = (function () {
    const instances = new Map();

    function init(id, options) {
        const el = document.getElementById(id);
        if (!el) return;

        // Gộp options mặc định + custom
        const base = {            
            selector: `#${id}`,
            license_key: 'gpl',
            menubar: true,
            plugins: 'lists link image table media code',
            toolbar: 'undo redo | bold italic underline blockquote | alignleft aligncenter alignright | bullist numlist outdent indent| table link image media | code',
            automatic_uploads: true,  
            branding: false,
            promotion: false,
            content_style: `
            body { font-size: 13px; }
            blockquote {
                border-left: 4px solid #ccc;
                padding-left: 10px;
                color: #666;
                font-style: italic;
                margin: 1rem 0;
            }
            `,
            height: 500,
            //language: 'vi',
            // Đồng bộ nội dung về textarea để hỗ trợ form submit
            setup: function (editor) {
                editor.on('change input undo redo keyup', function () {
                    editor.save(); // cập nhật <textarea>
                });
                editor.on('keydown', function (e) {
                    if (e.key === 'Tab') {
                        if (e.shiftKey) {
                            editor.execCommand('Outdent');
                        } else {
                            editor.execCommand('Indent');
                        }
                        e.preventDefault();
                    }
                });
            },
            file_picker_types: 'image',
            file_picker_callback: function (cb, value, meta) {
                if (meta.filetype === 'image') {
                    const input = document.createElement('input');
                    input.setAttribute('type', 'file');
                    input.setAttribute('accept', 'image/*');

                    input.onchange = function () {
                        const file = this.files[0];
                        const reader = new FileReader();
                        reader.onload = function () {
                            const id = 'blobid' + (new Date()).getTime();
                            const blobCache = tinymce.activeEditor.editorUpload.blobCache;
                            const base64 = reader.result.split(',')[1];
                            const blobInfo = blobCache.create(id, file, base64);
                            blobCache.add(blobInfo);
                            cb(blobInfo.blobUri(), { title: file.name });
                        };
                        reader.readAsDataURL(file);
                    };

                    input.click();
                }
            }
        };
        const cfg = Object.assign({}, base, options || {});

        // Khởi tạo
        tinymce.init(cfg).then((eds) => {
            const editor = eds[0];
            instances.set(id, editor);
        });
    }

    function setContent(id, html) {
        const ed = instances.get(id);
        if (ed) ed.setContent(html || '');
    }

    function getContent(id) {
        const ed = instances.get(id);
        return ed ? ed.getContent() : '';
    }

    function destroy(id) {
        const ed = instances.get(id);
        if (ed) {
            ed.destroy();
            instances.delete(id);
        }
    }

    return { init, setContent, getContent, destroy };
})();