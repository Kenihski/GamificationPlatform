document.addEventListener("DOMContentLoaded", function () {
    const questionType =
        document.getElementById("questionType");

    const correctOptions =
        document.querySelectorAll(".correct-option");

    const questionTypeHelp =
        document.getElementById("questionTypeHelp");

    const choiceAnswerSection =
        document.getElementById("choiceAnswerSection");

    const shortAnswerSection =
        document.getElementById("shortAnswerSection");

    if (!questionType) {
        return;
    }

    function updateQuestionType() {
        if (questionType.value === "ShortAnswer") {
            if (choiceAnswerSection) {
                choiceAnswerSection.style.display = "none";
            }

            if (shortAnswerSection) {
                shortAnswerSection.style.display = "block";
            }

            if (questionTypeHelp) {
                questionTypeHelp.textContent =
                    "Enter one or more accepted answers.";
            }

            return;
        }

        if (choiceAnswerSection) {
            choiceAnswerSection.style.display = "block";
        }

        if (shortAnswerSection) {
            shortAnswerSection.style.display = "none";
        }

        if (questionType.value === "SingleChoice") {
            if (questionTypeHelp) {
                questionTypeHelp.textContent =
                    "Select one correct answer.";
            }

            // Single Choice can only have one correct answer.
            let foundCheckedOption = false;

            correctOptions.forEach(function (option) {
                if (option.checked) {
                    if (foundCheckedOption) {
                        option.checked = false;
                    }

                    foundCheckedOption = true;
                }
            });
        }
        else if (questionType.value === "MultipleChoice") {
            if (questionTypeHelp) {
                questionTypeHelp.textContent =
                    "One or more answers may be correct.";
            }
        }
    }

    correctOptions.forEach(function (option) {
        option.addEventListener("change", function () {
            if (questionType.value !== "SingleChoice" ||
                !option.checked) {
                return;
            }

            // Uncheck all other answers for Single Choice.
            correctOptions.forEach(function (otherOption) {
                if (otherOption !== option) {
                    otherOption.checked = false;
                }
            });
        });
    });

    questionType.addEventListener(
        "change",
        updateQuestionType);

    updateQuestionType();
});