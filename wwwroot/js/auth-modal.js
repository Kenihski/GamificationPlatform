
document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("modalLoginForm");
    const loginError = document.getElementById("modalLoginError");

    const registerForm = document.getElementById("modalRegisterForm");
    const registerError = document.getElementById("modalRegisterError");

    // Login modal
    if (loginForm && loginError) {
        loginForm.addEventListener("submit", async function (event) {
            event.preventDefault();

            loginError.classList.add("d-none");
            loginError.textContent = "";

            try {
                const response = await fetch(loginForm.action, {
                    method: "POST",
                    body: new FormData(loginForm)
                });

                if (!response.ok) {
                    throw new Error("Login request failed.");
                }

                const result = await response.json();

                if (result.success) {
                    // Stay on the current page.
                    window.location.reload();
                } else {
                    loginError.textContent =
                        result.message || "Login failed.";

                    loginError.classList.remove("d-none");
                }
            } catch (error) {
                loginError.textContent =
                    "Something went wrong. Please try again.";

                loginError.classList.remove("d-none");
            }
        });
    }

    // Register modal
    if (registerForm && registerError) {
        registerForm.addEventListener("submit", async function (event) {
            event.preventDefault();

            registerError.classList.add("d-none");
            registerError.textContent = "";

            try {
                const response = await fetch(registerForm.action, {
                    method: "POST",
                    body: new FormData(registerForm)
                });

                if (!response.ok) {
                    throw new Error("Registration request failed.");
                }

                const result = await response.json();

                if (result.success) {
                    // Fill in the username in the Login modal.
                    const registerUsername =
                        document.getElementById("modalRegisterUsername");

                    const loginUsername =
                        document.getElementById("modalLoginUsername");

                    if (registerUsername && loginUsername) {
                        loginUsername.value = registerUsername.value.trim();
                    }

                    // Clear the registration form.
                    registerForm.reset();

                    // Keep registration feedback visible until the user chooses to log in.
                    const registerModal =
                        document.getElementById("registerModal");
                    const loginModal =
                        document.getElementById("loginModal");

                    const successMessage = document.createElement("div");
                    successMessage.className = "alert alert-success";
                    successMessage.setAttribute("role", "status");
                    successMessage.textContent =
                        "Registration successful! Your account has been created. You can now log in.";

                    const loginButton = document.createElement("button");
                    loginButton.type = "button";
                    loginButton.className = "btn btn-primary mt-2";
                    loginButton.textContent = "Log In";
                    loginButton.addEventListener("click", function () {
                        if (registerModal && loginModal) {
                            registerModal.addEventListener(
                                "hidden.bs.modal",
                                function () {
                                    successMessage.remove();
                                    registerForm.classList.remove("d-none");
                                    bootstrap.Modal
                                        .getOrCreateInstance(loginModal)
                                        .show();
                                },
                                { once: true }
                            );
                            bootstrap.Modal
                                .getOrCreateInstance(registerModal)
                                .hide();
                        }
                    });
                    successMessage.appendChild(document.createElement("br"));
                    successMessage.appendChild(loginButton);
                    registerForm.before(successMessage);
                    registerForm.classList.add("d-none");

                    if (registerModal) {
                        registerModal.addEventListener("hidden.bs.modal", function () {
                            successMessage.remove();
                            registerForm.classList.remove("d-none");
                        }, { once: true });
                    }
                } else {
                    // Show validation errors inside the Register modal.
                    registerError.textContent =
                        result.message || "Registration failed.";

                    registerError.classList.remove("d-none");
                }
            } catch (error) {
                registerError.textContent =
                    "Something went wrong. Please try again.";

                registerError.classList.remove("d-none");
            }
        });
    }
});
