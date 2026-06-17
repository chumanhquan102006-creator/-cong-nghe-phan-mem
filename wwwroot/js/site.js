document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".alert-dismissible").forEach(alertElement => {
        window.setTimeout(() => {
            const alert = bootstrap.Alert.getOrCreateInstance(alertElement);
            alert.close();
        }, 5000);
    });

    document.querySelectorAll(".js-loading-form").forEach(form => {
        form.addEventListener("submit", event => {
            if (!isFormSubmittable(form)) {
                return;
            }

            const submitButton = getSubmitButton(event, form);
            if (!submitButton) {
                return;
            }

            if (submitButton.dataset.loadingApplied === "true") {
                event.preventDefault();
                return;
            }

            applyLoadingState(submitButton);
        });
    });
});

function isFormSubmittable(form) {
    if (typeof form.checkValidity === "function" && !form.checkValidity()) {
        return false;
    }

    if (typeof window.jQuery !== "undefined") {
        const jqueryForm = window.jQuery(form);
        if (typeof jqueryForm.valid === "function" && !jqueryForm.valid()) {
            return false;
        }
    }

    return true;
}

function getSubmitButton(event, form) {
    if (event.submitter instanceof HTMLButtonElement || event.submitter instanceof HTMLInputElement) {
        return event.submitter;
    }

    return form.querySelector("button[type='submit'], input[type='submit']");
}

function applyLoadingState(submitButton) {
    const loadingText = submitButton.dataset.loadingText || "Processing...";
    const originalHtml = submitButton.innerHTML;

    submitButton.dataset.originalHtml = originalHtml;
    submitButton.dataset.loadingApplied = "true";
    submitButton.disabled = true;
    submitButton.innerHTML = `
        <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
        ${loadingText}
    `;
}
