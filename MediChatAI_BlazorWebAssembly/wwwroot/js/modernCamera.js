// Modern Camera with AI-powered features
// Author: MediChatAI
// Features: Quality indicators, annotation, video recording, AI filters

let currentStream = null;
let mediaRecorder = null;
let recordedChunks = [];
let isRecording = false;
let videoQualityCheckInterval = null;

/**
 * Initialize camera with high quality settings
 */
window.initializeModernCamera = async function (videoElement, facingMode = 'user') {
    try {
        // Validate video element
        if (!videoElement) {
            throw new Error('Video element is null or undefined');
        }

        console.log('Initializing camera with facingMode:', facingMode);

        // Stop any existing stream
        if (currentStream) {
            currentStream.getTracks().forEach(track => track.stop());
            currentStream = null;
        }

        // Request camera with optimal settings
        const constraints = {
            video: {
                facingMode: facingMode,
                width: { ideal: 1920, max: 3840 },
                height: { ideal: 1080, max: 2160 },
                aspectRatio: { ideal: 16 / 9 },
                frameRate: { ideal: 30 }
            },
            audio: false
        };

        console.log('Requesting camera access...');
        currentStream = await navigator.mediaDevices.getUserMedia(constraints);

        console.log('Camera access granted, setting up video element...');

        // Set the stream to video element
        videoElement.srcObject = currentStream;

        // Set video attributes for better compatibility
        videoElement.muted = true;
        videoElement.playsInline = true;
        videoElement.autoplay = true;

        // Wait for video metadata to load
        await new Promise((resolve, reject) => {
            videoElement.onloadedmetadata = () => {
                console.log('Video metadata loaded');
                resolve();
            };
            videoElement.onerror = (error) => {
                console.error('Video element error:', error);
                reject(error);
            };

            // Timeout after 5 seconds
            setTimeout(() => reject(new Error('Video metadata load timeout')), 5000);
        });

        // Explicitly play the video
        try {
            await videoElement.play();
            console.log('Video playing successfully');
        } catch (playError) {
            console.warn('Video play error (may be autoplay restriction):', playError);
            // Try to play again after user interaction
        }

        // Start quality monitoring
        startQualityMonitoring(videoElement);

        const videoTrack = currentStream.getVideoTracks()[0];
        const settings = videoTrack.getSettings();

        console.log('Camera initialized successfully:', settings);

        return {
            success: true,
            videoTrack: settings
        };
    } catch (error) {
        console.error('Camera initialization error:', error);

        // Clean up on error
        if (currentStream) {
            currentStream.getTracks().forEach(track => track.stop());
            currentStream = null;
        }

        return {
            success: false,
            error: error.message || 'Failed to initialize camera'
        };
    }
};

/**
 * Start video recording
 */
window.startVideoRecording = async function (videoElement) {
    try {
        if (!currentStream) {
            throw new Error('No active camera stream');
        }

        // Create MediaRecorder with video stream
        const options = {
            mimeType: 'video/webm;codecs=vp9',
            videoBitsPerSecond: 2500000 // 2.5 Mbps
        };

        // Fallback for Safari
        if (!MediaRecorder.isTypeSupported(options.mimeType)) {
            options.mimeType = 'video/webm';
        }

        mediaRecorder = new MediaRecorder(currentStream, options);
        recordedChunks = [];

        mediaRecorder.ondataavailable = (event) => {
            if (event.data && event.data.size > 0) {
                recordedChunks.push(event.data);
            }
        };

        mediaRecorder.start(100); // Collect data every 100ms
        isRecording = true;

        return { success: true };
    } catch (error) {
        console.error('Video recording error:', error);
        return { success: false, error: error.message };
    }
};

/**
 * Stop video recording and return blob URL
 */
