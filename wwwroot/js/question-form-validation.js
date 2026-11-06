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

    return errors;
  }

  // Keep validation messages in the same order as the form:
  // Title, Points, then answer configuration. Do not invoke jQuery's
  // validator.element()/form.valid() on every keystroke (it steals focus).
  form.noValidate = true;
  const clientSummary = document.createElement("div");
  clientSummary.className = "alert alert-danger d-none";
  clientSummary.setAttribute("role", "alert");
  clientSummary.tabIndex = -1;
  clientSummary.id = "questionFormValidationSummary";
  form.prepend(clientSummary);

  let attemptedSubmit = false;

  function validateForm() {
    const errors = [];
    const title = form.querySelector('[name="Question.Title"]');
    const points = form.querySelector('[name="Question.Points"]');
    if (!title?.value.trim()) {
      errors.push("Question title is required.");
    }
    if (
      !points?.value.trim() ||
      !Number.isInteger(Number(points.value)) ||
      Number(points.value) < 1 ||
      Number(points.value) > 100
    ) {
      errors.push("Points must be between 1 and 100.");
    }
    errors.push(...checkAnswers());
    return errors;
  }

  function showErrors(errors) {
    clientSummary.replaceChildren();
    if (errors.length) {
      const heading = document.createElement("strong");
      heading.textContent = "Please correct the following errors:";
      const list = document.createElement("ul");
      list.className = "mb-0 mt-2";
      for (const error of errors) {
        const item = document.createElement("li");
        item.textContent = error;
        list.append(item);
      }
      clientSummary.append(heading, list);
    }
    clientSummary.classList.toggle("d-none", errors.length === 0);
    // The separate answer alert is replaced by the ordered summary.
    message.classList.add("d-none");
    message.textContent = "";
  }

  form.addEventListener("submit", function (event) {
    attemptedSubmit = true;
    const errors = validateForm();
    showErrors(errors);
    if (errors.length) {
      event.preventDefault();
      event.stopImmediatePropagation();
      clientSummary.focus({ preventScroll: true });
      clientSummary.scrollIntoView({ behavior: "smooth", block: "start" });
    }
  }, true);

  function updateSummary() {
    if (attemptedSubmit) showErrors(validateForm());
  }

  form.addEventListener("input", updateSummary);
  form.addEventListener("change", updateSummary);
});
