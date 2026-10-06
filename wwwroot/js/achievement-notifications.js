document.addEventListener("DOMContentLoaded", () => {
    const notifications = Array.from(
        document.querySelectorAll(".achievement-notification")
    );

    if (notifications.length === 0) {
        return;
    }

    const container = document.querySelector(
        ".achievement-notification-container"
    );

    const antiForgeryToken = document.querySelector(
        "#achievement-notification-antiforgery " +
        "input[name='__RequestVerificationToken']"
    )?.value;

    const markSeenUrl = container.dataset.markSeenUrl;

    const maxVisible = 3;
    const timeBetweenNotifications = 2500;
    const displayTime = 6000;
    const highlightTime = 3000;

    let nextIndex = 0;
    let visibleCount = 0;

    async function markNotificationSeen(notification) {
        const userAchievementId =
            notification.dataset.userAchievementId;

        if (
            !userAchievementId ||
            !antiForgeryToken ||
            !markSeenUrl
        ) {
            return;
        }

        const formData = new URLSearchParams();

        formData.append(
            "userAchievementId",
            userAchievementId
        );

        formData.append(
            "__RequestVerificationToken",
            antiForgeryToken
        );

        try {
            const response = await fetch(markSeenUrl, {
                method: "POST",
                headers: {
                    "Content-Type":
                        "application/x-www-form-urlencoded"
                },
                body: formData,
                keepalive: true
            });

            if (!response.ok) {
                console.error(
                    "Could not mark achievement notification as seen."
                );
            }
        }
        catch (error) {
            console.error(
                "Could not mark achievement notification as seen.",
                error
            );
        }
    }

    function showNotification(notification) {
        visibleCount++;

        // New notifications are placed at the top.
        container.prepend(notification);
        notification.style.display = "flex";

        const closeButton = notification.querySelector(
            ".achievement-notification-close"
        );

        let timeoutId;
        let highlightTimeoutId;

        function closeNotification() {
            clearTimeout(timeoutId);
            clearTimeout(highlightTimeoutId);

            if (notification.classList.contains("closing")) {
                return;
            }

            notification.classList.add("closing");

            // Marks the notification as seen when it is closed.
            markNotificationSeen(notification);

            setTimeout(() => {
                notification.style.display = "none";
                visibleCount--;
            }, 300);
        }

        if (closeButton) {
            closeButton.addEventListener(
                "click",
                closeNotification
            );
        }

        // Changes to a neutral colour after the unlock highlight.
        highlightTimeoutId = setTimeout(() => {
            notification.classList.add("expiring");
        }, highlightTime);

        timeoutId = setTimeout(
            closeNotification,
            displayTime
        );
    }

    function tryShowNextNotification() {
        if (
            nextIndex >= notifications.length ||
            visibleCount >= maxVisible
        ) {
            return;
        }

        const notification = notifications[nextIndex];
        nextIndex++;

        showNotification(notification);
    }

    // Shows the first notification immediately.
    tryShowNextNotification();

    // Uses one fixed interval so new notifications
    // always appear at a consistent pace.
    const notificationInterval = setInterval(() => {
        if (nextIndex >= notifications.length) {
            clearInterval(notificationInterval);
            return;
        }

        tryShowNextNotification();
    }, timeBetweenNotifications);
});