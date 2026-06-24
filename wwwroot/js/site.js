document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-app-toast]").forEach(toastElement => {
        bootstrap.Toast.getOrCreateInstance(toastElement).show();
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
                showCopyToast(button.dataset.copyErrorText || "Could not copy content.", "error");
                return;
            }

            const text = "value" in target
                ? target.value
                : (target.innerText || target.textContent || "");
            if (!text || !text.trim()) {
                showCopyToast(button.dataset.copyErrorText || "Could not copy content.", "error");
                return;
            }

            const originalHtml = button.innerHTML;
            button.disabled = true;

            try {
                await navigator.clipboard.writeText(text);
                button.textContent = button.dataset.copiedText || "Copied!";
                showCopyToast(button.dataset.copySuccessText || button.dataset.copiedText || "Copied!", "success");
            } catch {
                const errorText = button.dataset.copyErrorText || "Could not copy content.";
                button.textContent = errorText;
                showCopyToast(errorText, "error");
            }

            window.setTimeout(() => {
                button.innerHTML = originalHtml;
                button.disabled = false;
            }, 1500);
        });
    });

    document.querySelectorAll("[data-text-counter]").forEach(textArea => {
        const targetId = textArea.dataset.counterTarget;
        const counter = targetId ? document.getElementById(targetId) : null;

        const wordCount = getCounterElement(textArea.dataset.wordCountTarget, counter, "[data-word-count]");
        const characterCount = getCounterElement(textArea.dataset.charCountTarget, counter, "[data-character-count]");

        if (!wordCount && !characterCount) {
            return;
        }

        const updateCounter = () => {
            const text = textArea.value || "";
            const words = text.trim() ? (text.trim().match(/\S+/gu) || []).length : 0;

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

window.showToast = function showToast(message, type = "info", options = {}) {
    const container = document.getElementById("appToastContainer");
    if (!container || !message) {
        return null;
    }

    const normalizedType = ["success", "error", "warning", "info"].includes(type)
        ? type
        : "info";
    const title = options.title
        || container.dataset[`${normalizedType}Title`]
        || normalizedType;
    const closeLabel = container.dataset.closeLabel || "Close";
    const iconClasses = {
        success: "bi-check-circle-fill",
        error: "bi-x-circle-fill",
        warning: "bi-exclamation-triangle-fill",
        info: "bi-info-circle-fill"
    };

    const toastElement = document.createElement("div");
    toastElement.className = `toast app-toast app-toast-${normalizedType}`;
    toastElement.setAttribute("role", "alert");
    toastElement.setAttribute("aria-live", normalizedType === "error" ? "assertive" : "polite");
    toastElement.setAttribute("aria-atomic", "true");

    const header = document.createElement("div");
    header.className = "toast-header";

    const icon = document.createElement("i");
    icon.className = `bi ${iconClasses[normalizedType]} app-toast-icon me-2`;
    icon.setAttribute("aria-hidden", "true");

    const titleElement = document.createElement("strong");
    titleElement.className = "me-auto";
    titleElement.textContent = title;

    const closeButton = document.createElement("button");
    closeButton.type = "button";
    closeButton.className = "btn-close";
    closeButton.dataset.bsDismiss = "toast";
    closeButton.setAttribute("aria-label", closeLabel);

    const body = document.createElement("div");
    body.className = "toast-body";
    body.textContent = message;

    header.append(icon, titleElement, closeButton);
    toastElement.append(header, body);
    container.appendChild(toastElement);

    const toast = bootstrap.Toast.getOrCreateInstance(toastElement, {
        autohide: options.autohide !== false,
        delay: options.delay || 5000
    });

    toastElement.addEventListener("hidden.bs.toast", () => toastElement.remove(), { once: true });
    toast.show();
    return toast;
};

function showCopyToast(message, type) {
    if (typeof window.showToast === "function") {
        window.showToast(message, type);
    }
}

function getCounterElement(targetId, container, selector) {
    if (targetId) {
        return document.getElementById(targetId);
    }

    return container ? container.querySelector(selector) : null;
}

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
