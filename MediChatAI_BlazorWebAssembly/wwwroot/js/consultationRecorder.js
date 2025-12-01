// Consultation Video/Audio Recorder JavaScript Interop
// Supports recording full consultation sessions with video, audio, and screen sharing

window.consultationRecorder = {
    mediaRecorder: null,
    recordedChunks: [],
    stream: null,
    recordingStartTime: null,
    recordingType: 'video', // 'video', 'audio', 'screen'

    // Get supported MIME types
    getSupportedMimeType() {
        const types = [
            'video/webm;codecs=vp9,opus',
            'video/webm;codecs=vp8,opus',
            'video/webm;codecs=h264,opus',
            'video/webm',
            'video/mp4'
        ];

        for (const type of types) {
            if (MediaRecorder.isTypeSupported(type)) {
                console.log('Using MIME type:', type);
                return type;
            }
        }

        return 'video/webm'; // Fallback
    },

    // Check camera and microphone permissions
    async checkMediaPermissions() {
        try {
            const cameraPermission = await navigator.permissions.query({ name: 'camera' });
            const micPermission = await navigator.permissions.query({ name: 'microphone' });

            return {
                camera: cameraPermission.state,
                microphone: micPermission.state
            };
        } catch (err) {
            console.warn('Permissions API not supported, will request at recording time');
            return null;
        }
    },

    // Request media permissions and get stream
    async getMediaStream(options = {}) {
        const constraints = {
            video: options.video !== false ? {
                width: { ideal: 1280, max: 1920 },
                height: { ideal: 720, max: 1080 },
                frameRate: { ideal: 30, max: 60 },
                facingMode: 'user'
            } : false,
            audio: options.audio !== false ? {
                echoCancellation: true,
                noiseSuppression: true,
                autoGainControl: true,
                sampleRate: 48000
            } : false
        };

        try {
            const stream = await navigator.mediaDevices.getUserMedia(constraints);
            console.log('Media stream obtained:', stream.getTracks().map(t => t.kind));
            return stream;
        } catch (err) {
            console.error('Error getting media stream:', err);
            throw err;
        }
    },

    // Get screen capture stream
    async getScreenStream(options = {}) {
        try {
            const stream = await navigator.mediaDevices.getDisplayMedia({
                video: {
                    cursor: 'always',
                    displaySurface: 'monitor'
                },
                audio: options.includeSystemAudio || false
            });

            console.log('Screen stream obtained');
            return stream;
        } catch (err) {
            console.error('Error getting screen stream:', err);
            throw err;
        }
    },

    // Combine multiple streams into one
    combineStreams(videoStream, audioStream) {
        const combinedStream = new MediaStream();

        if (videoStream) {
            videoStream.getVideoTracks().forEach(track => {
                combinedStream.addTrack(track);
            });
        }

        if (audioStream) {
            audioStream.getAudioTracks().forEach(track => {
                combinedStream.addTrack(track);
            });
        }

        return combinedStream;
    },

    // Start recording consultation
    async startRecording(dotNetHelper, options = {}) {
        try {
            this.recordedChunks = [];
            this.recordingStartTime = Date.now();
            this.recordingType = options.type || 'video';

            // Get appropriate stream based on recording type
            if (this.recordingType === 'screen') {
                this.stream = await this.getScreenStream(options);
            } else if (this.recordingType === 'video') {
                this.stream = await this.getMediaStream(options);
            } else if (this.recordingType === 'audio') {
                this.stream = await this.getMediaStream({ video: false, audio: true });
            }

            const mimeType = this.getSupportedMimeType();

            // Create MediaRecorder with optimized settings
            this.mediaRecorder = new MediaRecorder(this.stream, {
                mimeType: mimeType,
                videoBitsPerSecond: options.videoBitrate || 2500000, // 2.5 Mbps
                audioBitsPerSecond: options.audioBitrate || 128000   // 128 kbps
            });

            // Handle data available event - collect chunks
            this.mediaRecorder.ondataavailable = (event) => {
                if (event.data && event.data.size > 0) {
                    this.recordedChunks.push(event.data);

                    // Notify .NET about chunk received (for progress tracking)
                    const totalSize = this.recordedChunks.reduce((acc, chunk) => acc + chunk.size, 0);
                    dotNetHelper.invokeMethodAsync('OnChunkRecorded', totalSize);
                }
            };

            // Handle recording start
            this.mediaRecorder.onstart = () => {
                console.log('Recording started at', new Date(this.recordingStartTime).toISOString());
                dotNetHelper.invokeMethodAsync('OnRecordingStarted');
            };

            // Handle recording stop
            this.mediaRecorder.onstop = () => {
                const duration = Date.now() - this.recordingStartTime;
                console.log('Recording stopped, duration:', duration, 'ms');
                dotNetHelper.invokeMethodAsync('OnRecordingStopped', duration);
            };

            // Handle errors
            this.mediaRecorder.onerror = (event) => {
                console.error('MediaRecorder error:', event.error);
                dotNetHelper.invokeMethodAsync('OnRecordingError', event.error?.message || 'Unknown error');
            };

            // Handle stream ended (e.g., user stopped screen share)
            this.stream.getTracks().forEach(track => {
                track.onended = () => {
                    console.log('Track ended:', track.kind);
                    if (this.mediaRecorder && this.mediaRecorder.state === 'recording') {
                        this.stopRecording();
                    }
                };
            });

            // Start recording - collect data every 1 second
            this.mediaRecorder.start(1000);

            return {
                success: true,
                mimeType: mimeType
            };
        } catch (err) {
            console.error('Error starting recording:', err);
            dotNetHelper.invokeMethodAsync('OnRecordingError', err.message);
            return {
                success: false,
                error: err.message
            };
        }
    },

    // Stop recording and get blob
    async stopRecording() {
        return new Promise((resolve, reject) => {
            if (!this.mediaRecorder || this.mediaRecorder.state === 'inactive') {
                reject({ success: false, error: 'No active recording' });
                return;
            }

            this.mediaRecorder.onstop = async () => {
                try {
                    const mimeType = this.mediaRecorder.mimeType;
                    const recordingBlob = new Blob(this.recordedChunks, { type: mimeType });

                    // Create object URL for preview
                    const objectUrl = URL.createObjectURL(recordingBlob);

                    // Calculate recording info
                    const duration = Date.now() - this.recordingStartTime;
                    const sizeInBytes = recordingBlob.size;
                    const sizeInMB = (sizeInBytes / (1024 * 1024)).toFixed(2);

                    // Stop all tracks
                    if (this.stream) {
                        this.stream.getTracks().forEach(track => {
                            track.stop();
                            console.log('Stopped track:', track.kind);
                        });
                        this.stream = null;
                    }

                    console.log(`Recording complete: ${sizeInMB}MB, ${(duration / 1000).toFixed(1)}s`);

                    resolve({
                        success: true,
                        objectUrl: objectUrl,
                        sizeInBytes: sizeInBytes,
                        durationMs: duration,
                        mimeType: mimeType,
                        chunkCount: this.recordedChunks.length
                    });
                } catch (err) {
                    console.error('Error processing recording:', err);
                    reject({ success: false, error: err.message });
                }
            };

            this.mediaRecorder.stop();
        });
    },

    // Pause recording
    pauseRecording() {
        if (this.mediaRecorder && this.mediaRecorder.state === 'recording') {
            this.mediaRecorder.pause();
            console.log('Recording paused');
            return true;
        }
        return false;
    },

    // Resume recording
    resumeRecording() {
        if (this.mediaRecorder && this.mediaRecorder.state === 'paused') {
            this.mediaRecorder.resume();
            console.log('Recording resumed');
            return true;
        }
        return false;
    },

    // Cancel recording without saving
    async cancelRecording() {
        if (this.mediaRecorder && this.mediaRecorder.state !== 'inactive') {
            this.mediaRecorder.stop();
        }

        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }

        this.recordedChunks = [];
        console.log('Recording cancelled');

        return { success: true };
    },

    // Get recording state
    getRecordingState() {
        if (!this.mediaRecorder) {
            return 'inactive';
        }
        return this.mediaRecorder.state;
    },

    // Get recording duration
    getRecordingDuration() {
        if (!this.recordingStartTime) {
            return 0;
        }
        return Date.now() - this.recordingStartTime;
    },

    // Get recording size
    getRecordingSize() {
        return this.recordedChunks.reduce((acc, chunk) => acc + chunk.size, 0);
    },

    // Convert blob to base64 (for smaller files)
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

    // Get blob from object URL
    async objectUrlToBlob(objectUrl) {
        const response = await fetch(objectUrl);
        return await response.blob();
    },

    // Convert blob to array of chunks for chunked upload
    async blobToChunks(blob, chunkSize = 1024 * 1024) { // 1MB chunks
        const chunks = [];
        let offset = 0;

        while (offset < blob.size) {
            const chunk = blob.slice(offset, offset + chunkSize);
            const base64Chunk = await this.blobToBase64(chunk);
            chunks.push({
                index: chunks.length,
                data: base64Chunk,
                size: chunk.size,
                isLast: offset + chunkSize >= blob.size
            });
            offset += chunkSize;
        }

        return chunks;
    },

    // Upload recording with progress tracking
    async uploadRecording(objectUrl, uploadUrl, dotNetHelper) {
        try {
            const blob = await this.objectUrlToBlob(objectUrl);
            const formData = new FormData();

            // Determine file extension from MIME type
            const extension = this.getFileExtension(blob.type);
            const filename = `consultation_${Date.now()}.${extension}`;

            formData.append('file', blob, filename);

            // Create XMLHttpRequest for progress tracking
            return new Promise((resolve, reject) => {
                const xhr = new XMLHttpRequest();

                xhr.upload.addEventListener('progress', (e) => {
                    if (e.lengthComputable) {
                        const percentComplete = (e.loaded / e.total) * 100;
                        dotNetHelper.invokeMethodAsync('OnUploadProgress', percentComplete, e.loaded, e.total);
                    }
                });

                xhr.addEventListener('load', () => {
                    if (xhr.status >= 200 && xhr.status < 300) {
                        resolve({
                            success: true,
                            response: xhr.responseText
                        });
                    } else {
                        reject({
                            success: false,
                            error: `Upload failed with status ${xhr.status}`
                        });
                    }
                });

                xhr.addEventListener('error', () => {
                    reject({
                        success: false,
                        error: 'Network error during upload'
                    });
                });

                xhr.open('POST', uploadUrl);
                xhr.send(formData);
            });
        } catch (err) {
            console.error('Error uploading recording:', err);
            return {
                success: false,
                error: err.message
            };
        }
    },

    // Get file extension from MIME type
    getFileExtension(mimeType) {
        if (mimeType.includes('webm')) return 'webm';
        if (mimeType.includes('mp4')) return 'mp4';
        if (mimeType.includes('ogg')) return 'ogg';
        if (mimeType.includes('mkv')) return 'mkv';
        return 'webm'; // Default
    },

    // Clean up resources
    cleanup() {
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }

        this.recordedChunks = [];
        this.mediaRecorder = null;
        this.recordingStartTime = null;

        console.log('Consultation recorder cleaned up');
    },

    // Revoke object URL to free memory
    revokeObjectURL(url) {
        if (url) {
            URL.revokeObjectURL(url);
        }
    },

    // Get video thumbnail from recorded video
    async getVideoThumbnail(objectUrl, seekTimeSeconds = 1) {
        return new Promise((resolve, reject) => {
            const video = document.createElement('video');
            const canvas = document.createElement('canvas');

            video.addEventListener('loadedmetadata', () => {
                canvas.width = video.videoWidth;
                canvas.height = video.videoHeight;
                video.currentTime = Math.min(seekTimeSeconds, video.duration / 2);
            });

            video.addEventListener('seeked', () => {
                const ctx = canvas.getContext('2d');
                ctx.drawImage(video, 0, 0, canvas.width, canvas.height);

                canvas.toBlob((blob) => {
                    if (blob) {
                        const thumbnailUrl = URL.createObjectURL(blob);
                        resolve(thumbnailUrl);
                    } else {
                        reject('Failed to generate thumbnail');
                    }
                }, 'image/jpeg', 0.8);
            });

            video.addEventListener('error', (err) => {
                reject('Error loading video: ' + err.message);
            });

            video.src = objectUrl;
        });
    },

    // Get media devices info
    async getMediaDevices() {
        try {
            const devices = await navigator.mediaDevices.enumerateDevices();
            return {
                videoInputs: devices.filter(d => d.kind === 'videoinput').map(d => ({
                    deviceId: d.deviceId,
                    label: d.label || `Camera ${d.deviceId.substring(0, 5)}`
                })),
                audioInputs: devices.filter(d => d.kind === 'audioinput').map(d => ({
                    deviceId: d.deviceId,
                    label: d.label || `Microphone ${d.deviceId.substring(0, 5)}`
                })),
                audioOutputs: devices.filter(d => d.kind === 'audiooutput').map(d => ({
                    deviceId: d.deviceId,
                    label: d.label || `Speaker ${d.deviceId.substring(0, 5)}`
                }))
            };
        } catch (err) {
            console.error('Error enumerating devices:', err);
            return { videoInputs: [], audioInputs: [], audioOutputs: [] };
        }
    }
};
