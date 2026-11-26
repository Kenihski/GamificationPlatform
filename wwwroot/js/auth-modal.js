
document.addEventListener("DOMContentLoaded", function () {
    const loginForm = document.getElementById("modalLoginForm");
    const loginError = document.getElementById("modalLoginError");
    const loginModal = document.getElementById("loginModal");

    // Focus username after Bootstrap has activated the login dialog.
    if (loginModal) {
        loginModal.addEventListener("shown.bs.modal", function () {
            document.getElementById("modalLoginUsername")?.focus();
        });
    }

    const registerForm = document.getElementById("modalRegisterForm");
    const registerError = document.getElementById("modalRegisterError");
    const registerModal = document.getElementById("registerModal");
    const loginSuccess = document.getElementById("modalLoginSuccess");

    function openLoginAfterRegistration(username, message, returnUrl) {
        if (!loginModal || !loginForm || !loginSuccess) return;
        loginForm.reset();
        document.getElementById("modalLoginUsername").value = username || "";
        document.getElementById("modalLoginReturnUrl").value = returnUrl || "";
        loginError.textContent = "";
        loginError.classList.add("d-none");
        loginSuccess.textContent = message;
        loginSuccess.classList.remove("d-none");
        const showLogin = () => bootstrap.Modal.getOrCreateInstance(loginModal).show();
        if (registerModal?.classList.contains("show")) {
            registerModal.addEventListener("hidden.bs.modal", showLogin, { once: true });
            bootstrap.Modal.getOrCreateInstance(registerModal).hide();
        } else {
            showLogin();
        }
    }

    loginModal?.addEventListener("hidden.bs.modal", function () {
        loginSuccess?.classList.add("d-none");
        if (loginSuccess) loginSuccess.textContent = "";
        loginForm?.reset();
    });

    if (loginModal?.dataset.registrationMessage) {
        openLoginAfterRegistration(
            loginModal.dataset.registeredUsername,
            loginModal.dataset.registrationMessage,
            loginModal.dataset.registrationReturnUrl
        );
    }

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
                    if (result.redirectUrl) window.location.assign(result.redirectUrl);
                    else window.location.reload();
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
                    registerForm.reset();
                    openLoginAfterRegistration(result.username, result.message, result.returnUrl);
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
