/**
 * Image Service for handling profile images and fallbacks
 */
window.imageService = {
    // Default avatar service URLs
    defaultAvatarServices: [
        'https://ui-avatars.com/api/?name={name}&background=6366f1&color=fff&size=200&rounded=true',
        'https://avatar.vercel.sh/{name}?size=200',
        'https://source.boringavatars.com/marble/200/{name}?colors=6366f1,8b5cf6,ec4899,f59e0b,10b981'
    ],

    // Generate a fallback image URL
    generateFallbackUrl: function(name, index = 0) {
        if (!name || index >= this.defaultAvatarServices.length) {
            return null;
        }
        
        const cleanName = encodeURIComponent(name.trim().replace(/\s+/g, '+'));
        return this.defaultAvatarServices[index].replace('{name}', cleanName);
    },

    // Try multiple fallback services
    tryFallbackImage: function(imgElement, name, serviceIndex = 0) {
        if (serviceIndex >= this.defaultAvatarServices.length) {
            // All services failed, use JavaScript fallback
            const initials = this.getInitials(name);
            window.handleImageError(imgElement, initials);
            return;
        }

        const fallbackUrl = this.generateFallbackUrl(name, serviceIndex);
        if (!fallbackUrl) {
            const initials = this.getInitials(name);
            window.handleImageError(imgElement, initials);
            return;
        }

        // Create a test image to check if the service is available
        const testImg = new Image();
        testImg.onload = function() {
            imgElement.src = fallbackUrl;
        };
        testImg.onerror = function() {
            // Try next service
            window.imageService.tryFallbackImage(imgElement, name, serviceIndex + 1);
        };
        testImg.src = fallbackUrl;
    },

    // Get initials from name
    getInitials: function(name) {
        if (!name || typeof name !== 'string') return 'U';
        const parts = name.trim().split(' ').filter(part => part.length > 0);
        if (parts.length >= 2) {
            return (parts[0][0] + parts[1][0]).toUpperCase();
        }
        return parts[0] ? parts[0].substring(0, 2).toUpperCase() : 'U';
    },

    // Enhanced image error handler that tries fallback services first
    handleImageError: function(imgElement, name) {
        if (!imgElement || imgElement.dataset.fallbackAttempted) return;
        
        imgElement.dataset.fallbackAttempted = 'true';
        console.log('Image failed to load, trying fallback services:', imgElement.src);
        
        if (name && name.trim()) {
            this.tryFallbackImage(imgElement, name);
        } else {
            // No name provided, use JavaScript fallback immediately
            window.handleImageError(imgElement, this.getInitials(name || 'User'));
        }
    },

    // Preload image to check if it exists
    preloadImage: function(url) {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => resolve(url);
            img.onerror = () => reject(new Error('Image failed to load'));
            img.src = url;
        });
    },

    // Get a working image URL or fallback
    getWorkingImageUrl: async function(originalUrl, name) {
        try {
            await this.preloadImage(originalUrl);
            return originalUrl;
        } catch {
            // Try fallback services
            for (let i = 0; i < this.defaultAvatarServices.length; i++) {
                try {
                    const fallbackUrl = this.generateFallbackUrl(name, i);
                    if (fallbackUrl) {
                        await this.preloadImage(fallbackUrl);
                        return fallbackUrl;
                    }
                } catch {
                    continue;
                }
            }
            // All failed, return null to trigger JavaScript fallback
            return null;
        }
    }
};

// Override the global handleImageError to use the enhanced version
window.handleImageError = function(imgElement, fallbackText = '') {
    window.imageService.handleImageError(imgElement, fallbackText);
};