
document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("modalLoginForm");
    const loginError = document.getElementById("modalLoginError");

    const registerForm = document.getElementById("modalRegisterForm");
    const registerError = document.getElementById("modalRegisterError");

    // Login modal
    if (loginForm && loginError) {
        loginError.setAttribute("tabindex", "-1");
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
                loginError.focus();
                }
            } catch (error) {
                loginError.textContent =
                    "Something went wrong. Please try again.";

                loginError.classList.remove("d-none");
                loginError.focus();
            }
        });
    }

    // Register modal
    if (registerForm && registerError) {
        registerError.setAttribute("tabindex", "-1");
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
                    // The server has stored the success notification for the login page.
                    // Use the application's generated URL, preserving returnUrl.
                    if (result.loginUrl) {
                        window.location.assign(result.loginUrl);
                    } else {
                        registerError.textContent =
                            "Registration succeeded, but the login page could not be opened.";
                        registerError.classList.remove("d-none");
                    registerError.focus();
                    }
                } else {
                    // Show validation errors inside the Register modal.
                    registerError.textContent =
                        result.message || "Registration failed.";

                    registerError.classList.remove("d-none");
                    registerError.focus();
                }
            } catch (error) {
                registerError.textContent =
                    "Something went wrong. Please try again.";

                registerError.classList.remove("d-none");
                    registerError.focus();
            }
        });
    }
});
