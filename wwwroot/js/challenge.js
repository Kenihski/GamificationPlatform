document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("challengeForm");

    if (!form) {
        return;
    }

    // --------------------------------
    // Saves selected and written answers
    // --------------------------------

    const challengeAttemptId =
        form.querySelector(
            'input[name="challengeAttemptId"]'
        ).value;

    // Each attempt gets its own storage key.
    const storageKey =
        "challengeAnswers_" + challengeAttemptId;

    // Loads previously saved answers.
    const savedAnswers =
        JSON.parse(
            sessionStorage.getItem(storageKey) || "{}"
        );

    const choiceInputs =
        form.querySelectorAll(
            'input[type="radio"], input[type="checkbox"]'
        );

    const shortAnswerInputs =
        form.querySelectorAll(
            ".short-answer"
        );

    // Restores choice answers after refresh.
    choiceInputs.forEach(function (answerInput) {
        const questionName =
            answerInput.name;

        if (answerInput.type === "radio") {
            if (savedAnswers[questionName] ===
                answerInput.value) {
                answerInput.checked = true;
            }
        }
        else {
            const selectedValues =
                savedAnswers[questionName] || [];

            answerInput.checked =
                selectedValues.includes(
                    answerInput.value
                );
        }

        // Saves an answer when the user
        // selects or deselects an option.
        answerInput.addEventListener(
            "change",
            function () {
                if (this.type === "radio") {
                    savedAnswers[this.name] =
                        this.value;
                }
                else {
                    const checkedOptions =
                        form.querySelectorAll(
                            'input[type="checkbox"]' +
                            '[name="' + this.name + '"]:checked'
                        );

                    savedAnswers[this.name] =
                        Array.from(checkedOptions)
                            .map(function (option) {
                                return option.value;
                            });
                }

                sessionStorage.setItem(
                    storageKey,
                    JSON.stringify(savedAnswers)
                );
            }
        );
    });

    // Restores Short Answer text after refresh.
    shortAnswerInputs.forEach(function (answerInput) {
        if (savedAnswers[answerInput.name] !== undefined) {
            answerInput.value =
                savedAnswers[answerInput.name];
        }

        // Saves the text while the user types.
        answerInput.addEventListener(
            "input",
            function () {
                savedAnswers[this.name] =
                    this.value;

                sessionStorage.setItem(
                    storageKey,
                    JSON.stringify(savedAnswers)
                );
            }
        );
    });

    // --------------------------------
    // Challenge timer
    // --------------------------------

    const timeRemaining =
        document.getElementById("timeRemaining");

    const startedAt =
        form.dataset.startedAt;

    const timeLimit =
        form.dataset.timeLimit;

    let timeExpired = false;

    if (timeRemaining &&
        startedAt &&
        timeLimit) {

        const startTime =
            new Date(startedAt).getTime();

        const timeLimitMilliseconds =
            Number(timeLimit) * 60 * 1000;

        const endTime =
            startTime + timeLimitMilliseconds;

        function updateTimer() {
            const now =
                new Date().getTime();

            const remaining =
                endTime - now;

            // Time has expired.
            if (remaining <= 0) {
                timeRemaining.textContent =
                    "00:00";

                timeExpired = true;

                // Removes the saved answers
                // because the attempt is ending.
                sessionStorage.removeItem(
                    storageKey
                );

                form.submit();
                return;
            }

            const totalSeconds =
                Math.floor(remaining / 1000);

            const minutes =
                Math.floor(totalSeconds / 60);

            const seconds =
                totalSeconds % 60;

            timeRemaining.textContent =
                String(minutes).padStart(2, "0") +
                ":" +
                String(seconds).padStart(2, "0");
        }

        // Shows the timer immediately.
        updateTimer();

        // Updates the timer every second.
        setInterval(updateTimer, 1000);
    }

    // --------------------------------
    // Exit confirmation
    // --------------------------------

    const exitButton =
        document.getElementById("exitChallengeButton");

    if (exitButton) {
        exitButton.addEventListener(
            "click",
            function (event) {
                const confirmed = confirm(
                    "Are you sure you want to exit the challenge?\n\n" +
                    "Your attempt will be submitted and unanswered " +
                    "questions will receive 0 points."
                );

                if (!confirmed) {
                    event.preventDefault();
                }
            }
        );
    }

    // --------------------------------
    // Submit confirmation
    // --------------------------------

    form.addEventListener("submit", function (event) {
        // Do not show confirmation when
        // the timer submits automatically.
        if (timeExpired) {
            return;
        }

        // Exit has its own confirmation.
        if (event.submitter &&
            event.submitter.id ===
                "exitChallengeButton") {

            // The attempt is being ended,
            // so temporary answers are removed.
            sessionStorage.removeItem(
                storageKey
            );

            return;
        }

        const questions =
            form.querySelectorAll(".question-card");

        let unanswered = 0;

        questions.forEach(function (question) {
            const selectedChoice =
                question.querySelector(
                    'input[type="radio"]:checked, ' +
                    'input[type="checkbox"]:checked'
                );

            const shortAnswer =
                question.querySelector(
                    ".short-answer"
                );

            let isAnswered = false;

            if (selectedChoice) {
                isAnswered = true;
            }

            if (shortAnswer &&
                shortAnswer.value.trim() !== "") {
                isAnswered = true;
            }

            if (!isAnswered) {
                unanswered++;
            }
        });

        if (unanswered > 0) {
            const confirmed = confirm(
                "You have " + unanswered +
                " unanswered question(s).\n\n" +
                "Unanswered questions will receive 0 points.\n\n" +
                "Do you still want to submit?"
            );

            if (!confirmed) {
                event.preventDefault();
                return;
            }
        }

        // The attempt is being submitted,
        // so the temporary answers are removed.
        sessionStorage.removeItem(
            storageKey
        );
    });
});