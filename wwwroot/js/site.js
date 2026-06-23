document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".alert-dismissible").forEach(alertElement => {
        window.setTimeout(() => {
            const alert = bootstrap.Alert.getOrCreateInstance(alertElement);
            alert.close();
        }, 5000);
    });

    document.querySelectorAll(".js-confirm-form").forEach(form => {
        form.addEventListener("submit", event => {
            const message = form.dataset.confirmMessage || "Are you sure?";
            if (!window.confirm(message)) {
                event.preventDefault();
            }
        });
    });

    document.querySelectorAll("[data-copy-target]").forEach(button => {
        button.addEventListener("click", async () => {
            const targetId = button.dataset.copyTarget;
            const target = targetId ? document.getElementById(targetId) : null;
            if (!target) {
                return;
            }

            const text = "value" in target ? target.value : target.innerText;
            if (!text || !text.trim()) {
                return;
            }

            const originalHtml = button.innerHTML;
            try {
                await navigator.clipboard.writeText(text);
                button.textContent = button.dataset.copiedText || "Copied!";
            } catch {
                button.textContent = button.dataset.copyErrorText || "Copy failed";
            }

            window.setTimeout(() => {
                button.innerHTML = originalHtml;
            }, 1500);
        });
    });

    document.querySelectorAll("[data-text-counter]").forEach(textArea => {
        const targetId = textArea.dataset.counterTarget;
        const counter = targetId ? document.getElementById(targetId) : null;
        if (!counter) {
            return;
        }

        const wordCount = counter.querySelector("[data-word-count]");
        const characterCount = counter.querySelector("[data-character-count]");

        const updateCounter = () => {
            const text = textArea.value || "";
            const words = text.trim() ? text.trim().split(/\s+/u).length : 0;

            if (wordCount) {
                wordCount.textContent = words.toString();
            }

            if (characterCount) {
                characterCount.textContent = text.length.toString();
            }
        };

        textArea.addEventListener("input", updateCounter);
        updateCounter();
    });

    document.querySelectorAll(".js-loading-form").forEach(form => {
        form.addEventListener("submit", event => {
            if (event.defaultPrevented) {
                return;
            }

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
