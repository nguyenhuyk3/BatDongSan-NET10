window._uploadSessions = {};
window.initUploader = (id, allowExt, maxSize, allowMultiple, dotnetHelper) => {
    const container = document.getElementById(id);
    const input = document.getElementById(`${id}-input`);
    const preview = document.getElementById(`${id}-preview`);
    if (!input || !preview) {
        return;
    }
    const session = {
        uploadedFiles: [],
        dotnetHelper: dotnetHelper
    };
    window._uploadSessions[id] = session;
    const uploadedFiles = session.uploadedFiles;

    const handleFiles = async (files) => {
        if (!files || files.length === 0) return;

        //const validFiles = allowMultiple ? Array.from(files) : [files[0]];
        let validFiles = Array.from(files);
        if (!allowMultiple && validFiles.length > 1) {
            validFiles = [validFiles[validFiles.length - 1]];
        }
        const dt = new DataTransfer();
        let hasNewFile = false;

        for (let file of validFiles) {
            const ext = "." + file.name.split('.').pop().toLowerCase();
            if (!allowExt.includes(ext)) continue;
            if (file.size > maxSize) continue;

            const sizeText = getReadableSize(file.size);
            const exists = session.uploadedFiles.some(f =>
                f.fileName === file.name && f.fileSize === sizeText
            );
            if (exists) continue;
            if (!exists) {
                dt.items.add(file);
                hasNewFile = true;
            }
            const alreadyInList = session.uploadedFiles.some(f =>
                f.fileName === file.name && f.fileSize === sizeText
            );
            if (!alreadyInList) {
                let previewSrc = null;
                if (file.type.startsWith("image") && file.size < maxSize) {
                    previewSrc = await new Promise((resolve, reject) => {
                        const reader = new FileReader();
                        reader.onload = () => resolve(reader.result);
                        reader.onerror = () => reject(reader.error);
                        reader.readAsDataURL(file);
                    });
                }
                else if (file.type.startsWith("image")) {
                    previewSrc = URL.createObjectURL(file);
                }
                //reader.onload = (e) => {
                const fileData = {
                    id: null,
                    fileName: file.name,
                    contentType: file.type,
                    fileSize: sizeText,
                    _preview: previewSrc
                };

                if (allowMultiple) {
                    uploadedFiles.push(fileData);
                } else {
                    uploadedFiles.splice(0, uploadedFiles.length, fileData);
                }
            }
        }

        if (hasNewFile) {
            input.files = dt.files;
            input.dispatchEvent(new Event('change'));
        }

        //renderPreview();
        dotnetHelper.invokeMethodAsync("NotifyUploadComplete", uploadedFiles.map(f => {
            const { _preview, ...safeData } = f;
            return safeData;
        }));
    };

    const getReadableSize = (sizeInBytes) => {
        const kb = sizeInBytes / 1024;
        const mb = kb / 1024;
        const gb = mb / 1024;

        if (kb < 1024)
            return `${kb.toFixed(2)} KB`;
        else if (mb < 1024)
            return `${mb.toFixed(2)} MB`;
        else
            return `${gb.toFixed(2)} GB`;
    };

    container.addEventListener("drop", (e) => {
        e.preventDefault();
        container.classList.remove("drag-over");
        handleFiles(e.dataTransfer.files);
    });

    container.addEventListener("dragover", (e) => {
        e.preventDefault();
        container.classList.add("drag-over");
    });

    container.addEventListener("dragenter", () => {
        container.classList.add("drag-over");
    });
    container.addEventListener("dragleave", () => {
        container.classList.remove("drag-over");
    });

    //input.addEventListener("change", () => {
    //    handleFiles(input.files);

    //});

};

