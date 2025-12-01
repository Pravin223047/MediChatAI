// Enhanced WebRTC with Multi-Peer Support for Consultation Sessions
window.webrtcMultiPeer = {
    peers: new Map(), // Map<participantId, RTCPeerConnection>
    localStream: null,
    screenStream: null,
    dotNetRef: null,
    isScreenSharing: false,
    networkMonitor: null,

    config: {
        iceServers: [
            { urls: 'stun:stun.l.google.com:19302' },
            { urls: 'stun:stun1.l.google.com:19302' },
            { urls: 'stun:stun2.l.google.com:19302' },
            { urls: 'stun:stun3.l.google.com:19302' },
            { urls: 'stun:stun4.l.google.com:19302' }
        ],
        iceTransportPolicy: 'all',
        bundlePolicy: 'max-bundle',
        rtcpMuxPolicy: 'require'
    },

    initialize(dotNetReference) {
        this.dotNetRef = dotNetReference;
        this.startNetworkMonitoring();
        console.log('Multi-peer WebRTC initialized');
    },

    // ==================== LOCAL STREAM ====================

    async startLocalStream(videoElementId, constraints = {}) {
        try {
            const defaultConstraints = {
                audio: {
                    echoCancellation: true,
                    noiseSuppression: true,
                    autoGainControl: true,
                    sampleRate: 48000
                },
                video: {
                    width: { ideal: 1280, max: 1920 },
                    height: { ideal: 720, max: 1080 },
                    frameRate: { ideal: 30, max: 60 },
                    facingMode: 'user'
                }
            };

            const finalConstraints = { ...defaultConstraints, ...constraints };
            this.localStream = await navigator.mediaDevices.getUserMedia(finalConstraints);

            const videoElement = document.getElementById(videoElementId);
            if (videoElement) {
                videoElement.srcObject = this.localStream;
                videoElement.muted = true;
                videoElement.autoplay = true;
                videoElement.playsInline = true;
                await videoElement.play();
            }

            console.log('Local stream started with', this.localStream.getTracks().length, 'tracks');
            return { success: true, trackCount: this.localStream.getTracks().length };
        } catch (err) {
            console.error('Error starting local stream:', err);
            this.dotNetRef?.invokeMethodAsync('OnMediaError', err.message);
            return { success: false, error: err.message };
        }
    },

    toggleAudio(enabled) {
        if (!this.localStream) return false;
        this.localStream.getAudioTracks().forEach(track => track.enabled = enabled);
        return true;
    },

    toggleVideo(enabled) {
        if (!this.localStream) return false;
        this.localStream.getVideoTracks().forEach(track => track.enabled = enabled);
        return true;
    },

    // ==================== SCREEN SHARING ====================

    async startScreenShare(screenElementId, includeAudio = false) {
        try {
            this.screenStream = await navigator.mediaDevices.getDisplayMedia({
                video: {
                    cursor: 'always',
                    displaySurface: 'monitor',
                    logicalSurface: true,
                    width: { ideal: 1920, max: 3840 },
                    height: { ideal: 1080, max: 2160 },
                    frameRate: { ideal: 30, max: 60 }
                },
                audio: includeAudio
            });

            const screenElement = document.getElementById(screenElementId);
            if (screenElement) {
                screenElement.srcObject = this.screenStream;
                screenElement.autoplay = true;
                screenElement.playsInline = true;
                await screenElement.play();
            }

            // Replace video track in all peer connections
            const screenTrack = this.screenStream.getVideoTracks()[0];
            screenTrack.addEventListener('ended', () => {
                this.stopScreenShare();
            });

            for (const [participantId, peer] of this.peers) {
                const sender = peer.getSenders().find(s => s.track?.kind === 'video');
                if (sender) {
                    await sender.replaceTrack(screenTrack);
                }
            }

            this.isScreenSharing = true;
            this.dotNetRef?.invokeMethodAsync('OnScreenShareStarted');
            console.log('Screen sharing started');
            return { success: true };
        } catch (err) {
            console.error('Error starting screen share:', err);
            return { success: false, error: err.message };
        }
    },

    async stopScreenShare() {
        if (!this.screenStream) return { success: false };

        try {
            this.screenStream.getTracks().forEach(track => track.stop());
            this.screenStream = null;
            this.isScreenSharing = false;

            // Restore camera video track
            if (this.localStream) {
                const videoTrack = this.localStream.getVideoTracks()[0];
                for (const [participantId, peer] of this.peers) {
                    const sender = peer.getSenders().find(s => s.track?.kind === 'video');
                    if (sender && videoTrack) {
                        await sender.replaceTrack(videoTrack);
                    }
                }
            }

            this.dotNetRef?.invokeMethodAsync('OnScreenShareStopped');
            console.log('Screen sharing stopped');
            return { success: true };
        } catch (err) {
            console.error('Error stopping screen share:', err);
            return { success: false, error: err.message };
        }
    },

    // ==================== MULTI-PEER MANAGEMENT ====================

    async addPeer(participantId, isInitiator, remoteElementId) {
        try {
            if (this.peers.has(participantId)) {
                console.warn('Peer already exists:', participantId);
                return { success: false, error: 'Peer exists' };
            }

            const peer = new RTCPeerConnection(this.config);
            this.setupPeerListeners(peer, participantId, remoteElementId);

            // Add local tracks
            if (this.localStream) {
                this.localStream.getTracks().forEach(track => {
                    peer.addTrack(track, this.localStream);
                });
            }

            this.peers.set(participantId, peer);

            if (isInitiator) {
                const offer = await peer.createOffer();
                await peer.setLocalDescription(offer);
                console.log('Created offer for', participantId);
                return { success: true, offer: JSON.stringify(offer) };
            }

            console.log('Added peer (waiting for offer):', participantId);
            return { success: true };
        } catch (err) {
            console.error('Error adding peer:', err);
            return { success: false, error: err.message };
        }
    },

    async removePeer(participantId) {
        const peer = this.peers.get(participantId);
        if (!peer) return { success: false };

        try {
            peer.close();
            this.peers.delete(participantId);

            // Clear video element
            const remoteElement = document.getElementById(`remote-${participantId}`);
            if (remoteElement) {
                remoteElement.srcObject = null;
            }

            console.log('Removed peer:', participantId);
            return { success: true };
        } catch (err) {
            console.error('Error removing peer:', err);
            return { success: false, error: err.message };
        }
    },

    async handleOffer(participantId, offerJson, remoteElementId) {
        try {
            let peer = this.peers.get(participantId);
            if (!peer) {
                const result = await this.addPeer(participantId, false, remoteElementId);
                if (!result.success) return result;
                peer = this.peers.get(participantId);
            }

            const offer = JSON.parse(offerJson);
            await peer.setRemoteDescription(new RTCSessionDescription(offer));

            const answer = await peer.createAnswer();
            await peer.setLocalDescription(answer);

            console.log('Created answer for', participantId);
            return { success: true, answer: JSON.stringify(answer) };
        } catch (err) {
            console.error('Error handling offer:', err);
            return { success: false, error: err.message };
        }
    },

    async handleAnswer(participantId, answerJson) {
        try {
            const peer = this.peers.get(participantId);
            if (!peer) return { success: false, error: 'Peer not found' };

            const answer = JSON.parse(answerJson);
            await peer.setRemoteDescription(new RTCSessionDescription(answer));

            console.log('Set remote answer for', participantId);
            return { success: true };
        } catch (err) {
            console.error('Error handling answer:', err);
            return { success: false, error: err.message };
        }
    },

    async addIceCandidate(participantId, candidateJson) {
        try {
            const peer = this.peers.get(participantId);
            if (!peer) return { success: false };

            const candidate = JSON.parse(candidateJson);
            await peer.addIceCandidate(new RTCIceCandidate(candidate));
            return { success: true };
        } catch (err) {
            console.error('Error adding ICE candidate:', err);
            return { success: false, error: err.message };
        }
    },

    setupPeerListeners(peer, participantId, remoteElementId) {
        peer.onicecandidate = (event) => {
            if (event.candidate) {
                this.dotNetRef?.invokeMethodAsync('OnIceCandidate',
                    participantId, JSON.stringify(event.candidate));
            }
        };

        peer.ontrack = (event) => {
            console.log('Received track from', participantId, ':', event.track.kind);
            const remoteElement = document.getElementById(remoteElementId || `remote-${participantId}`);
            if (remoteElement) {
                if (!remoteElement.srcObject) {
                    remoteElement.srcObject = new MediaStream();
                }
                remoteElement.srcObject.addTrack(event.track);
                remoteElement.autoplay = true;
                remoteElement.playsInline = true;
                remoteElement.play().catch(e => console.log('Remote play failed:', e));
            }
        };

        peer.oniceconnectionstatechange = () => {
            console.log('ICE state for', participantId, ':', peer.iceConnectionState);
            this.dotNetRef?.invokeMethodAsync('OnConnectionStateChanged',
                participantId, peer.iceConnectionState);

            if (peer.iceConnectionState === 'failed' || peer.iceConnectionState === 'disconnected') {
                this.handleReconnection(participantId);
            }
        };

        peer.onconnectionstatechange = () => {
            console.log('Connection state for', participantId, ':', peer.connectionState);
        };
    },

    // ==================== NETWORK QUALITY ====================

    startNetworkMonitoring() {
        this.networkMonitor = setInterval(async () => {
            const qualities = {};

            for (const [participantId, peer] of this.peers) {
                try {
                    const stats = await peer.getStats();
                    const quality = this.analyzeStats(stats);
                    qualities[participantId] = quality;
                } catch (err) {
                    console.error('Error getting stats for', participantId, err);
                }
            }

            if (Object.keys(qualities).length > 0) {
                this.dotNetRef?.invokeMethodAsync('OnNetworkQualityUpdate', qualities);
            }
        }, 5000); // Every 5 seconds
    },

    analyzeStats(stats) {
        let quality = { level: 'good', rtt: 0, packetLoss: 0, bitrate: 0 };

        stats.forEach(report => {
            if (report.type === 'candidate-pair' && report.state === 'succeeded') {
                quality.rtt = Math.round((report.currentRoundTripTime || 0) * 1000);
            }

            if (report.type === 'inbound-rtp') {
                const packetsLost = report.packetsLost || 0;
                const packetsReceived = report.packetsReceived || 0;
                if (packetsReceived > 0) {
                    quality.packetLoss = (packetsLost / (packetsLost + packetsReceived)) * 100;
                }
                quality.bitrate = report.bytesReceived || 0;
            }
        });

        // Determine quality level
        if (quality.rtt > 300 || quality.packetLoss > 5) {
            quality.level = 'poor';
        } else if (quality.rtt > 150 || quality.packetLoss > 2) {
            quality.level = 'fair';
        }

        return quality;
    },

    stopNetworkMonitoring() {
        if (this.networkMonitor) {
            clearInterval(this.networkMonitor);
            this.networkMonitor = null;
        }
    },

    // ==================== RECONNECTION ====================

    async handleReconnection(participantId) {
        console.log('Attempting reconnection for', participantId);

        const peer = this.peers.get(participantId);
        if (!peer) return;

        try {
            // Create new offer for ICE restart
            const offer = await peer.createOffer({ iceRestart: true });
            await peer.setLocalDescription(offer);

            this.dotNetRef?.invokeMethodAsync('OnReconnectionNeeded',
                participantId, JSON.stringify(offer));
        } catch (err) {
            console.error('Reconnection failed for', participantId, err);
        }
    },

    // ==================== BANDWIDTH ADAPTATION ====================

    async adaptBandwidth(maxBitrate) {
        for (const [participantId, peer] of this.peers) {
            const senders = peer.getSenders();
            for (const sender of senders) {
                if (sender.track?.kind === 'video') {
                    const parameters = sender.getParameters();
                    if (!parameters.encodings) {
                        parameters.encodings = [{}];
                    }
                    parameters.encodings[0].maxBitrate = maxBitrate * 1000; // Convert to bps
                    await sender.setParameters(parameters);
                }
            }
        }
        console.log('Adapted bandwidth to', maxBitrate, 'kbps');
    },

    // ==================== CLEANUP ====================

    endCall() {
        try {
            // Stop local stream
            if (this.localStream) {
                this.localStream.getTracks().forEach(track => track.stop());
                this.localStream = null;
            }

            // Stop screen share
            if (this.screenStream) {
                this.screenStream.getTracks().forEach(track => track.stop());
                this.screenStream = null;
            }

            // Close all peer connections
            for (const [participantId, peer] of this.peers) {
                peer.close();
            }
            this.peers.clear();

            // Stop network monitoring
            this.stopNetworkMonitoring();

            console.log('Call ended, all resources cleaned up');
            return { success: true };
        } catch (err) {
            console.error('Error ending call:', err);
            return { success: false, error: err.message };
        }
    },

    // ==================== UTILITY ====================

    getPeerCount() {
        return this.peers.size;
    },

    getPeerIds() {
        return Array.from(this.peers.keys());
    },

    getConnectionState(participantId) {
        const peer = this.peers.get(participantId);
        return peer ? peer.connectionState : 'closed';
    },

    async getMediaDevices() {
        try {
            const devices = await navigator.mediaDevices.enumerateDevices();
            return {
                videoInputs: devices.filter(d => d.kind === 'videoinput'),
                audioInputs: devices.filter(d => d.kind === 'audioinput'),
                audioOutputs: devices.filter(d => d.kind === 'audiooutput')
            };
        } catch (err) {
            console.error('Error enumerating devices:', err);
            return { videoInputs: [], audioInputs: [], audioOutputs: [] };
        }
    }
};
