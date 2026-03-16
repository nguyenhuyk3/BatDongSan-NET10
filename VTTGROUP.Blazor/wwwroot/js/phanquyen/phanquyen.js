export function ToggleCheckAll(element, checked) {
    $("[data-u='" + element + "']").prop("checked", checked);
}