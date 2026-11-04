// Additional immediate checks; ASP.NET Core remains authoritative.
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("form").forEach(function (form) {
        const username = form.querySelector('input[name="Username"]');
        const email = form.querySelector('input[name="Email"]');
        const password = form.querySelector('input[name="Password"]');
        const confirmation = form.querySelector('input[name="ConfirmPassword"]');
        if (!username || !email || !password || !confirmation) return;

        // Collect field errors together instead of the browser stopping at the first one.
        form.noValidate = true;

        // Share the MVC summary so server and client errors never use separate boxes.
        const summary = form.querySelector("[data-valmsg-summary]");
        const existing = form.querySelector("#modalRegisterError") || summary;
        const message = existing || document.createElement("div");
        if (!existing) {
            message.className = "alert alert-danger d-none";
            message.setAttribute("role", "alert");
            form.prepend(message);
        }

        // Reuse the page's field messages and create matching ones for the modal.
        const fieldMessages = new Map();
        [username, email, password, confirmation].forEach(function (field) {
            let fieldMessage = form.querySelector(`[data-valmsg-for="${field.name}"]`);
            if (!fieldMessage) {
                fieldMessage = document.createElement("span");
                fieldMessage.className = "text-danger";
                field.parentElement.appendChild(fieldMessage);
            }
            fieldMessage.id = fieldMessage.id || `${field.id}-error`;
            const descriptions = (field.getAttribute("aria-describedby") || "")
                .split(/\s+/).filter(Boolean);
            if (!descriptions.includes(fieldMessage.id)) descriptions.push(fieldMessage.id);
            field.setAttribute("aria-describedby", descriptions.join(" "));
            fieldMessages.set(field, fieldMessage);
        });

        function clearFieldError(field) {
            fieldMessages.get(field).textContent = "";
            field.removeAttribute("aria-invalid");
        }

        function checkRegistration() {
            const name = username.value.trim();
            const secret = password.value;
            const errors = [];
            fieldMessages.forEach(function (_, field) { clearFieldError(field); });

            // Show the same error beside its field and in the summary.
            function addError(field, text) {
                errors.push(text);
                fieldMessages.get(field).textContent = text;
                field.setAttribute("aria-invalid", "true");
            }
            if (!name) {
                addError(username, "Username is required.");
            } else if (name.length < 3 || name.length > 50) {
                addError(username, "Username must be between 3 and 50 characters.");
            }
            if (!email.value.trim()) {
                addError(email, "Email is required.");
            } else if (email.validity.typeMismatch) {
                addError(email, "Please enter a valid email address.");
            } else if (email.value.length > 100) {
                addError(email, "Email cannot be longer than 100 characters.");
            }
            if (!secret.trim()) {
                addError(password, "Password is required.");
            } else if (secret.length < 6 || secret.length > 100) {
                addError(password, "Password must be between 6 and 100 characters.");
            } else if (secret.toLowerCase() === name.toLowerCase()) {
                addError(password, "Username and password cannot be the same.");
            }
            if (!confirmation.value.trim()) {
                addError(confirmation, "Please confirm your password.");
            } else if (confirmation.value !== secret) {
                addError(confirmation, "Passwords do not match.");
            }
            if (errors.length) {
                const list = document.createElement("ul");
                list.className = "mb-0";
                errors.forEach(function (error) {
                    const item = document.createElement("li");
                    item.textContent = error;
                    list.appendChild(item);
                });
                message.replaceChildren(list);
                message.classList.remove("d-none");
                if (summary) {
                    message.classList.remove("validation-summary-valid");
                    message.classList.add("validation-summary-errors", "alert", "alert-danger");
                }
                // Bring keyboard focus to the existing registration error message.
                message.setAttribute("tabindex", "-1");
                message.focus();
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

        form.addEventListener("input", function (event) {
            if (fieldMessages.has(event.target)) clearFieldError(event.target);
            if (summary) {
                // Keep the list element that jQuery uses when displaying its errors.
                message.replaceChildren(document.createElement("ul"));
                message.classList.remove("validation-summary-errors", "alert", "alert-danger");
                message.classList.add("validation-summary-valid");
            } else if (!message.classList.contains("d-none")) {
                message.classList.add("d-none");
                message.textContent = "";
            }
        });
    });
});
