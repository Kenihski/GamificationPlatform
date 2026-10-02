document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById("challengeForm");

    if (!form) {
        return;
    }


    // --------------------------------
    // Saves selected answers
    // --------------------------------

    const challengeAttemptId =
        form.querySelector(
            'input[name="challengeAttemptId"]'
        ).value;

    // Each attempt gets its own storage key.
    const storageKey =
        "challengeAnswers_" + challengeAttemptId;


    // Loads previously selected answers.
    const savedAnswers =
        JSON.parse(
            sessionStorage.getItem(storageKey) || "{}"
        );


    const radioButtons =
        form.querySelectorAll(
            'input[type="radio"]'
        );


    // Restores answers after refresh.
    radioButtons.forEach(function (radioButton) {

        const questionName =
            radioButton.name;

        if (savedAnswers[questionName] ===
            radioButton.value) {

            radioButton.checked = true;
        }


        // Saves an answer when the user
        // selects an option.
        radioButton.addEventListener(
            "change",
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

            const selectedAnswer =
                question.querySelector(
                    'input[type="radio"]:checked'
                );

            if (!selectedAnswer) {
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