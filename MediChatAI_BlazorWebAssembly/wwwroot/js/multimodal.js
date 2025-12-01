// Multi-Modal Component JavaScript Helpers
// Supports Voice Recording and Camera Capture

// ============================================
// VOICE RECORDING
// ============================================

let mediaRecorder = null;
let audioChunks = [];
let dotNetHelper = null;

window.startVoiceRecording = async function (dotNetReference) {
    try {
        dotNetHelper = dotNetReference;
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

        mediaRecorder = new MediaRecorder(stream);
        audioChunks = [];

        mediaRecorder.ondataavailable = (event) => {
            if (event.data.size > 0) {
                audioChunks.push(event.data);
            }
        };

        mediaRecorder.onstop = async () => {
            const audioBlob = new Blob(audioChunks, { type: 'audio/webm' });
            const reader = new FileReader();

            reader.onloadend = async () => {
                const base64Audio = reader.result;
                if (dotNetHelper) {
                    await dotNetHelper.invokeMethodAsync('OnRecordingDataAvailable', base64Audio);
                }
            };

            reader.readAsDataURL(audioBlob);

            // Stop all tracks
            stream.getTracks().forEach(track => track.stop());
        };

        mediaRecorder.start();
        console.log('Voice recording started');
        return true;
    } catch (error) {
        console.error('Error starting voice recording:', error);
        return false;
    }
};

window.stopVoiceRecording = function () {
    if (mediaRecorder && mediaRecorder.state !== 'inactive') {
        mediaRecorder.stop();
        console.log('Voice recording stopped');
    }
};

window.playAudio = function (audioElement) {
    if (audioElement) {
        audioElement.play();
    }
};

window.pauseAudio = function (audioElement) {
    if (audioElement) {
        audioElement.pause();
    }
};

// ============================================
// CAMERA CAPTURE
// ============================================

let videoStream = null;

window.startCamera = async function (videoElement, facingMode = 'user') {
    try {
        console.log('[Camera] Starting camera initialization...');
        console.log('[Camera] Video element:', videoElement);
        console.log('[Camera] Facing mode:', facingMode);

        // Check if mediaDevices API is available
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            console.error('[Camera] MediaDevices API not available. HTTPS required or browser not supported.');
            return false;
        }

        // Stop existing stream if any
        if (videoStream) {
            console.log('[Camera] Stopping existing stream...');
            videoStream.getTracks().forEach(track => track.stop());
        }

        const constraints = {
            video: {
                facingMode: facingMode,
                width: { ideal: 1280 },
                height: { ideal: 720 }
            },
            audio: false
        };

        console.log('[Camera] Requesting user media with constraints:', constraints);
        videoStream = await navigator.mediaDevices.getUserMedia(constraints);
        console.log('[Camera] Stream acquired successfully');

        videoElement.srcObject = videoStream;
        console.log('[Camera] Stream attached to video element');

        // Wait for video metadata to load, then explicitly play
        await new Promise((resolve) => {
            videoElement.onloadedmetadata = () => {
                console.log('[Camera] Video metadata loaded');
                resolve();
            };
        });

        // Explicitly start video playback
        await videoElement.play();
        console.log('[Camera] Video playback started successfully');

        console.log('[Camera] Camera started with facing mode:', facingMode);
        return true;
    } catch (error) {
        console.error('[Camera] Error starting camera:', error);
        console.error('[Camera] Error name:', error.name);
        console.error('[Camera] Error message:', error.message);

        // Log specific error types to help diagnose
        if (error.name === 'NotAllowedError') {
            console.error('[Camera] USER DENIED PERMISSION - User needs to allow camera access');
        } else if (error.name === 'NotFoundError') {
            console.error('[Camera] NO CAMERA FOUND - Device has no camera or camera is disabled');
        } else if (error.name === 'NotReadableError') {
            console.error('[Camera] CAMERA IN USE - Camera is already being used by another application');
        } else if (error.name === 'OverconstrainedError') {
            console.error('[Camera] CONSTRAINTS NOT SATISFIED - Camera cannot meet the requested constraints');
        } else if (error.name === 'SecurityError') {
            console.error('[Camera] SECURITY ERROR - HTTPS required or insecure context');
        }

        return false;
    }
};

window.stopCamera = function () {
    if (videoStream) {
        videoStream.getTracks().forEach(track => track.stop());
        videoStream = null;
        console.log('Camera stopped');
    }
};

window.canSwitchCamera = async function () {
    try {
        const devices = await navigator.mediaDevices.enumerateDevices();
        const videoDevices = devices.filter(device => device.kind === 'videoinput');
        return videoDevices.length > 1;
    } catch (error) {
        console.error('Error checking for multiple cameras:', error);
        return false;
    }
};

