document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById("challengeForm");

    if (!form) {
        return;
    }


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


            // Time has expired
            if (remaining <= 0) {

                timeRemaining.textContent =
                    "00:00";

                timeExpired = true;

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
    // Submit confirmation
    // --------------------------------

    form.addEventListener("submit", function (event) {

        // Do not show confirmation when
        // the timer submits automatically.
        if (timeExpired) {
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
            }
        }

    });

});