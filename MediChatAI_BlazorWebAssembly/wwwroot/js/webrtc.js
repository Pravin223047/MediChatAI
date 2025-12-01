// WebRTC JavaScript Interop for Video/Voice Calling
window.webrtc = {
    peerConnection: null,
    localStream: null,
    remoteStream: null,
    dotNetRef: null,

    config: {
        iceServers: [
            { urls: 'stun:stun.l.google.com:19302' },
            { urls: 'stun:stun1.l.google.com:19302' },
            { urls: 'stun:stun2.l.google.com:19302' }
        ]
    },

    // Initialize WebRTC with .NET reference for callbacks
    initialize: function(dotNetReference) {
        this.dotNetRef = dotNetReference;
        console.log('WebRTC initialized');
    },

    // Check if browser supports WebRTC
    isSupported: function() {
        return !!(navigator.mediaDevices &&
                 navigator.mediaDevices.getUserMedia &&
                 window.RTCPeerConnection);
    },

    // Request camera and microphone permissions
    checkMediaPermissions: async function() {
        try {
            const result = await navigator.permissions.query({ name: 'camera' });
            const audioResult = await navigator.permissions.query({ name: 'microphone' });

            return result.state === 'granted' && audioResult.state === 'granted';
        } catch (err) {
            // Fallback: try to get user media to check permissions
            try {
                const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
                stream.getTracks().forEach(track => track.stop());
                return true;
            } catch (mediaErr) {
                console.error('Media permission denied:', mediaErr);
                return false;
            }
        }
    },

    // Start local media stream
    startLocalStream: async function(videoElementId, audioOnly = false) {
        try {
            const constraints = audioOnly
                ? { audio: true, video: false }
                : {
                    audio: {
                        echoCancellation: true,
                        noiseSuppression: true,
                        autoGainControl: true
                    },
                    video: {
                        width: { ideal: 640 },
                        height: { ideal: 480 },
                        facingMode: 'user'
                    }
                };

            this.localStream = await navigator.mediaDevices.getUserMedia(constraints);
            console.log('Got camera stream with tracks:', this.localStream.getTracks().length);

            if (!audioOnly) {
                const videoElement = document.getElementById(videoElementId);
                if (videoElement && this.localStream) {
                    videoElement.srcObject = this.localStream;
                    videoElement.muted = true;
                    videoElement.autoplay = true;
                    videoElement.playsInline = true;
                    await videoElement.play().catch(e => console.log('Local video play failed:', e));
                    console.log('Local video element setup complete');
                    
                    const hasVideo = this.localStream.getVideoTracks().length > 0;
                    this.updateVideoState(true, hasVideo);
                } else if (!audioOnly) {
                    console.warn('Video element not found for video call:', videoElementId);
                }
            }

            console.log('Local stream started successfully');
            return true;
        } catch (err) {
            console.error('Error accessing media devices:', err);
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnMediaError', err.message);
            }
            return false;
        }
    },



    // Create peer connection and make an offer
    createOffer: async function(remoteUserId, isVideoCall) {
        try {
            if (!this.peerConnection) {
                this.peerConnection = new RTCPeerConnection(this.config);
                this.setupPeerConnectionListeners();
            }

            if (this.localStream) {
                this.localStream.getTracks().forEach(track => {
                    this.peerConnection.addTrack(track, this.localStream);
                });
            }

            const offer = await this.peerConnection.createOffer();
            await this.peerConnection.setLocalDescription(offer);

            console.log('Offer created successfully');
            return JSON.stringify(offer);
        } catch (err) {
            console.error('Error creating offer:', err);
            return null;
        }
    },

    // Handle incoming offer and create answer
    handleOffer: async function(offerJson, remoteVideoElementId, isVideoCall) {
        try {
            const offer = JSON.parse(offerJson);

            if (!this.peerConnection) {
                this.peerConnection = new RTCPeerConnection(this.config);
                this.setupPeerConnectionListeners();
            }

            if (this.localStream) {
                this.localStream.getTracks().forEach(track => {
                    this.peerConnection.addTrack(track, this.localStream);
                });
            }

            this.peerConnection.ontrack = (event) => {
                console.log('Received remote track:', event.track.kind);
                const remoteVideo = document.getElementById(remoteVideoElementId);
                if (remoteVideo) {
                    if (remoteVideo.srcObject) {
                        remoteVideo.srcObject.addTrack(event.track);
                    } else {
                        const stream = new MediaStream([event.track]);
                        remoteVideo.srcObject = stream;
                        this.remoteStream = stream;
                    }
                    remoteVideo.play().catch(e => console.log('Remote video play failed:', e));
                    
                    // Update UI based on track type and enabled state
                    if (event.track.kind === 'video') {
                        this.updateRemoteVideoState(event.track.enabled);
                        // Listen for track enabled/disabled changes
                        event.track.addEventListener('ended', () => {
                            this.updateRemoteVideoState(false);
                        });
                        event.track.addEventListener('mute', () => {
                            this.updateRemoteVideoState(false);
                        });
                        event.track.addEventListener('unmute', () => {
                            this.updateRemoteVideoState(true);
                        });
                        
                        // Monitor video frames after video starts playing
                        remoteVideo.addEventListener('loadeddata', () => {
                            this.monitorVideoFrames(event.track, remoteVideo);
                        });
                    }
                }
            };

            await this.peerConnection.setRemoteDescription(new RTCSessionDescription(offer));
            const answer = await this.peerConnection.createAnswer();
            await this.peerConnection.setLocalDescription(answer);

            return JSON.stringify(answer);
        } catch (err) {
            console.error('Error handling offer:', err);
            return null;
        }
    },

    // Handle incoming answer
    handleAnswer: async function(answerJson, remoteVideoElementId) {
        try {
            const answer = JSON.parse(answerJson);

            this.peerConnection.ontrack = (event) => {
                console.log('Received remote track in answer:', event.track.kind);
                const remoteVideo = document.getElementById(remoteVideoElementId);
                if (remoteVideo) {
                    if (remoteVideo.srcObject) {
                        remoteVideo.srcObject.addTrack(event.track);
                    } else {
                        const stream = new MediaStream([event.track]);
                        remoteVideo.srcObject = stream;
                        this.remoteStream = stream;
                    }
                    remoteVideo.play().catch(e => console.log('Remote video play failed:', e));
                    
                    // Update UI based on track type and enabled state
                    if (event.track.kind === 'video') {
                        this.updateRemoteVideoState(event.track.enabled);
                        // Listen for track enabled/disabled changes
                        event.track.addEventListener('ended', () => {
                            this.updateRemoteVideoState(false);
                        });
                        event.track.addEventListener('mute', () => {
                            this.updateRemoteVideoState(false);
                        });
                        event.track.addEventListener('unmute', () => {
                            this.updateRemoteVideoState(true);
                        });
                    }
                }
            };

            await this.peerConnection.setRemoteDescription(new RTCSessionDescription(answer));
            return true;
        } catch (err) {
            console.error('Error handling answer:', err);
            return false;
        }
    },

    // Add ICE candidate
    addIceCandidate: async function(candidateJson) {
        try {
            if (!this.peerConnection || !this.peerConnection.remoteDescription) {
                return false;
            }

            const candidate = JSON.parse(candidateJson);
            await this.peerConnection.addIceCandidate(new RTCIceCandidate(candidate));
            return true;
        } catch (err) {
            console.error('Error adding ICE candidate:', err);
            return false;
        }
    },

    // Setup peer connection event listeners
    setupPeerConnectionListeners: function() {
        if (!this.peerConnection) return;

        this.peerConnection.onicecandidate = (event) => {
            if (event.candidate && this.dotNetRef) {
                console.log('ICE candidate generated');
                this.dotNetRef.invokeMethodAsync('OnIceCandidate', JSON.stringify(event.candidate));
            }
        };

        this.peerConnection.oniceconnectionstatechange = () => {
            console.log('ICE connection state:', this.peerConnection.iceConnectionState);
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnConnectionStateChanged', this.peerConnection.iceConnectionState);
            }
        };

        this.peerConnection.onconnectionstatechange = () => {
            console.log('Connection state:', this.peerConnection.connectionState);
        };
    },

    // Toggle audio mute
    toggleAudio: function(mute) {
        if (this.localStream) {
            this.localStream.getAudioTracks().forEach(track => {
                track.enabled = !mute;
            });
            return true;
        }
        return false;
    },

    // Toggle video mute
    toggleVideo: function(mute) {
        if (this.localStream) {
            this.localStream.getVideoTracks().forEach(track => {
                track.enabled = !mute;
            });
            // Update local video state
            this.updateVideoState(true, !mute);
            return true;
        }
        return false;
    },

    // Switch camera (front/back)
    switchCamera: async function(videoElementId) {
        try {
            const videoTrack = this.localStream.getVideoTracks()[0];
            const currentFacingMode = videoTrack.getSettings().facingMode;
            const newFacingMode = currentFacingMode === 'user' ? 'environment' : 'user';

            // Stop current video track
            videoTrack.stop();

            // Get new stream with switched camera
            const newStream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: newFacingMode },
                audio: false
            });

            const newVideoTrack = newStream.getVideoTracks()[0];

            // Replace track in peer connection
            if (this.peerConnection) {
                const sender = this.peerConnection.getSenders().find(s => s.track && s.track.kind === 'video');
                if (sender) {
                    await sender.replaceTrack(newVideoTrack);
                }
            }

            // Replace track in local stream
            this.localStream.removeTrack(videoTrack);
            this.localStream.addTrack(newVideoTrack);

            // Update video element
            const videoElement = document.getElementById(videoElementId);
            if (videoElement) {
                videoElement.srcObject = this.localStream;
            }

            return true;
        } catch (err) {
            console.error('Error switching camera:', err);
            return false;
        }
    },

    // End call and cleanup
    endCall: function() {
        try {
            // Stop local stream (but keep shared stream for other tabs)
            if (this.localStream) {
                this.localStream.getTracks().forEach(track => track.stop());
                this.localStream = null;
            }

            // Stop remote stream
            if (this.remoteStream) {
                this.remoteStream.getTracks().forEach(track => track.stop());
                this.remoteStream = null;
            }

            // Close peer connection
            if (this.peerConnection) {
                this.peerConnection.close();
                this.peerConnection = null;
            }

            console.log('Call ended and resources cleaned up');
            return true;
        } catch (err) {
            console.error('Error ending call:', err);
            return false;
        }
    },

    // Get call statistics
    getStats: async function() {
        if (!this.peerConnection) return null;

        try {
            const stats = await this.peerConnection.getStats();
            const statsReport = {
                audio: {},
                video: {},
                connection: {}
            };

            stats.forEach(report => {
                if (report.type === 'inbound-rtp' && report.kind === 'audio') {
                    statsReport.audio.bytesReceived = report.bytesReceived;
                    statsReport.audio.packetsLost = report.packetsLost;
                }
                if (report.type === 'inbound-rtp' && report.kind === 'video') {
                    statsReport.video.bytesReceived = report.bytesReceived;
                    statsReport.video.frameRate = report.framesPerSecond;
                }
                if (report.type === 'candidate-pair' && report.state === 'succeeded') {
                    statsReport.connection.roundTripTime = report.currentRoundTripTime;
                }
            });

            return JSON.stringify(statsReport);
        } catch (err) {
            console.error('Error getting stats:', err);
            return null;
        }
    },

    // Simulate connection for testing
    simulateConnection: function() {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnConnectionStateChanged', 'connected');
        }
    },

    // Show/hide thumbnails based on video state
    updateVideoState: function(isLocalVideo, hasVideo) {
        const thumbnailId = isLocalVideo ? 'localThumbnail' : 'remoteThumbnail';
        const videoId = isLocalVideo ? 'localVideo' : 'remoteVideo';
        
        const thumbnail = document.getElementById(thumbnailId);
        const video = document.getElementById(videoId);
        
        if (thumbnail && video) {
            if (hasVideo) {
                thumbnail.classList.add('hidden');
                video.classList.remove('hidden');
            } else {
                thumbnail.classList.remove('hidden');
                video.classList.add('hidden');
            }
        }
    },

    // Update remote video state when track is received
    updateRemoteVideoState: function(hasVideo) {
        this.updateVideoState(false, hasVideo);
    },

    // Monitor remote stream for video track changes
    monitorRemoteStream: function() {
        if (this.remoteStream) {
            const videoTracks = this.remoteStream.getVideoTracks();
            if (videoTracks.length > 0) {
                const videoTrack = videoTracks[0];
                this.updateRemoteVideoState(videoTrack.enabled);
                
                setInterval(() => {
                    if (videoTrack.readyState === 'ended') {
                        this.updateRemoteVideoState(false);
                    }
                }, 1000);
            }
        }
    },

    // Monitor video frames to detect black screen
    monitorVideoFrames: function(videoTrack, videoElement) {
        if (!videoElement) return;
        
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        canvas.width = 320;
        canvas.height = 240;
        
        const checkFrames = () => {
            if (videoTrack.readyState === 'ended' || !videoTrack.enabled) {
                this.updateRemoteVideoState(false);
                return;
            }
            
            try {
                ctx.drawImage(videoElement, 0, 0, canvas.width, canvas.height);
                const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
                const data = imageData.data;
                
                // Check if image is mostly black (camera off)
                let blackPixels = 0;
                for (let i = 0; i < data.length; i += 4) {
                    const r = data[i];
                    const g = data[i + 1];
                    const b = data[i + 2];
                    if (r < 30 && g < 30 && b < 30) {
                        blackPixels++;
                    }
                }
                
                const blackPercentage = blackPixels / (canvas.width * canvas.height);
                this.updateRemoteVideoState(blackPercentage < 0.9);
            } catch (e) {
                // Video not ready yet
            }
        };
        
        setInterval(checkFrames, 1000);
    },

    // Handle missing video elements gracefully
    handleMissingVideoElement: function(elementId, isAudioOnly) {
        if (isAudioOnly) {
            console.log('Audio-only call, video element not required');
            return true;
        }
        console.warn('Video element not found:', elementId);
        return false;
    }
};