window.capturePhoto = function (videoElement, canvasElement) {
    try {
        const video = videoElement;
        const canvas = canvasElement;

        // Set canvas dimensions to match video
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        // Draw the video frame to canvas
        const context = canvas.getContext('2d');
        context.drawImage(video, 0, 0, canvas.width, canvas.height);

        // Convert canvas to base64 image
        const imageData = canvas.toDataURL('image/png');
        console.log('Photo captured');

        return imageData;
    } catch (error) {
        console.error('Error capturing photo:', error);
        return null;
    }
};

// ============================================
// AUDIO PLAYBACK HELPERS
// ============================================

window.playNotificationSound = function (soundFile) {
    try {
        const audio = new Audio(`/sounds/${soundFile}`);
        audio.play().catch(err => console.error('Error playing sound:', err));
    } catch (error) {
        console.error('Error in playNotificationSound:', error);
    }
};

// ============================================
// PATIENT THEME HELPERS
// ============================================

window.applyPatientTheme = function (themeConfig) {
    try {
        const root = document.documentElement;

        // Apply theme mode
        if (themeConfig.mode === 'dark') {
            root.classList.add('dark');
        } else {
            root.classList.remove('dark');
        }

        // Apply color variables
        root.style.setProperty('--primary-color', themeConfig.primaryColor);
        root.style.setProperty('--accent-color', themeConfig.accentColor);

        // Calculate lighter/darker shades
        const primary = hexToRgb(themeConfig.primaryColor);
        if (primary) {
            root.style.setProperty('--primary-50', `rgb(${lighten(primary, 0.95)})`);
            root.style.setProperty('--primary-100', `rgb(${lighten(primary, 0.9)})`);
            root.style.setProperty('--primary-200', `rgb(${lighten(primary, 0.75)})`);
            root.style.setProperty('--primary-300', `rgb(${lighten(primary, 0.6)})`);
            root.style.setProperty('--primary-400', `rgb(${lighten(primary, 0.3)})`);
            root.style.setProperty('--primary-500', themeConfig.primaryColor);
            root.style.setProperty('--primary-600', `rgb(${darken(primary, 0.1)})`);
            root.style.setProperty('--primary-700', `rgb(${darken(primary, 0.2)})`);
            root.style.setProperty('--primary-800', `rgb(${darken(primary, 0.3)})`);
            root.style.setProperty('--primary-900', `rgb(${darken(primary, 0.4)})`);
        }

        console.log('Patient theme applied:', themeConfig);
    } catch (error) {
        console.error('Error applying patient theme:', error);
    }
};

// Helper function to convert hex to RGB
function hexToRgb(hex) {
    const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}

// Helper function to lighten color
function lighten(rgb, amount) {
    return `${Math.min(255, rgb.r + (255 - rgb.r) * amount)}, ${Math.min(255, rgb.g + (255 - rgb.g) * amount)}, ${Math.min(255, rgb.b + (255 - rgb.b) * amount)}`;
}

// Helper function to darken color
function darken(rgb, amount) {
    return `${Math.max(0, rgb.r - rgb.r * amount)}, ${Math.max(0, rgb.g - rgb.g * amount)}, ${Math.max(0, rgb.b - rgb.b * amount)}`;
}

// ============================================
// SPEECH RECOGNITION (Optional Enhancement)
// ============================================

let recognition = null;

window.startSpeechRecognition = function (dotNetReference, continuous = false) {
    try {
        if ('webkitSpeechRecognition' in window || 'SpeechRecognition' in window) {
            const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
            recognition = new SpeechRecognition();

            recognition.continuous = continuous;
            recognition.interimResults = true;
            recognition.lang = 'en-US';

            recognition.onresult = (event) => {
                const transcript = Array.from(event.results)
                    .map(result => result[0])
                    .map(result => result.transcript)
                    .join('');

                dotNetReference.invokeMethodAsync('OnSpeechRecognized', transcript);
            };

            recognition.onerror = (event) => {
                console.error('Speech recognition error:', event.error);
                dotNetReference.invokeMethodAsync('OnSpeechError', event.error);
            };

            recognition.onend = () => {
                console.log('Speech recognition ended');
            };

            recognition.start();
            console.log('Speech recognition started');
            return true;
        } else {
            console.error('Speech recognition not supported in this browser');
            return false;
        }
    } catch (error) {
        console.error('Error starting speech recognition:', error);
        return false;
    }
};

window.stopSpeechRecognition = function () {
    if (recognition) {
        recognition.stop();
        recognition = null;
        console.log('Speech recognition stopped');
    }
};

console.log('Multi-modal JavaScript helpers loaded successfully');
