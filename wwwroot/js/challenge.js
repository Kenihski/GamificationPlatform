document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById("challengeForm");

    if (!form) {
        return;
    }

    form.addEventListener("submit", function (event) {

        const questions = form.querySelectorAll(".question-card");

        let unanswered = 0;

        questions.forEach(function (question) {

            const selectedAnswer =
                question.querySelector('input[type="radio"]:checked');

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