window.stopVideoRecording = async function () {
    try {
        if (!mediaRecorder || !isRecording) {
            throw new Error('No active recording');
        }

        return new Promise((resolve, reject) => {
            mediaRecorder.onstop = () => {
                const blob = new Blob(recordedChunks, { type: 'video/webm' });
                const url = URL.createObjectURL(blob);
                isRecording = false;

                // Convert to base64 for easier transfer to Blazor
                const reader = new FileReader();
                reader.onloadend = () => {
                    resolve({
                        success: true,
                        videoUrl: url,
                        videoData: reader.result,
                        duration: recordedChunks.length * 0.1, // Approximate duration
                        size: blob.size
                    });
                };
                reader.readAsDataURL(blob);
            };

            mediaRecorder.onerror = (error) => {
                reject(error);
            };

            mediaRecorder.stop();
        });
    } catch (error) {
        console.error('Stop recording error:', error);
        return { success: false, error: error.message };
    }
};

/**
 * Capture photo with AI enhancements
 */
window.capturePhotoWithEnhancements = async function (videoElement, canvasElement, applyAI = true) {
    try {
        const context = canvasElement.getContext('2d');

        // Set canvas size to match video
        canvasElement.width = videoElement.videoWidth;
        canvasElement.height = videoElement.videoHeight;

        // Draw video frame to canvas
        context.drawImage(videoElement, 0, 0);

        // Apply AI enhancements if requested
        if (applyAI) {
            applyAIEnhancements(context, canvasElement.width, canvasElement.height);
        }

        // Get image data
        const imageData = canvasElement.toDataURL('image/jpeg', 0.95);

        return {
            success: true,
            imageData: imageData,
            width: canvasElement.width,
            height: canvasElement.height
        };
    } catch (error) {
        console.error('Photo capture error:', error);
        return { success: false, error: error.message };
    }
};

/**
 * Apply AI-powered image enhancements
 */
function applyAIEnhancements(context, width, height) {
    const imageData = context.getImageData(0, 0, width, height);
    const data = imageData.data;

    // Auto brightness and contrast adjustment
    const brightness = calculateOptimalBrightness(data);
    const contrast = 1.2; // Increase contrast slightly for medical clarity

    for (let i = 0; i < data.length; i += 4) {
        // Brightness adjustment
        data[i] = Math.min(255, data[i] + brightness);     // Red
        data[i + 1] = Math.min(255, data[i + 1] + brightness); // Green
        data[i + 2] = Math.min(255, data[i + 2] + brightness); // Blue

        // Contrast adjustment
        data[i] = Math.min(255, Math.max(0, (data[i] - 128) * contrast + 128));
        data[i + 1] = Math.min(255, Math.max(0, (data[i + 1] - 128) * contrast + 128));
        data[i + 2] = Math.min(255, Math.max(0, (data[i + 2] - 128) * contrast + 128));
    }

    context.putImageData(imageData, 0, 0);
}

/**
 * Calculate optimal brightness adjustment
 */
function calculateOptimalBrightness(data) {
    let totalBrightness = 0;
    const sampleSize = data.length / 4; // Total pixels

    for (let i = 0; i < data.length; i += 4) {
        // Calculate perceived brightness
        const brightness = (0.299 * data[i] + 0.587 * data[i + 1] + 0.114 * data[i + 2]);
        totalBrightness += brightness;
    }

    const averageBrightness = totalBrightness / sampleSize;
    const targetBrightness = 128; // Mid-range brightness

    return (targetBrightness - averageBrightness) * 0.3; // Gentle adjustment
}

/**
 * Analyze image quality in real-time
 */
window.analyzeImageQuality = function (videoElement, canvasElement) {
    try {
        // Validate inputs
        if (!videoElement || !canvasElement) {
            return null;
        }

        if (!videoElement.videoWidth || videoElement.videoWidth === 0) {
            return null;
        }

        const context = canvasElement.getContext('2d');
        if (!context) {
            return null;
        }

        canvasElement.width = videoElement.videoWidth;
        canvasElement.height = videoElement.videoHeight;

        if (canvasElement.width === 0 || canvasElement.height === 0) {
            return null;
        }

        context.drawImage(videoElement, 0, 0);

        const imageData = context.getImageData(0, 0, canvasElement.width, canvasElement.height);
        const data = imageData.data;

        if (!data || data.length === 0) {
            return null;
        }

        // Calculate various quality metrics
        const brightness = calculateBrightness(data);
        const contrast = calculateContrast(data);
        const sharpness = calculateSharpness(imageData);
        const resolution = { width: canvasElement.width, height: canvasElement.height };

        // Determine quality score (0-100)
        const qualityScore = calculateOverallQuality(brightness, contrast, sharpness, resolution);

        return {
            brightness: Math.round(brightness),
            contrast: Math.round(contrast * 100),
            sharpness: Math.round(sharpness * 100),
            resolution: resolution,
            qualityScore: qualityScore,
            recommendation: getQualityRecommendation(brightness, contrast, sharpness)
        };
    } catch (error) {
        console.error('Quality analysis error:', error);
        return null;
    }
};

