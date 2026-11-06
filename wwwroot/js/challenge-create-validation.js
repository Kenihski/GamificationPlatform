// Live required-field validation for Challenge/Create.
// Backend model validation remains authoritative for submitted data.
document.addEventListener("DOMContentLoaded", function () {
  const form = document.querySelector('form[action*="/Challenge/Create"]');
  if (!form) return;

  const title = form.querySelector('[name="Title"]');
  const description = form.querySelector('[name="Description"]');
  const serverSummary = form.querySelector('[data-valmsg-summary="true"]');
  if (!title || !description) return;

  form.noValidate = true;
  const summary = document.createElement("div");
  summary.className = "alert alert-danger d-none";
  summary.setAttribute("role", "alert");
  summary.tabIndex = -1;
  if (serverSummary) serverSummary.insertAdjacentElement("afterend", summary);
  else form.prepend(summary);

  let attempted = false;

  function getErrors() {
    const errors = [];
    if (!title.value.trim()) errors.push({ field: "Title", text: "Challenge title is required." });
    if (!description.value.trim()) errors.push({ field: "Description", text: "Description is required." });
    return errors;
  }

  function render(errors) {
    summary.replaceChildren();
    summary.classList.toggle("d-none", errors.length === 0);
    if (errors.length) {
      const heading = document.createElement("strong");
      heading.textContent = "Please correct the following errors:";
      const list = document.createElement("ul");
      list.className = "mb-0 mt-2";
      errors.forEach(({ text }) => {
        const item = document.createElement("li");
        item.textContent = text;
        list.append(item);
      });
      summary.append(heading, list);
    }

    for (const [field, element] of [["Title", title], ["Description", description]]) {
      const error = errors.find((item) => item.field === field);
      element.classList.toggle("input-validation-error", Boolean(error));
      element.setAttribute("aria-invalid", error ? "true" : "false");
      const inline = form.querySelector('[data-valmsg-for="' + field + '"]');
      if (inline) {
        inline.textContent = error ? error.text : "";
        inline.classList.toggle("field-validation-error", Boolean(error));
        inline.classList.toggle("field-validation-valid", !error);
      }
    }

    // Avoid displaying the previous server summary alongside the live one.
    // It may contain stale required-field messages from a prior submission.
    if (serverSummary && attempted) {
      const list = serverSummary.querySelector("ul");
      if (list) list.replaceChildren();
      serverSummary.classList.remove("validation-summary-errors");
      serverSummary.classList.add("validation-summary-valid");
    }
  }

  form.addEventListener("submit", function (event) {
    attempted = true;
    const errors = getErrors();
    render(errors);
    if (errors.length) {
      event.preventDefault();
      event.stopImmediatePropagation();
      summary.focus({ preventScroll: true });
      summary.scrollIntoView({ behavior: "smooth", block: "start" });
    }
  }, true);

  form.addEventListener("input", function () {
    if (attempted) render(getErrors());
  });
});
