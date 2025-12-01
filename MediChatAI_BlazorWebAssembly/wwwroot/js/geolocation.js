// Geolocation API wrapper for Blazor WebAssembly
window.geolocationHelper = {
    // Track live location watching
    watchId: null,
    locationCallbacks: [],

    /**
     * Gets the user's current location
     * @returns {Promise<{latitude: number, longitude: number, accuracy: number}>}
     */
    getCurrentPosition: function () {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject({
                    code: 0,
                    message: "Geolocation is not supported by your browser"
                });
                return;
            }

            navigator.geolocation.getCurrentPosition(
                (position) => {
                    resolve({
                        latitude: position.coords.latitude,
                        longitude: position.coords.longitude,
                        accuracy: position.coords.accuracy,
                        heading: position.coords.heading,
                        speed: position.coords.speed
                    });
                },
                (error) => {
                    let message = "Unknown error occurred";
                    switch (error.code) {
                        case error.PERMISSION_DENIED:
                            message = "Location permission denied. Please enable location access in your browser settings.";
                            break;
                        case error.POSITION_UNAVAILABLE:
                            message = "Location information is unavailable. Please check your device settings.";
                            break;
                        case error.TIMEOUT:
                            message = "Location request timed out. Please try again.";
                            break;
                    }
                    reject({
                        code: error.code,
                        message: message
                    });
                },
                {
                    enableHighAccuracy: true,
                    timeout: 10000,
                    maximumAge: 0
                }
            );
        });
    },

    /**
     * Starts continuous location tracking
     * @param {DotNet} dotNetReference - .NET object reference for callbacks
     * @returns {number} Watch ID
     */
    startWatchingPosition: function (dotNetReference) {
        if (this.watchId !== null) {
            return this.watchId;
        }

        if (!navigator.geolocation) {
            return -1;
        }

        // Track retry attempts for fallback
        let retryCount = 0;
        const maxRetries = 3;
        let useHighAccuracy = true;
        let lastErrorCode = null;

        const attemptWatch = () => {
            this.watchId = navigator.geolocation.watchPosition(
                (position) => {
                    // Reset retry count on success
                    retryCount = 0;
                    useHighAccuracy = true;

                    const locationData = {
                        latitude: position.coords.latitude,
                        longitude: position.coords.longitude,
                        accuracy: position.coords.accuracy,
                        heading: position.coords.heading,
                        speed: position.coords.speed,
                        timestamp: position.timestamp
                    };

                    // Invoke .NET callback
                    if (dotNetReference) {
                        dotNetReference.invokeMethodAsync('OnLocationUpdated', locationData);
                    }

                    // Notify all registered callbacks
                    this.locationCallbacks.forEach(callback => {
                        if (typeof callback === 'function') {
                            callback(locationData);
                        }
                    });
                },
                (error) => {
                    console.error('Watch position error:', error);
                    lastErrorCode = error.code;

                    // If timeout error (code 3) and retries left, try again with lower accuracy
                    if (error.code === 3 && retryCount < maxRetries) {
                        retryCount++;
                        useHighAccuracy = false; // Fallback to lower accuracy for speed
                        console.log(`Geolocation retry ${retryCount}/${maxRetries} with lower accuracy mode...`);

                        // Clear current watch and retry after delay
                        if (this.watchId !== null) {
                            navigator.geolocation.clearWatch(this.watchId);
                            this.watchId = null;
                        }

                        // Exponential backoff: 1s, 2s, 4s
                        const delay = Math.pow(2, retryCount - 1) * 1000;
                        setTimeout(() => attemptWatch(), delay);
                        return;
                    }

                    // All retries exhausted or non-timeout error, report to .NET
                    if (dotNetReference) {
                        let message = error.message;
                        if (retryCount >= maxRetries && error.code === 3) {
                            message = `Location tracking failed after ${maxRetries} attempts. Please check GPS signal or move outdoors.`;
                        }

                        dotNetReference.invokeMethodAsync('OnLocationError', {
                            code: error.code,
                            message: message
                        });
                    }
                },
                {
                    enableHighAccuracy: useHighAccuracy,
                    timeout: 15000, // Increased from 5000ms to 15000ms (15 seconds)
                    maximumAge: retryCount > 0 ? 10000 : 0 // Allow cached position on retry
                }
            );
        };

        // Start the watch with retry logic
        attemptWatch();
        return this.watchId || 1; // Return positive number to indicate attempt started
    },

    /**
     * Stops continuous location tracking
     */
    stopWatchingPosition: function () {
        if (this.watchId !== null) {
            navigator.geolocation.clearWatch(this.watchId);
            this.watchId = null;
            this.locationCallbacks = [];
        }
    },

    /**
     * Calculates distance between two coordinates (Haversine formula)
     * @param {number} lat1 - Latitude 1
     * @param {number} lon1 - Longitude 1
     * @param {number} lat2 - Latitude 2
     * @param {number} lon2 - Longitude 2
     * @returns {number} Distance in kilometers
     */
    calculateDistance: function (lat1, lon1, lat2, lon2) {
        const R = 6371; // Earth's radius in km
        const dLat = (lat2 - lat1) * Math.PI / 180;
        const dLon = (lon2 - lon1) * Math.PI / 180;
        const a =
            Math.sin(dLat / 2) * Math.sin(dLat / 2) +
            Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
            Math.sin(dLon / 2) * Math.sin(dLon / 2);
        const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
        return R * c;
    },

    /**
     * Calculates estimated time of arrival based on distance and average speed
     * @param {number} distanceKm - Distance in kilometers
     * @param {number} averageSpeedKmh - Average speed in km/h (default: 50 for urban driving)
     * @returns {number} ETA in minutes
     */
    calculateETA: function (distanceKm, averageSpeedKmh = 50) {
        return Math.round((distanceKm / averageSpeedKmh) * 60);
    },

    /**
     * Formats ETA into human-readable string
     * @param {number} minutes - ETA in minutes
     * @returns {string} Formatted ETA
     */
    formatETA: function (minutes) {
        if (minutes < 1) return 'Less than 1 min';
        if (minutes < 60) return `${minutes} min`;
        const hours = Math.floor(minutes / 60);
        const mins = minutes % 60;
        return mins > 0 ? `${hours}h ${mins}min` : `${hours}h`;
    },

    /**
     * Gets user's current heading/bearing
     * @returns {Promise<number>} Heading in degrees (0-360)
     */
    getCurrentHeading: function () {
        return new Promise((resolve, reject) => {
            if (window.DeviceOrientationEvent) {
                const handleOrientation = (event) => {
                    const heading = event.alpha; // 0-360 degrees
                    window.removeEventListener('deviceorientation', handleOrientation);
                    resolve(heading);
                };

                window.addEventListener('deviceorientation', handleOrientation);

                // Timeout after 3 seconds
                setTimeout(() => {
                    window.removeEventListener('deviceorientation', handleOrientation);
                    reject('Unable to get device orientation');
                }, 3000);
            } else {
                reject('Device orientation not supported');
            }
        });
    },

    /**
     * Checks if geolocation is supported
     * @returns {boolean}
     */
    isSupported: function () {
        return 'geolocation' in navigator;
    },

    /**
     * Opens Google Maps with directions to a location
     * @param {number} latitude - Destination latitude
     * @param {number} longitude - Destination longitude
     * @param {string} name - Location name
     * @param {boolean} openInApp - Try to open in Google Maps app if available
     */
    openDirections: function (latitude, longitude, name, openInApp = true) {
        const destination = `${latitude},${longitude}`;
        const label = encodeURIComponent(name);
        const webUrl = `https://www.google.com/maps/dir/?api=1&destination=${destination}&destination_name=${label}&travelmode=driving`;

        if (openInApp && this.isMobileDevice()) {
            // Detect Android for native app deep linking
            const isAndroid = /Android/i.test(navigator.userAgent);

            if (isAndroid) {
                // Use Android intent URL (more reliable than google.navigation:)
                const intentUrl = `intent://maps.google.com/maps?daddr=${destination}&directionsmode=driving#Intent;scheme=https;package=com.google.android.apps.maps;end`;

                // Try to open in app, with immediate web fallback
                const iframe = document.createElement('iframe');
                iframe.style.display = 'none';
                iframe.src = intentUrl;
                document.body.appendChild(iframe);

                // Clean up iframe and open web version after short delay
                setTimeout(() => {
                    document.body.removeChild(iframe);
                    window.open(webUrl, '_blank');
                }, 300);
            } else {
                // For iOS or other mobile devices, open web version directly
                window.open(webUrl, '_blank');
            }
        } else {
            // Desktop: open web version
            window.open(webUrl, '_blank');
        }
    },

    /**
     * Opens navigation with current location as starting point
     * @param {number} destLat - Destination latitude
     * @param {number} destLng - Destination longitude
     * @param {string} name - Destination name
     */
    navigateToDestination: async function (destLat, destLng, name) {
        try {
            const currentPos = await this.getCurrentPosition();
            const origin = `${currentPos.latitude},${currentPos.longitude}`;
            const destination = `${destLat},${destLng}`;

            if (this.isMobileDevice()) {
                // For mobile, try to open in native app
                const url = `https://www.google.com/maps/dir/?api=1&origin=${origin}&destination=${destination}&travelmode=driving`;
                window.location.href = url;
            } else {
                // For desktop, open in new tab
                const url = `https://www.google.com/maps/dir/?api=1&origin=${origin}&destination=${destination}&travelmode=driving`;
                window.open(url, '_blank');
            }
        } catch (error) {
            console.error('Navigation error:', error);
            // Fallback without current location
            this.openDirections(destLat, destLng, name);
        }
    },

    /**
     * Shares location via native share API
     * @param {number} latitude - Latitude
     * @param {number} longitude - Longitude
     * @param {string} message - Message to share
     */
    shareLocation: async function (latitude, longitude, message = '') {
        const url = `https://www.google.com/maps?q=${latitude},${longitude}`;
        const text = message || `My location: ${url}`;

        if (navigator.share) {
            try {
                await navigator.share({
                    title: 'Location Share',
                    text: text,
                    url: url
                });
                return true;
            } catch (error) {
                console.log('Share cancelled or failed:', error);
                return false;
            }
        } else {
            // Fallback: copy to clipboard
            try {
                await navigator.clipboard.writeText(text);
                return true;
            } catch (error) {
                console.error('Clipboard write failed:', error);
                return false;
            }
        }
    },

    /**
     * Checks if device is mobile
     * @returns {boolean}
     */
    isMobileDevice: function () {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
    },

    /**
     * Initiates a phone call
     * @param {string} phoneNumber - Phone number to call
     */
    makeCall: function (phoneNumber) {
        window.location.href = `tel:${phoneNumber}`;
    },

    /**
     * Opens a website in a new tab
     * @param {string} url - Website URL
     */
    openWebsite: function (url) {
        if (url && !url.startsWith('http')) {
            url = 'https://' + url;
        }
        window.open(url, '_blank');
    },

    /**
     * Generates a shareable location link
     * @param {number} latitude - Latitude
     * @param {number} longitude - Longitude
     * @returns {string} Shareable URL
     */
    generateLocationLink: function (latitude, longitude) {
        return `https://www.google.com/maps?q=${latitude},${longitude}`;
    }
};
