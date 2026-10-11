
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

        context.clearRect(
            0,
            0,
            canvas.width,
            canvas.height
        );

        if (!reducedMotion.matches) {
            animationId = requestAnimationFrame(draw);
        }
    }

    window.addEventListener("resize", function () {
        resizeCanvas();
        startAnimation();
    });

    reducedMotion.addEventListener("change", startAnimation);

    resizeCanvas();
    startAnimation();
});
