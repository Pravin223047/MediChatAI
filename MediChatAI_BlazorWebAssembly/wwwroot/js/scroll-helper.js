/**
 * Scroll Helper - Provides smooth scrolling utilities for chat interface
 */
window.scrollHelper = {
    /**
     * Scrolls an element to the bottom with smooth animation
     * @param {HTMLElement} element - The element to scroll
     */
    scrollToBottom: function(element) {
        if (!element) {
            console.warn('ScrollHelper: Element is null or undefined');
            return;
        }

        try {
            // Use smooth scroll behavior for better UX
            element.scrollTo({
                top: element.scrollHeight,
                behavior: 'smooth'
            });
        } catch (error) {
            // Fallback for browsers that don't support smooth scrolling
            console.warn('ScrollHelper: Smooth scroll not supported, using fallback');
            element.scrollTop = element.scrollHeight;
        }
    },

    /**
     * Scrolls to a specific element within a container
     * @param {HTMLElement} container - The scrollable container
     * @param {HTMLElement} targetElement - The element to scroll to
     */
    scrollToElement: function(container, targetElement) {
        if (!container || !targetElement) {
            console.warn('ScrollHelper: Container or target element is null');
            return;
        }

        try {
            targetElement.scrollIntoView({
                behavior: 'smooth',
                block: 'nearest'
            });
        } catch (error) {
            console.warn('ScrollHelper: scrollIntoView failed', error);
        }
    },

    /**
     * Checks if an element is scrolled to the bottom
     * @param {HTMLElement} element - The element to check
     * @param {number} threshold - Pixel threshold to consider "at bottom" (default: 50)
     * @returns {boolean} True if element is at or near bottom
     */
    isAtBottom: function(element, threshold = 50) {
        if (!element) {
            return false;
        }

        const scrollTop = element.scrollTop;
        const scrollHeight = element.scrollHeight;
        const clientHeight = element.clientHeight;

        return (scrollHeight - scrollTop - clientHeight) <= threshold;
    }
};