window.renderUploadedPreviews = (id, uploadedFiles, allowMultiple, isDuyet) => {
    const session = window._uploadSessions[id];
    if (!session) return;
    session.uploadedFiles = uploadedFiles;

    let isDaDuyet = false;
    if (isDuyet !== null && isDuyet !== undefined) {
        isDaDuyet = isDuyet
    }
    const preview = document.getElementById(`${id}-preview`);
    preview.innerHTML = "";
    uploadedFiles.forEach((file, index) => {
        const wrapper = document.createElement("div");
        wrapper.className = "relative rounded bg-gray-50 shadow opacity-0 transition-all duration-500 image-card";

        const removeBtn = document.createElement("button");
        removeBtn.innerText = "✖";
        removeBtn.className = "absolute top-1 right-2 text-red-500 delete-btn";
        removeBtn.onclick = () => {
            session.dotnetHelper.invokeMethodAsync("DeletePhysicalFile", file.fileNameSave, file.id);
            session.uploadedFiles.splice(index, 1);
            session.dotnetHelper.invokeMethodAsync("SyncUploadedFiles", session.uploadedFiles);
            window.renderUploadedPreviews(id, session.uploadedFiles, allowMultiple);
            session.dotnetHelper.invokeMethodAsync("NotifyUploadComplete", session.uploadedFiles);
        };

        const content = document.createElement(file.contentType.startsWith("image") ? "img" : "div");

        if (file.contentType.startsWith("image")) {
            content.src = file._preview || (file.folderUrl + file.fileNameSave);
            content.className = "w-full h-28 object-cover rounded-t image-element";
            if (allowMultiple) {
                preview.classList.add("md:grid-cols-4");
                preview.classList.remove("md:grid-cols-2");
            }
        }
        else {
            preview.classList.add("md:grid-cols-2");
            preview.classList.remove("md:grid-cols-4");

            const fileWrap = document.createElement("div");
            fileWrap.className = "d-flex align-items-center";

            const iconWrap = document.createElement("div");
            iconWrap.className = "d-flex align-items-center justify-content-center rounded m-2 icon-file-upload";

            const iconFile = document.createElement("i");
            iconFile.className = "bi bi-file-earmark-text";

            iconWrap.appendChild(iconFile);
            fileWrap.appendChild(iconWrap);

            const fileRightInfo = document.createElement("div");
            const fileInfo = document.createElement("div");
            fileInfo.className = "text-sm text-gray-700 file-name-upload text-left mt-2";
            fileInfo.innerText = file.fileName;

            const sizeInfo = document.createElement("div");
            sizeInfo.className = "file-size-upload text-xs text-gray-500 text-left mb-2";
            sizeInfo.innerText = file.fileSize;

            fileRightInfo.appendChild(fileInfo);
            fileRightInfo.appendChild(sizeInfo);
            fileWrap.appendChild(fileRightInfo);

            wrapper.appendChild(fileWrap);
        }



        const download = document.createElement("div");
        download.className = "action-image absolute top-0 left-0 d-none d-flex align-items-center justify-content-center w-full h-full rounded transition-all duration-500";

        const bgdownload = document.createElement("div");
        bgdownload.className = "action-image absolute d-flex top-0 left-0 align-items-center justify-content-center w-full h-full rounded bg-gray-500 opacity-40";
        download.appendChild(bgdownload);

        const downloadSpanIcon = document.createElement("span");
        downloadSpanIcon.className = "absolute pt-1 icon-download-file";
        downloadSpanIcon.onclick = () => {
            if (session.dotnetHelper) {
                session.dotnetHelper.invokeMethodAsync("DownloadPhysicalFile", file.fileNameSave, file.id);
            }
        };

        const downloadIcon = document.createElement("i");
        downloadIcon.className = "bi bi-download text-white";
        downloadSpanIcon.appendChild(downloadIcon);

        download.appendChild(downloadSpanIcon);
        if (!isDaDuyet)
            download.appendChild(removeBtn);

        if (file.contentType.startsWith("image")) {
            wrapper.appendChild(content);
        }

        wrapper.appendChild(download);
        preview.appendChild(wrapper);
        setTimeout(() => wrapper.classList.remove("opacity-0"), 10);
    });
};

window.updateFileNameSaves = (serverFiles, uploaderId) => {
    const session = window._uploadSessions[uploaderId];
    if (!session) return;

    session.uploadedFiles.forEach(local => {
        const matched = serverFiles.find(s =>
            s.fileName === local.fileName &&
            s.fileSize === local.fileSize
        );
        if (matched) {
            local.fileNameSave = matched.fileNameSave;
            local.folderUrl = matched.folderUrl;
        }
    });

    const dotnetHelper = session.dotnetHelper;
    if (dotnetHelper) {
        dotnetHelper.invokeMethodAsync("NotifyUploadComplete", session.uploadedFiles.map(f => {
            const { _preview, ...safe } = f;
            return safe;
        }));
    }
};

window.triggerUploaderSelect = (id) => {
    const input = document.getElementById(`${id}-input`);
    if (input && typeof input.click === 'function') {
        input.click();
    }
};

window.downloadHelper = {
    download: function (url, fileName) {
        try {
            const a = document.createElement("a");
            a.href = url;
            if (fileName) {
                a.download = fileName; // gợi ý tên file
            } else {
                a.download = "";
            }
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
        } catch (e) {
            console.error("Download error:", e);
        }
    }
};

window.downloadFileFromUrl = function (url) {
    const a = document.createElement("a");
    a.href = url;
    a.target = "_blank";
    a.download = "";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}
window.downloadFileFromByteArray = ({ byteArray, fileName, contentType }) => {
    const link = document.createElement('a');
    link.href = `data:${contentType};base64,${byteArray}`;
    link.download = fileName;
    link.click();
};
window.downloadFileFromBytes = (filename, base64) => {
    const link = document.createElement('a');
    link.href = "data:application/octet-stream;base64," + base64;
    link.download = filename;
    link.click();
};