/**
 * Speech Recognition - Provides Web Speech API integration for voice-to-text
 */
window.speechRecognition = (function() {
    let recognition = null;
    let dotNetReference = null;
    let isRecognizing = false;
    let finalTranscript = '';

    // Initialize Speech Recognition
    function initializeRecognition() {
        // Check if browser supports Web Speech API
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;

        if (!SpeechRecognition) {
            console.error('Speech Recognition not supported in this browser');
            return null;
        }

        recognition = new SpeechRecognition();
        recognition.continuous = true;
        recognition.interimResults = true;
        recognition.lang = 'en-US';

        // Handle recognition results
        recognition.onresult = function(event) {
            let interimTranscript = '';

            for (let i = event.resultIndex; i < event.results.length; i++) {
                const transcript = event.results[i][0].transcript;

                if (event.results[i].isFinal) {
                    finalTranscript += transcript + ' ';
                } else {
                    interimTranscript += transcript;
                }
            }

            // Send result to Blazor
            if (dotNetReference && finalTranscript) {
                dotNetReference.invokeMethodAsync('HandleRecognitionResult', finalTranscript.trim());
            }
        };

        // Handle recognition errors
        recognition.onerror = function(event) {
            console.error('Speech recognition error:', event.error);
            isRecognizing = false;

            if (dotNetReference) {
                dotNetReference.invokeMethodAsync('HandleRecognitionError', event.error);
            }
        };

        // Handle recognition end
        recognition.onend = function() {
            isRecognizing = false;
            console.log('Speech recognition ended');
        };

        return recognition;
    }

    return {
        /**
         * Starts speech recognition
         * @param {DotNetObjectReference} dotNetRef - Reference to .NET object
         * @returns {boolean} True if started successfully
         */
        start: function(dotNetRef) {
            try {
                if (isRecognizing) {
                    console.warn('Recognition already in progress');
                    return false;
                }

                dotNetReference = dotNetRef;

                if (!recognition) {
                    recognition = initializeRecognition();
                }

                if (!recognition) {
                    console.error('Failed to initialize speech recognition');
                    return false;
                }

                finalTranscript = '';
                recognition.start();
                isRecognizing = true;
                console.log('Speech recognition started');

                return true;
            } catch (error) {
                console.error('Error starting speech recognition:', error);
                isRecognizing = false;
                return false;
            }
        },

        /**
         * Stops speech recognition
         * @returns {string} The final transcript
         */
        stop: function() {
            try {
                if (!isRecognizing || !recognition) {
                    console.warn('No recognition in progress');
                    return '';
                }

                recognition.stop();
                isRecognizing = false;
                console.log('Speech recognition stopped');

                return finalTranscript.trim();
            } catch (error) {
                console.error('Error stopping speech recognition:', error);
                isRecognizing = false;
                return '';
            }
        },

        /**
         * Checks if speech recognition is supported
         * @returns {boolean} True if supported
         */
        isSupported: function() {
            return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
        },

        /**
         * Gets the current recognition status
         * @returns {boolean} True if recognizing
         */
        isRecognizing: function() {
            return isRecognizing;
        }
    };
})();
