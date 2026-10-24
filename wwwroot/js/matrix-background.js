
document.addEventListener("DOMContentLoaded", function () {
    const canvas = document.getElementById("matrixBackground");

    if (!canvas) {
        return;
    }

    const context = canvas.getContext("2d");

    if (!context) {
        return;
    }

    const reducedMotion = window.matchMedia(
        "(prefers-reduced-motion: reduce)"
    );
    const toggle = document.getElementById("matrixToggle");
    const icon = document.getElementById("matrixIcon");
    const tooltip = document.getElementById("matrixTooltip");
    const defaultOff = canvas.dataset.defaultOff === "true";
    const storageKey = defaultOff
        ? "matrix-challenge-animation-enabled"
        : "matrix-animation-enabled";
    let savedPreference = null;

    try {
        savedPreference = localStorage.getItem(storageKey);
    } catch {
        // The control still works when browser storage is unavailable.
    }

    let enabled = savedPreference === "true" ||
        (savedPreference !== "false" && !defaultOff && !reducedMotion.matches);

    const characters = "01{}[]();<>/=#@$";
    const fontSize = 16;

    let columns = [];
    let animationId;
    let lastFrame = 0;

    function resizeCanvas() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;

        const columnCount = Math.ceil(canvas.width / fontSize);

        columns = Array.from({ length: columnCount }, function () {
            return {
                y: Math.random() * -canvas.height,
                speed: 1 + Math.random() * 2,
                color: Math.random() > 0.5
                    ? "#587cff"
                    : "#a56aff"
            };
        });
    }

    function draw(timestamp) {
        animationId = requestAnimationFrame(draw);

        if (timestamp - lastFrame < 50) {
            return;
        }

        lastFrame = timestamp;

        context.fillStyle = "rgba(7, 10, 25, 0.08)";
        context.fillRect(
            0,
            0,
            canvas.width,
            canvas.height
        );

        context.font = fontSize + "px monospace";

        columns.forEach(function (column, index) {
            const character = characters.charAt(
                Math.floor(Math.random() * characters.length)
            );

            context.fillStyle = column.color;
            context.shadowColor = column.color;
            context.shadowBlur = 8;

            context.fillText(
                character,
                index * fontSize,
                column.y
            );

            context.shadowBlur = 0;

            column.y += column.speed * fontSize;

            if (column.y > canvas.height &&
                Math.random() > 0.95) {
                column.y = -fontSize;
            }
        });
    }

    function startAnimation() {
        cancelAnimationFrame(animationId);
        lastFrame = 0;

        context.clearRect(
            0,
            0,
            canvas.width,
            canvas.height
        );

        if (enabled) {
            animationId = requestAnimationFrame(draw);
        }

        if (toggle && icon && tooltip) {
            toggle.hidden = false;
            const action = enabled
                ? "Pause background animation"
                : "Enable background animation";
            toggle.setAttribute("aria-label", action);
            tooltip.textContent = action;
            icon.setAttribute("d", enabled
                ? "M6 5h4v14H6zM14 5h4v14h-4z"
                : "M8 5v14l11-7z");
        }
    }

    toggle?.addEventListener("click", function () {
        enabled = !enabled;
        savedPreference = String(enabled);

        try {
            localStorage.setItem(storageKey, savedPreference);
        } catch {
            // Keep the current page usable even without persistent storage.
        }

        startAnimation();
    });

    // Keep the visual label available on hover/focus, and dismissible with Escape.
    const control = toggle?.closest(".matrix-control");
    control?.addEventListener("mouseenter", function () {
        control.classList.remove("tooltip-dismissed");
    });
    control?.addEventListener("focusin", function () {
        control.classList.remove("tooltip-dismissed");
    });
    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape") {
            control?.classList.add("tooltip-dismissed");
        }
    });

    window.addEventListener("resize", function () {
        resizeCanvas();
        startAnimation();
    });

    reducedMotion.addEventListener("change", function () {
        if (savedPreference !== "true" && savedPreference !== "false") {
            enabled = !defaultOff && !reducedMotion.matches;
            startAnimation();
        }
    });

    resizeCanvas();
    startAnimation();
});
