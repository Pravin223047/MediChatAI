// Modal Focus Management
// Handles keyboard navigation and focus trapping for accessible modals

/**
 * Focuses the first focusable element within a modal
 * @param {string} modalId - The ID of the modal element
 */
window.focusFirstElement = function(modalId) {
    try {
        const modal = document.getElementById(modalId);
        if (!modal) {
            console.warn(`Modal with ID "${modalId}" not found`);
            return;
        }

        // Find all focusable elements
        const focusableSelector = 'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';
        const focusableElements = modal.querySelectorAll(focusableSelector);

        if (focusableElements.length > 0) {
            // Focus the first focusable element
            focusableElements[0].focus();
        }
    } catch (error) {
        console.error('Error focusing first element:', error);
    }
};

/**
 * Creates a focus trap within a modal to prevent Tab key from escaping
 * @param {string} modalId - The ID of the modal element
 * @returns {Function} Cleanup function to remove event listeners
 */
window.createFocusTrap = function(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) {
        console.warn(`Modal with ID "${modalId}" not found`);
        return () => {};
    }

    const focusableSelector = 'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

    const handleKeyDown = function(event) {
        // Only handle Tab key
        if (event.key !== 'Tab') return;

        const focusableElements = Array.from(modal.querySelectorAll(focusableSelector));
        if (focusableElements.length === 0) return;

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];
        const activeElement = document.activeElement;

        // Shift + Tab on first element -> focus last
        if (event.shiftKey && activeElement === firstElement) {
            event.preventDefault();
            lastElement.focus();
        }
        // Tab on last element -> focus first
        else if (!event.shiftKey && activeElement === lastElement) {
            event.preventDefault();
            firstElement.focus();
        }
    };

    // Add event listener
    modal.addEventListener('keydown', handleKeyDown);

    // Return cleanup function
    return function() {
        modal.removeEventListener('keydown', handleKeyDown);
    };
};

/**
 * Restores focus to the element that triggered the modal
 * @param {HTMLElement} element - The element to restore focus to
 */
window.restoreFocus = function(element) {
    if (element && typeof element.focus === 'function') {
        element.focus();
    }
};

/**
 * Outside Click Handler for Modals
 * Registers a click handler to detect clicks outside of specific elements
 */
let outsideClickHandlers = {};

/**
 * Register outside click handler for a modal/dropdown
 * @param {string} triggerId - The ID of the trigger button element
 * @param {string} modalId - The ID of the modal/dropdown element
 * @param {object} dotNetHelper - The .NET object reference to call back
 */
window.registerOutsideClickHandler = function(triggerId, modalId, dotNetHelper) {
    // Remove existing handler if present
    if (outsideClickHandlers[modalId]) {
        document.removeEventListener('click', outsideClickHandlers[modalId]);
    }

    const handler = function(event) {
        const trigger = document.getElementById(triggerId);
        const modal = document.getElementById(modalId);

        if (!trigger || !modal) {
            return;
        }

        // Check if click is outside both trigger and modal
        const clickedOutside = !trigger.contains(event.target) && !modal.contains(event.target);

        if (clickedOutside) {
            // Call back to .NET to close the modal
            dotNetHelper.invokeMethodAsync('CloseModal');
        }
    };

    // Store handler reference
    outsideClickHandlers[modalId] = handler;

    // Add event listener with a small delay to prevent immediate trigger
    setTimeout(() => {
        document.addEventListener('click', handler);
    }, 100);
};

/**
 * Unregister outside click handler
 * @param {string} modalId - The ID of the modal element
 */
window.unregisterOutsideClickHandler = function(modalId) {
    if (outsideClickHandlers[modalId]) {
        document.removeEventListener('click', outsideClickHandlers[modalId]);
        delete outsideClickHandlers[modalId];
    }
};

console.log('Modal focus management and outside-click handlers loaded');
