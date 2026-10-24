document.addEventListener("DOMContentLoaded", function () {
    let firstErrorSummary = null;

    document.querySelectorAll("form").forEach(function (form, formIndex) {
        const controls = Array.from(form.elements);

        form.querySelectorAll("[data-valmsg-for]").forEach(function (message, messageIndex) {
            const fieldName = message.dataset.valmsgFor;
            message.id = message.id || `validation-${formIndex}-${messageIndex}`;

            controls.forEach(function (control) {
                // Some visible selectors edit a value stored in a hidden field.
                const matchesField = control.name === fieldName ||
                    control.name?.startsWith(fieldName + "[") ||
                    control.dataset.validationFor === fieldName;

                if (!matchesField || control.type === "hidden") {
                    return;
                }

                const descriptions = (control.getAttribute("aria-describedby") || "")
                    .split(/\s+/).filter(Boolean);

                if (!descriptions.includes(message.id)) {
                    descriptions.push(message.id);
                }

                control.setAttribute("aria-describedby", descriptions.join(" "));

                if (message.textContent.trim()) {
                    control.setAttribute("aria-invalid", "true");
                }
            });
        });

        const summary = form.querySelector("[data-valmsg-summary]");
        if (summary && summary.textContent.trim()) {
            summary.classList.add("alert", "alert-danger");
            summary.setAttribute("tabindex", "-1");
            firstErrorSummary = firstErrorSummary || summary;
        }
    });

    // After a server-side validation failure, start with the complete error list.
    if (firstErrorSummary) {
        firstErrorSummary.focus();
    }
});
