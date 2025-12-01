// Notification sound playback
window.playNotificationSound = function(soundFile) {
    try {
        const audio = new Audio(`/sounds/${soundFile}`);
        audio.volume = 0.5; // 50% volume

        // Play the sound
        audio.play().catch(error => {
            console.log('Sound playback prevented by browser:', error);
            // Browser may prevent autoplay, this is expected
        });
    } catch (error) {
        console.error('Error playing notification sound:', error);
    }
};

// Request notification permission (for future browser notifications)
window.requestNotificationPermission = function() {
    if ('Notification' in window && Notification.permission === 'default') {
        return Notification.requestPermission();
    }
    return Promise.resolve(Notification.permission);
};

// Show browser notification (optional enhancement)
window.showBrowserNotification = function(title, message, icon) {
    if ('Notification' in window && Notification.permission === 'granted') {
        try {
            new Notification(title, {
                body: message,
                icon: icon || '/icon-192.png',
                badge: '/icon-192.png',
                vibrate: [200, 100, 200],
                tag: 'medichatai-notification'
            });
        } catch (error) {
            console.error('Error showing browser notification:', error);
        }
    }
};
