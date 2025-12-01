// Emergency Chat JavaScript Interop
// Provides voice recognition, text-to-speech, and UI utilities for the emergency chatbot

window.emergencyChatHelper = {
    // Voice Recognition
    recognition: null,
    speechSynthesis: window.speechSynthesis,
    currentUtterance: null,

    /**
     * Initializes speech recognition
     * @returns {boolean} True if speech recognition is supported
     */
    initializeSpeechRecognition: function () {
        if (!('webkitSpeechRecognition' in window) && !('SpeechRecognition' in window)) {
            console.warn('Speech recognition not supported in this browser');
            return false;
        }

        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        this.recognition = new SpeechRecognition();
        this.recognition.continuous = false;
        this.recognition.interimResults = true;
        this.recognition.maxAlternatives = 1;


        return true;
    },

    /**
     * Starts voice recognition
     * @param {object} dotNetRef - DotNet object reference for callbacks
     * @param {string} language - Language code for recognition
     * @returns {Promise<boolean>} True if started successfully
     */
    startVoiceRecognition: async function (dotNetRef, language = 'en') {
        try {
            if (!this.recognition) {
                const initialized = this.initializeSpeechRecognition();
                if (!initialized) {
                    return false;
                }
            }
            
            // Set recognition language
            const langMap = {
                'en': 'en-US',
                'hi': 'hi-IN',
                'mr': 'mr-IN',
                'bn': 'bn-IN',
                'te': 'te-IN',
                'ta': 'ta-IN',
                'gu': 'gu-IN',
                'kn': 'kn-IN',
                'ml': 'ml-IN',
                'pa': 'pa-IN',
                'ur': 'ur-PK'
            };
            
            this.recognition.lang = langMap[language] || 'en-US';

            this.recognition.onresult = (event) => {
                const transcript = event.results[event.results.length - 1][0].transcript;
                const isFinal = event.results[event.results.length - 1].isFinal;

                if (isFinal) {
                    dotNetRef.invokeMethodAsync('OnVoiceRecognitionResult', transcript, true);
                } else {
                    dotNetRef.invokeMethodAsync('OnVoiceRecognitionResult', transcript, false);
                }
            };

            this.recognition.onerror = (event) => {
                const errorType = event.error;
                console.error('Speech recognition error:', errorType);

                // Filter out non-critical errors
                const ignorableErrors = ['no-speech', 'aborted'];

                if (ignorableErrors.includes(errorType)) {
                    // For no-speech, provide helpful feedback but don't treat as critical error
                    if (errorType === 'no-speech') {
                        dotNetRef.invokeMethodAsync('OnVoiceRecognitionWarning', 'No speech detected. Please speak louder or try again.');
                    }
                    // Don't report aborted errors (user stopped intentionally)
                    console.log('Non-critical error:', errorType);
                } else {
                    // Critical errors that need user attention
                    let userMessage = 'Voice recognition error occurred.';

                    switch (errorType) {
                        case 'not-allowed':
                        case 'service-not-allowed':
                            userMessage = 'Microphone access denied. Please allow microphone permission in your browser settings.';
                            break;
                        case 'network':
                            userMessage = 'Network error. Please check your internet connection.';
                            break;
                        case 'audio-capture':
                            userMessage = 'Microphone not found or not working. Please check your microphone.';
                            break;
                        case 'language-not-supported':
                            userMessage = 'Language not supported for voice recognition.';
                            break;
                        default:
                            userMessage = `Voice recognition error: ${errorType}`;
                    }

                    dotNetRef.invokeMethodAsync('OnVoiceRecognitionError', userMessage);
                }
            };

            this.recognition.onend = () => {
                dotNetRef.invokeMethodAsync('OnVoiceRecognitionEnd');
            };

            this.recognition.start();
            return true;
        } catch (error) {
            console.error('Error starting voice recognition:', error);
            dotNetRef.invokeMethodAsync('OnVoiceRecognitionError', 'Failed to start voice recognition. Please try again.');
            return false;
        }
    },

    /**
     * Stops voice recognition
     */
    stopVoiceRecognition: function () {
        if (this.recognition) {
            this.recognition.stop();
        }
    },

    /**
     * Checks if speech recognition is supported
     * @returns {boolean}
     */
    isSpeechRecognitionSupported: function () {
        return ('webkitSpeechRecognition' in window) || ('SpeechRecognition' in window);
    },

    /**
     * Strips markdown formatting from text for clean speech output
     * @param {string} text - Text with markdown formatting
     * @returns {string} Clean text suitable for speech
     */
    stripMarkdownForSpeech: function (text) {
        if (!text) return '';

        let cleanText = text;

        // Remove bold: **text** or __text__
        cleanText = cleanText.replace(/\*\*(.+?)\*\*/g, '$1');
        cleanText = cleanText.replace(/__(.+?)__/g, '$1');

        // Remove italic: *text* or _text_
        cleanText = cleanText.replace(/\*(.+?)\*/g, '$1');
        cleanText = cleanText.replace(/_(.+?)_/g, '$1');

        // Remove headers: # Header
        cleanText = cleanText.replace(/#{1,6}\s+/g, '');

        // Remove links: [text](url) -> text
        cleanText = cleanText.replace(/\[(.+?)\]\(.+?\)/g, '$1');

        // Remove inline code: `code`
        cleanText = cleanText.replace(/`(.+?)`/g, '$1');

        // Remove code blocks: ```code```
        cleanText = cleanText.replace(/```[\s\S]*?```/g, '');

        // Remove list bullets: - item, * item, + item
        cleanText = cleanText.replace(/^[\-\*\+]\s+/gm, '');
        cleanText = cleanText.replace(/\n[\-\*\+]\s+/g, '\n');

        // Remove numbered lists: 1. item
        cleanText = cleanText.replace(/^\d+\.\s+/gm, '');

        // Remove blockquotes: > quote
        cleanText = cleanText.replace(/^>\s+/gm, '');

        // Remove horizontal rules: --- or ***
        cleanText = cleanText.replace(/^[\-\*]{3,}$/gm, '');

        // Remove strikethrough: ~~text~~
        cleanText = cleanText.replace(/~~(.+?)~~/g, '$1');

        // Replace multiple newlines with a single pause
        cleanText = cleanText.replace(/\n{3,}/g, '. ');
        cleanText = cleanText.replace(/\n{2}/g, '. ');

        // Replace single newlines with commas for natural pauses
        cleanText = cleanText.replace(/\n/g, ', ');

        // Clean up multiple spaces
        cleanText = cleanText.replace(/\s{2,}/g, ' ');

        // Clean up multiple periods/commas
        cleanText = cleanText.replace(/\.{2,}/g, '.');
        cleanText = cleanText.replace(/,{2,}/g, ',');

        // Remove leading/trailing whitespace
        cleanText = cleanText.trim();

        return cleanText;
    },

    /**
     * Speaks text using text-to-speech
     * @param {string} text - Text to speak
     * @param {number} rate - Speech rate (0.1 to 10, default 1)
     * @param {number} pitch - Speech pitch (0 to 2, default 1)
     * @param {number} volume - Speech volume (0 to 1, default 1)
     * @param {string} language - Language code (default 'en')
     * @returns {boolean} True if started successfully
     */
    speak: function (text, rate = 1, pitch = 1, volume = 1, language = 'en') {
        try {
            // Cancel any ongoing speech
            this.stopSpeaking();

            if (!this.speechSynthesis) {
                console.warn('Speech synthesis not supported');
                return false;
            }

            // Strip markdown formatting for clean speech
            const cleanText = this.stripMarkdownForSpeech(text);

            this.currentUtterance = new SpeechSynthesisUtterance(cleanText);
            this.currentUtterance.rate = rate;
            this.currentUtterance.pitch = pitch;
            this.currentUtterance.volume = volume;
            
            // Map language codes to speech synthesis languages
            const langMap = {
                'en': 'en-US',
                'hi': 'hi-IN',
                'mr': 'mr-IN',
                'bn': 'bn-IN',
                'te': 'te-IN',
                'ta': 'ta-IN',
                'gu': 'gu-IN',
                'kn': 'kn-IN',
                'ml': 'ml-IN',
                'pa': 'pa-IN',
                'ur': 'ur-PK'
            };
            
            this.currentUtterance.lang = langMap[language] || 'en-US';



            this.speechSynthesis.speak(this.currentUtterance);
            return true;
        } catch (error) {
            console.error('Error in text-to-speech:', error);
            return false;
        }
    },

    /**
     * Stops any ongoing speech
     */
    stopSpeaking: function () {
        if (this.speechSynthesis && this.speechSynthesis.speaking) {
            this.speechSynthesis.cancel();
        }
    },

    /**
     * Checks if text-to-speech is currently speaking
     * @returns {boolean}
     */
    isSpeaking: function () {
        return this.speechSynthesis && this.speechSynthesis.speaking;
    },

    /**
     * Checks if text-to-speech is supported
     * @returns {boolean}
     */
    isTextToSpeechSupported: function () {
        return 'speechSynthesis' in window;
    },

    /**
     * Gets available voices for text-to-speech
     * @returns {Array} List of available voices
     */
    getAvailableVoices: function () {
        if (!this.speechSynthesis) return [];

        return this.speechSynthesis.getVoices().map(voice => ({
            name: voice.name,
            lang: voice.lang,
            default: voice.default
        }));
    },

    /**
     * Scrolls chat container to bottom smoothly
     * @param {string} containerId - ID of the chat container element
     */
    scrollToBottom: function (containerId) {
        try {
            const container = document.getElementById(containerId);
            if (container) {
                container.scrollTo({
                    top: container.scrollHeight,
                    behavior: 'smooth'
                });
            }
        } catch (error) {
            console.error('Error scrolling to bottom:', error);
        }
    },

    /**
     * Focuses on the message input element
     * @param {string} inputId - ID of the input element
     */
    focusMessageInput: function (inputId) {
        try {
            const input = document.getElementById(inputId);
            if (input) {
                input.focus();
            }
        } catch (error) {
            console.error('Error focusing input:', error);
        }
    },

    /**
     * Copies text to clipboard
     * @param {string} text - Text to copy
     * @returns {Promise<boolean>} True if successful
     */
    copyToClipboard: async function (text) {
        try {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                await navigator.clipboard.writeText(text);
                return true;
            } else {
                // Fallback for older browsers
                const textarea = document.createElement('textarea');
                textarea.value = text;
                textarea.style.position = 'fixed';
                textarea.style.opacity = '0';
                document.body.appendChild(textarea);
                textarea.select();
                const success = document.execCommand('copy');
                document.body.removeChild(textarea);
                return success;
            }
        } catch (error) {
            console.error('Error copying to clipboard:', error);
            return false;
        }
    },

    /**
     * Plays a notification sound
     * @param {string} soundUrl - URL of the sound file
     */
    playNotificationSound: function (soundUrl) {
        try {
            const audio = new Audio(soundUrl);
            audio.volume = 0.5;
            audio.play().catch(error => {
                console.warn('Could not play notification sound:', error);
            });
        } catch (error) {
            console.error('Error playing notification sound:', error);
        }
    },

    /**
     * Vibrates the device (mobile)
     * @param {number} duration - Vibration duration in milliseconds
     */
    vibrate: function (duration = 200) {
        try {
            if ('vibrate' in navigator) {
                navigator.vibrate(duration);
            }
        } catch (error) {
            console.error('Error vibrating device:', error);
        }
    },

    /**
     * Shows browser notification (requires permission)
     * @param {string} title - Notification title
     * @param {string} body - Notification body
     * @param {string} icon - Notification icon URL
     * @returns {Promise<boolean>} True if notification was shown
     */
    showNotification: async function (title, body, icon = null) {
        try {
            if (!('Notification' in window)) {
                return false;
            }

            let permission = Notification.permission;

            if (permission === 'default') {
                permission = await Notification.requestPermission();
            }

            if (permission === 'granted') {
                const options = {
                    body: body,
                    icon: icon || '/icon-192.png',
                    badge: icon || '/icon-192.png',
                    tag: 'emergency-chat',
                    requireInteraction: false
                };

                new Notification(title, options);
                return true;
            }

            return false;
        } catch (error) {
            console.error('Error showing notification:', error);
            return false;
        }
    },

    /**
     * Animates an element with a custom animation
     * @param {string} elementId - ID of the element
     * @param {string} animationClass - CSS animation class to add
     * @param {number} duration - Animation duration in milliseconds
     */
    animateElement: function (elementId, animationClass, duration = 1000) {
        try {
            const element = document.getElementById(elementId);
            if (element) {
                element.classList.add(animationClass);
                setTimeout(() => {
                    element.classList.remove(animationClass);
                }, duration);
            }
        } catch (error) {
            console.error('Error animating element:', error);
        }
    },

    /**
     * Sets up auto-resize for textarea
     * @param {string} textareaId - ID of the textarea element
     */
    setupAutoResize: function (textareaId) {
        try {
            const textarea = document.getElementById(textareaId);
            if (textarea) {
                textarea.style.height = 'auto';
                textarea.style.height = (textarea.scrollHeight) + 'px';

                textarea.addEventListener('input', function () {
                    this.style.height = 'auto';
                    this.style.height = (this.scrollHeight) + 'px';
                });
            }
        } catch (error) {
            console.error('Error setting up auto-resize:', error);
        }
    },

    /**
     * Checks if the user's device is mobile
     * @returns {boolean}
     */
    isMobileDevice: function () {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
    },

    /**
     * Gets the user's preferred language
     * @returns {string}
     */
    getUserLanguage: function () {
        return navigator.language || navigator.userLanguage || 'en-US';
    }
};

// Global scroll function for Emergency page
window.scrollToElement = function(elementId) {
    try {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({ 
                behavior: 'smooth', 
                block: 'start' 
            });
        }
    } catch (error) {
        console.error('Error scrolling to element:', error);
    }
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    console.log('Emergency Chat Helper initialized');

    // Request notification permission on page load (optional)
    if ('Notification' in window && Notification.permission === 'default') {
        // Don't auto-request, wait for user interaction
        console.log('Notification permission available but not requested');
    }
});
