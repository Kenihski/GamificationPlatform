// Additional immediate checks; ASP.NET Core remains authoritative.
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("form").forEach(function (form) {
        const username = form.querySelector('input[name="Username"]');
        const email = form.querySelector('input[name="Email"]');
        const password = form.querySelector('input[name="Password"]');
        const confirmation = form.querySelector('input[name="ConfirmPassword"]');
        if (!username || !email || !password || !confirmation) return;

        const existing = form.querySelector("#modalRegisterError");
        const message = existing || document.createElement("div");
        if (!existing) {
            message.className = "alert alert-danger d-none";
            message.setAttribute("role", "alert");
            const summary = form.querySelector("[data-valmsg-summary]");
            if (summary) summary.insertAdjacentElement("afterend", message);
            else form.prepend(message);
        }

        function checkRegistration() {
            const name = username.value.trim();
            const secret = password.value;
            let error = "";
            if (name.length < 3 || name.length > 50) {
                error = "Username must be between 3 and 50 characters.";
            } else if (secret.length < 6 || secret.length > 100) {
                error = "Password must be between 6 and 100 characters.";
            } else if (secret.toLowerCase() === name.toLowerCase()) {
                error = "Username and password cannot be the same.";
            } else if (confirmation.value !== secret) {
                error = "Passwords do not match.";
            }
            if (error) {
                message.textContent = error;
                message.classList.remove("d-none");
                return false;
            }
            return true;
        }

        // Capture phase prevents AJAX registration on invalid input.
        form.addEventListener("submit", function (event) {
            if (!checkRegistration()) {
                event.preventDefault();
                event.stopImmediatePropagation();
            }
        }, true);

        form.addEventListener("input", function () {
            if (!message.classList.contains("d-none")) {
                message.classList.add("d-none");
                message.textContent = "";
            }
        });
    });
});
