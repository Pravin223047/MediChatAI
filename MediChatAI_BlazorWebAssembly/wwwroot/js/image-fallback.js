/**
 * Image Fallback Handler
 * Handles profile image loading failures and displays default icons
 */

// Default profile icon SVG (user circle)
const DEFAULT_PROFILE_ICON = `
<svg xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 24 24" class="w-full h-full">
    <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 3c1.66 0 3 1.34 3 3s-1.34 3-3 3-3-1.34-3-3 1.34-3 3-3zm0 14.2c-2.5 0-4.71-1.28-6-3.22.03-1.99 4-3.08 6-3.08 1.99 0 5.97 1.09 6 3.08-1.29 1.94-3.5 3.22-6 3.22z"/>
</svg>`;

/**
 * Handle image loading error - replace with default icon
 * @param {HTMLImageElement} imgElement - The image element that failed to load
 * @param {string} fallbackText - Optional text to display (initials)
 */
window.handleImageError = function(imgElement, fallbackText = '') {
    if (!imgElement || imgElement.dataset.fallbackApplied) return;
    
    // Mark as processed to prevent multiple calls
    imgElement.dataset.fallbackApplied = 'true';
    
    console.log('Image failed to load, applying fallback:', imgElement.src);

    // Get the parent container
    const parent = imgElement.parentElement;
    if (!parent) return;

    // Get computed styles from the image
    const computedStyle = window.getComputedStyle(imgElement);
    const width = imgElement.offsetWidth || imgElement.width || 40;
    const height = imgElement.offsetHeight || imgElement.height || 40;
    const borderRadius = computedStyle.borderRadius || '50%';

    // Create fallback container
    const fallbackContainer = document.createElement('div');
    fallbackContainer.className = imgElement.className.replace('object-cover', '').replace('profile-image', '') + ' message-avatar-fallback';
    fallbackContainer.style.width = `${width}px`;
    fallbackContainer.style.height = `${height}px`;
    fallbackContainer.style.borderRadius = borderRadius;
    fallbackContainer.style.display = 'flex';
    fallbackContainer.style.alignItems = 'center';
    fallbackContainer.style.justifyContent = 'center';
    fallbackContainer.style.background = 'linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%)';
    fallbackContainer.style.color = 'white';
    fallbackContainer.style.overflow = 'hidden';
    fallbackContainer.style.flexShrink = '0';

    // If fallback text (initials) is provided, show it
    if (fallbackText && fallbackText.trim()) {
        fallbackContainer.innerHTML = `<span style="font-size: ${Math.max(width * 0.4, 12)}px; font-weight: 600; line-height: 1;">${fallbackText}</span>`;
    } else {
        // Otherwise show default icon
        fallbackContainer.innerHTML = DEFAULT_PROFILE_ICON;
        fallbackContainer.style.padding = '20%';
    }

    // Replace the image with the fallback
    parent.replaceChild(fallbackContainer, imgElement);
};

/**
 * Add error handler to an image element
 * @param {string} imgId - The ID of the image element
 * @param {string} fallbackText - Optional text to display (initials)
 */
window.addImageErrorHandler = function(imgId, fallbackText = '') {
    const img = document.getElementById(imgId);
    if (img) {
        img.onerror = function() {
            window.handleImageError(this, fallbackText);
        };
    }
};

/**
 * Add error handlers to all images with a specific class
 * @param {string} className - The class name of images to handle
 */
window.addImageErrorHandlersByClass = function(className) {
    const images = document.getElementsByClassName(className);
    Array.from(images).forEach(img => {
        if (!img.dataset.fallbackApplied) {
            img.onerror = function() {
                const initials = this.getAttribute('data-initials') || '';
                window.handleImageError(this, initials);
            };
        }
    });
};

// Auto-initialize for images with 'profile-image' class on page load
document.addEventListener('DOMContentLoaded', function() {
    window.addImageErrorHandlersByClass('profile-image');
});

// Re-initialize when Blazor updates the DOM
if (window.Blazor) {
    window.Blazor.addEventListener('enhancedload', function() {
        setTimeout(() => {
            window.addImageErrorHandlersByClass('profile-image');
        }, 100);
    });
}

// Also handle dynamic content updates
const observer = new MutationObserver(function(mutations) {
    mutations.forEach(function(mutation) {
        if (mutation.type === 'childList') {
            mutation.addedNodes.forEach(function(node) {
                if (node.nodeType === 1) { // Element node
                    const images = node.querySelectorAll ? node.querySelectorAll('.profile-image') : [];
                    images.forEach(img => {
                        if (!img.dataset.fallbackApplied) {
                            const initials = img.getAttribute('data-initials') || '';
                            img.onerror = function() {
                                window.handleImageError(this, initials);
                            };
                        }
                    });
                }
            });
        }
    });
});

// Start observing
if (document.body) {
    observer.observe(document.body, { childList: true, subtree: true });
} else {
    document.addEventListener('DOMContentLoaded', function() {
        observer.observe(document.body, { childList: true, subtree: true });
    });
}