function calculateBrightness(data) {
    let total = 0;
    for (let i = 0; i < data.length; i += 4) {
        total += (0.299 * data[i] + 0.587 * data[i + 1] + 0.114 * data[i + 2]);
    }
    return total / (data.length / 4);
}

function calculateContrast(data) {
    const brightness = calculateBrightness(data);
    let variance = 0;

    for (let i = 0; i < data.length; i += 4) {
        const pixelBrightness = (0.299 * data[i] + 0.587 * data[i + 1] + 0.114 * data[i + 2]);
        variance += Math.pow(pixelBrightness - brightness, 2);
    }

    return Math.sqrt(variance / (data.length / 4)) / 128; // Normalized
}

function calculateSharpness(imageData) {
    // Simple edge detection for sharpness
    const data = imageData.data;
    const width = imageData.width;
    let edgeStrength = 0;
    let edgeCount = 0;

    for (let y = 1; y < imageData.height - 1; y++) {
        for (let x = 1; x < width - 1; x++) {
            const idx = (y * width + x) * 4;
            const rightIdx = (y * width + (x + 1)) * 4;
            const bottomIdx = ((y + 1) * width + x) * 4;

            const gx = Math.abs(data[rightIdx] - data[idx]);
            const gy = Math.abs(data[bottomIdx] - data[idx]);
            const edge = Math.sqrt(gx * gx + gy * gy);

            if (edge > 30) { // Threshold for edge
                edgeStrength += edge;
                edgeCount++;
            }
        }
    }

    return edgeCount > 0 ? Math.min(1, edgeStrength / (edgeCount * 255)) : 0;
}

function calculateOverallQuality(brightness, contrast, sharpness, resolution) {
    // Brightness score (optimal: 120-140)
    const brightnessScore = 100 - Math.min(100, Math.abs(130 - brightness) * 2);

    // Contrast score (optimal: > 0.3)
    const contrastScore = Math.min(100, contrast * 300);

    // Sharpness score
    const sharpnessScore = sharpness * 100;

    // Resolution score (optimal: >= 1920x1080)
    const resolutionScore = Math.min(100, (resolution.width / 1920) * 100);

    // Weighted average
    return Math.round(
        brightnessScore * 0.3 +
        contrastScore * 0.25 +
        sharpnessScore * 0.25 +
        resolutionScore * 0.2
    );
}

function getQualityRecommendation(brightness, contrast, sharpness) {
    const issues = [];

    if (brightness < 80) {
        issues.push('Increase lighting - image is too dark');
    } else if (brightness > 180) {
        issues.push('Reduce lighting - image is too bright');
    }

    if (contrast < 0.2) {
        issues.push('Improve contrast - ensure good lighting difference');
    }

    if (sharpness < 0.2) {
        issues.push('Hold camera steady - image appears blurry');
    }

    if (issues.length === 0) {
        return 'Image quality is good!';
    }

    return issues.join('; ');
}

/**
 * Start quality monitoring
 */
function startQualityMonitoring(videoElement) {
    if (videoQualityCheckInterval) {
        clearInterval(videoQualityCheckInterval);
    }

    // Check quality every 500ms and dispatch custom event
    videoQualityCheckInterval = setInterval(() => {
        if (videoElement && videoElement.videoWidth > 0) {
            const canvas = document.createElement('canvas');
            const quality = window.analyzeImageQuality(videoElement, canvas);

            if (quality) {
                const event = new CustomEvent('cameraQualityUpdate', { detail: quality });
                window.dispatchEvent(event);
            }
        }
    }, 500);
}

