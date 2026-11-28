// Validate all achievement fields together before either form is submitted.
document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form[data-achievement-validation]");
    if (!form) return;

    const fields = ["Name", "Description", "Type", "RequirementValue", "Points"];
    const summary = form.querySelector("[data-valmsg-summary]");
    const requirement = form.elements.namedItem("RequirementValue");
    let attempted = false;

    // This form owns its client validation; avoid duplicate jQuery messages.
    if (window.jQuery && jQuery(form).data("validator")) {
        jQuery(form).validate().destroy();
    }

    function validate() {
        const errors = {};
        const name = form.elements.namedItem("Name").value;
        const description = form.elements.namedItem("Description").value;
        const type = form.elements.namedItem("Type").value;
        const maximum = type === "ScorePercentage" ? 100 : 1000;
        requirement.max = String(maximum);
        const value = Number(requirement.value);
        const points = Number(form.elements.namedItem("Points").value);

        if (!name.trim()) errors.Name = "Name is required.";
        else if (name.length > 100) errors.Name = "Name cannot be longer than 100 characters.";
        if (!description.trim()) errors.Description = "Description is required.";
        else if (description.length > 300) errors.Description = "Description cannot be longer than 300 characters.";
        if (!type) errors.Type = "Achievement type is required.";
        if (!Number.isInteger(value) || value < 1 || value > maximum) {
            errors.RequirementValue = "Requirement must be between 1 and " + maximum + ".";
        }
        if (![5, 10, 15, 20].includes(points)) errors.Points = "AP must be 5, 10, 15 or 20.";
        return errors;
    }

    function render(errors) {
        for (const name of fields) {
            const control = form.elements.namedItem(name);
            const message = form.querySelector('[data-valmsg-for="' + name + '"]');
            const invalid = Boolean(errors[name]);
            control.classList.toggle("input-validation-error", invalid);
            control.setAttribute("aria-invalid", String(invalid));
            if (message) {
                message.textContent = errors[name] || "";
                message.classList.toggle("field-validation-error", invalid);
                message.classList.toggle("field-validation-valid", !invalid);
            }
        }
        const messages = Object.values(errors);
        summary.replaceChildren();
        summary.classList.toggle("validation-summary-errors", messages.length > 0);
        summary.classList.toggle("validation-summary-valid", messages.length === 0);
        summary.classList.toggle("alert", messages.length > 0);
        summary.classList.toggle("alert-danger", messages.length > 0);
        if (messages.length) {
            const heading = document.createElement("strong");
            heading.textContent = "Please correct the following errors:";
            const list = document.createElement("ul");
            for (const error of messages) {
                const item = document.createElement("li");
                item.textContent = error;
                list.append(item);
            }
            summary.append(heading, list);
        }
    }

    validate();
    form.addEventListener("submit", function (event) {
        attempted = true;
        const errors = validate();
        render(errors);
        if (Object.keys(errors).length) {
            event.preventDefault();
            event.stopImmediatePropagation();
            summary.tabIndex = -1;
            summary.focus();
        }
    }, true);
    function refresh() {
        const errors = validate();
        if (attempted) render(errors);
    }
    form.addEventListener("input", refresh);
    form.addEventListener("change", refresh);
});
