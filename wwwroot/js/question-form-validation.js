// Mirrors QuestionController's answer-count, uniqueness and correct-option
// rules for immediate feedback. Server validation remains authoritative.
document.addEventListener("DOMContentLoaded", function () {
  const typeSelect = document.getElementById("questionType");
  const form = typeSelect?.closest("form");
  if (!form) return;

  const message = document.createElement("div");
  message.className = "alert alert-danger d-none";
  message.setAttribute("role", "alert");
  message.setAttribute("aria-live", "polite");
  message.id = "questionClientValidationError";

  const summary = form.querySelector('[data-valmsg-summary="true"]');
  if (summary) summary.insertAdjacentElement("afterend", message);
  else form.prepend(message);

  function checkAnswers() {
    const errors = [];
    const questionType = typeSelect.value;
    if (questionType === "ShortAnswer") {
      const answers = Array.from(
        form.querySelectorAll('input[name^="AcceptedAnswers["]'),
      )
        .map((input) => input.value.trim())
        .filter(Boolean);
      if (answers.length < 1 || answers.length > 4) {
        errors.push("Enter between 1 and 4 accepted answers.");
      }
      if (answers.some((answer) => answer.length > 200)) {
        errors.push("Each accepted answer must be at most 200 characters.");
      }
      if (
        new Set(answers.map((a) => a.toLowerCase())).size !== answers.length
      ) {
        errors.push("Accepted answers must be different.");
      }
    } else if (
      questionType === "SingleChoice" ||
      questionType === "MultipleChoice"
    ) {
      const options = Array.from(
        form.querySelectorAll('input[name^="Options["]'),
      );
      const filled = options.map((input) => input.value.trim()).filter(Boolean);
      if (options.length > 4 || filled.length < 2 || filled.length > 4) {
        errors.push("Enter between 2 and 4 answer options.");
      }
      if (filled.some((answer) => answer.length > 200)) {
        errors.push("Each answer option must be at most 200 characters.");
      }
      if (new Set(filled.map((a) => a.toLowerCase())).size !== filled.length) {
        errors.push("Answer options must be different.");
      }

      const selected = Array.from(
        form.querySelectorAll('input[name="CorrectOptions"]:checked'),
      ).map((input) => Number(input.value));
      if (!selected.length) {
        errors.push("Select at least one correct answer.");
      } else if (
        selected.some(
          (index) =>
            !Number.isInteger(index) ||
            index < 0 ||
            index > 3 ||
            !options[index]?.value.trim(),
        )
      ) {
        errors.push("Correct answers must use filled answer options.");
      }
      if (questionType === "SingleChoice" && selected.length !== 1) {
        errors.push(
          "Single choice questions must have exactly one correct answer.",
        );
      }
    } else {
      errors.push("Please select a valid question type.");
    }

    message.textContent = errors.join(" ");
    message.classList.toggle("d-none", errors.length === 0);
    return errors.length === 0;
  }

  // Validate before sending; preserve all existing server checks.
  form.addEventListener("submit", function (event) {
    if (!checkAnswers()) {
      event.preventDefault();
      message.scrollIntoView({ behavior: "smooth", block: "center" });
    }
  });
  function refreshFieldValidation(event) {
    const field = event.target;

    if (!field.matches("input, select, textarea")) {
      return;
    }

    const $ = window.jQuery;

    if (!$ || !$.validator) {
      return;
    }

    const validator = $(form).data("validator");

    if (validator && field.name) {
      validator.element(field);
    }
  }
  function clearStaleValidationSummary() {
    const $ = window.jQuery;

    if (!$ || !$.validator) {
      return;
    }

    const validator = $(form).data("validator");

    if (!validator) {
      return;
    }

    const summary = form.querySelector('[data-valmsg-summary="true"]');

    if (!summary) {
      return;
    }

    // Only clear the summary when jQuery reports
    // that the entire form is valid.
    if ($(form).valid()) {
      const list = summary.querySelector("ul");

      if (list) {
        list.replaceChildren();
      }

      summary.classList.remove("validation-summary-errors");

      summary.classList.add("validation-summary-valid");
    }
  }
  form.addEventListener("input", function (event) {
    refreshFieldValidation(event);
    clearStaleValidationSummary();

    if (!message.classList.contains("d-none")) {
      checkAnswers();
    }
  });

  form.addEventListener("change", function (event) {
    refreshFieldValidation(event);
    clearStaleValidationSummary();

    if (!message.classList.contains("d-none")) {
      checkAnswers();
    }
  });
});