/**
 * Stop camera and cleanup
 */
window.stopModernCamera = function () {
    if (currentStream) {
        currentStream.getTracks().forEach(track => track.stop());
        currentStream = null;
    }

    if (videoQualityCheckInterval) {
        clearInterval(videoQualityCheckInterval);
        videoQualityCheckInterval = null;
    }

    if (mediaRecorder && isRecording) {
        mediaRecorder.stop();
    }

    return { success: true };
};

/**
 * Crop image
 */
window.cropImage = function (imageData, x, y, width, height) {
    const img = new Image();
    img.src = imageData;

    return new Promise((resolve) => {
        img.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = width;
            canvas.height = height;
            const ctx = canvas.getContext('2d');

            ctx.drawImage(img, x, y, width, height, 0, 0, width, height);
            resolve(canvas.toDataURL('image/jpeg', 0.95));
        };
    });
};

/**
 * Rotate image
 */
window.rotateImage = function (imageData, degrees) {
    const img = new Image();
    img.src = imageData;

    return new Promise((resolve) => {
        img.onload = () => {
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');

            // Swap dimensions for 90 or 270 degree rotation
            if (degrees === 90 || degrees === 270) {
                canvas.width = img.height;
                canvas.height = img.width;
            } else {
                canvas.width = img.width;
                canvas.height = img.height;
            }

            ctx.translate(canvas.width / 2, canvas.height / 2);
            ctx.rotate((degrees * Math.PI) / 180);
            ctx.drawImage(img, -img.width / 2, -img.height / 2);

            resolve(canvas.toDataURL('image/jpeg', 0.95));
        };
    });
};

/**
 * Add annotation to image
 */
window.addAnnotationToImage = function (imageData, annotations) {
    const img = new Image();
    img.src = imageData;

    return new Promise((resolve) => {
        img.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = img.width;
            canvas.height = img.height;
            const ctx = canvas.getContext('2d');

            // Draw original image
            ctx.drawImage(img, 0, 0);

            // Draw annotations
            annotations.forEach(annotation => {
                ctx.strokeStyle = annotation.color || '#ff0000';
                ctx.lineWidth = annotation.lineWidth || 3;
                ctx.lineCap = 'round';
                ctx.lineJoin = 'round';

                if (annotation.type === 'circle') {
                    ctx.beginPath();
                    ctx.arc(annotation.x, annotation.y, annotation.radius, 0, 2 * Math.PI);
                    ctx.stroke();
                } else if (annotation.type === 'arrow') {
                    drawArrow(ctx, annotation.x1, annotation.y1, annotation.x2, annotation.y2);
                } else if (annotation.type === 'line') {
                    ctx.beginPath();
                    ctx.moveTo(annotation.x1, annotation.y1);
                    ctx.lineTo(annotation.x2, annotation.y2);
                    ctx.stroke();
                }
            });

            resolve(canvas.toDataURL('image/jpeg', 0.95));
        };
    });
};

function drawArrow(ctx, fromX, fromY, toX, toY) {
    const headLength = 15;
    const angle = Math.atan2(toY - fromY, toX - fromX);

    // Draw line
    ctx.beginPath();
    ctx.moveTo(fromX, fromY);
    ctx.lineTo(toX, toY);
    ctx.stroke();

    // Draw arrowhead
    ctx.beginPath();
    ctx.moveTo(toX, toY);
    ctx.lineTo(
        toX - headLength * Math.cos(angle - Math.PI / 6),
        toY - headLength * Math.sin(angle - Math.PI / 6)
    );
    ctx.moveTo(toX, toY);
    ctx.lineTo(
        toX - headLength * Math.cos(angle + Math.PI / 6),
        toY - headLength * Math.sin(angle + Math.PI / 6)
    );
    ctx.stroke();
}

console.log('Modern Camera JS loaded successfully');
