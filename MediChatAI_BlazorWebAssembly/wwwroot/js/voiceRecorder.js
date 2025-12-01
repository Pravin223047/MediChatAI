// Voice Recorder JavaScript Interop
window.voiceRecorder = {
    mediaRecorder: null,
    audioChunks: [],
    stream: null,

    // Check if microphone permission is granted
    async checkMicrophonePermission() {
        try {
            const result = await navigator.permissions.query({ name: 'microphone' });

            if (result.state === 'granted') {
                return true;
            } else if (result.state === 'prompt') {
                // Try to get user media to trigger permission prompt
                try {
                    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
                    stream.getTracks().forEach(track => track.stop());
                    return true;
                } catch (err) {
                    console.error('Microphone permission denied:', err);
                    alert('Microphone permission is required to record voice messages.');
                    return false;
                }
            } else {
                alert('Microphone permission is denied. Please enable it in your browser settings.');
                return false;
            }
        } catch (err) {
            console.error('Error checking microphone permission:', err);

            // Fallback for browsers that don't support permissions API
            try {
                const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
                stream.getTracks().forEach(track => track.stop());
                return true;
            } catch (mediaErr) {
                console.error('Microphone access denied:', mediaErr);
                alert('Microphone permission is required to record voice messages.');
                return false;
            }
        }
    },

    // Start recording audio
    async startRecording(dotNetHelper) {
        try {
            // Request microphone access
            this.stream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    echoCancellation: true,
                    noiseSuppression: true,
                    autoGainControl: true
                }
            });

            // Create MediaRecorder with appropriate MIME type
            let mimeType = 'audio/webm';
            if (MediaRecorder.isTypeSupported('audio/webm;codecs=opus')) {
                mimeType = 'audio/webm;codecs=opus';
            } else if (MediaRecorder.isTypeSupported('audio/ogg;codecs=opus')) {
                mimeType = 'audio/ogg;codecs=opus';
            } else if (MediaRecorder.isTypeSupported('audio/mp4')) {
                mimeType = 'audio/mp4';
            }

            this.mediaRecorder = new MediaRecorder(this.stream, { mimeType });
            this.audioChunks = [];

            this.mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    this.audioChunks.push(event.data);
                }
            };

            this.mediaRecorder.onerror = (event) => {
                console.error('MediaRecorder error:', event.error);
                dotNetHelper.invokeMethodAsync('OnRecordingError', event.error.name);
            };

            this.mediaRecorder.start(100); // Collect data every 100ms

            console.log('Recording started');
        } catch (err) {
            console.error('Error starting recording:', err);
            dotNetHelper.invokeMethodAsync('OnRecordingError', err.message);
            throw err;
        }
    },

    // Stop recording and return audio data URL
    async stopRecording() {
        return new Promise((resolve, reject) => {
            if (!this.mediaRecorder || this.mediaRecorder.state === 'inactive') {
                reject('No active recording');
                return;
            }

            this.mediaRecorder.onstop = () => {
                const audioBlob = new Blob(this.audioChunks, { type: this.mediaRecorder.mimeType });
                const audioUrl = URL.createObjectURL(audioBlob);

                // Stop all tracks
                if (this.stream) {
                    this.stream.getTracks().forEach(track => track.stop());
                    this.stream = null;
                }

                console.log('Recording stopped, audio URL:', audioUrl);
                resolve(audioUrl);
            };

            this.mediaRecorder.stop();
        });
    },

    // Cancel recording without saving
    cancelRecording() {
        if (this.mediaRecorder && this.mediaRecorder.state !== 'inactive') {
            this.mediaRecorder.stop();
        }

        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }

        this.audioChunks = [];
        console.log('Recording cancelled');
    },

    // Play audio element
    playAudio(audioElement) {
        if (audioElement) {
            audioElement.play();
        }
    },

    // Pause audio element
    pauseAudio(audioElement) {
        if (audioElement) {
            audioElement.pause();
        }
    },

    // Get current playback time
    getCurrentTime(audioElement) {
        return audioElement ? audioElement.currentTime : 0;
    },

    // Get audio duration
    getDuration(audioElement) {
        return audioElement ? audioElement.duration : 0;
    },

    // Revoke object URL to free memory
    revokeObjectURL(url) {
        if (url) {
            URL.revokeObjectURL(url);
        }
    },

    // Convert audio blob to base64 for upload
    async blobToBase64(blob) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => {
                const base64data = reader.result.split(',')[1];
                resolve(base64data);
            };
            reader.onerror = reject;
            reader.readAsDataURL(blob);
        });
    },

    // Convert data URL to blob for upload
    async dataURLtoBlob(dataUrl) {
        const response = await fetch(dataUrl);
        return await response.blob();
    },

    // Convert data URL to base64
    async dataURLtoBase64(dataUrl) {
        const blob = await this.dataURLtoBlob(dataUrl);
        return await this.blobToBase64(blob);
    },

    // Get audio file extension from MIME type
    getAudioExtension(mimeType) {
        if (mimeType.includes('webm')) return 'webm';
        if (mimeType.includes('ogg')) return 'ogg';
        if (mimeType.includes('mp4')) return 'mp4';
        if (mimeType.includes('mpeg')) return 'mp3';
        return 'webm'; // default
    }
};

// Global audio control functions for voice messages
window.playAudio = function(audioElement) {
    if (audioElement && audioElement.play) {
        audioElement.play().catch(err => console.error('Error playing audio:', err));
    }
};

window.pauseAudio = function(audioElement) {
    if (audioElement && audioElement.pause) {
        audioElement.pause();
    }
};
