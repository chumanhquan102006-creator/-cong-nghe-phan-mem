document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".alert-dismissible").forEach(alertElement => {
        window.setTimeout(() => {
            const alert = bootstrap.Alert.getOrCreateInstance(alertElement);
            alert.close();
        }, 5000);
    });
});
