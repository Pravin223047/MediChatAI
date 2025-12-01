// Speech Synthesis for AI Chatbot Voice Output

window.speechSynthesis = window.speechSynthesis || {};

window.aiChatSpeech = {
    isSpeaking: false,
    currentUtterance: null,

    // Speak text using Web Speech API
    speak: function(text, options = {}) {
        if ('speechSynthesis' in window) {
            // Cancel any ongoing speech
            this.stop();

            const utterance = new SpeechSynthesisUtterance(text);

            // Configure voice options
            utterance.rate = options.rate || 1.0;        // Speed (0.1 to 10)
            utterance.pitch = options.pitch || 1.0;      // Pitch (0 to 2)
            utterance.volume = options.volume || 1.0;    // Volume (0 to 1)
            utterance.lang = options.lang || 'en-US';    // Language

            // Select voice if specified
            if (options.voiceName) {
                const voices = window.speechSynthesis.getVoices();
                const selectedVoice = voices.find(voice => voice.name === options.voiceName);
                if (selectedVoice) {
                    utterance.voice = selectedVoice;
                }
            }

            // Event handlers
            utterance.onstart = () => {
                this.isSpeaking = true;
                console.log('Speech started');
            };

            utterance.onend = () => {
                this.isSpeaking = false;
                console.log('Speech ended');
            };

            utterance.onerror = (event) => {
                console.error('Speech error:', event);
                this.isSpeaking = false;
            };

            this.currentUtterance = utterance;
            window.speechSynthesis.speak(utterance);

            return true;
        } else {
            console.warn('Speech synthesis not supported in this browser');
            return false;
        }
    },

    // Stop current speech
    stop: function() {
        if ('speechSynthesis' in window) {
            window.speechSynthesis.cancel();
            this.isSpeaking = false;
            this.currentUtterance = null;
        }
    },

    // Pause current speech
    pause: function() {
        if ('speechSynthesis' in window && this.isSpeaking) {
            window.speechSynthesis.pause();
        }
    },

    // Resume paused speech
    resume: function() {
        if ('speechSynthesis' in window) {
            window.speechSynthesis.resume();
        }
    },

    // Get available voices
    getVoices: function() {
        if ('speechSynthesis' in window) {
            return window.speechSynthesis.getVoices().map(voice => ({
                name: voice.name,
                lang: voice.lang,
                localService: voice.localService,
                default: voice.default
            }));
        }
        return [];
    },

    // Check if speaking
    isSpeaking: function() {
        return this.isSpeaking;
    }
};

// Load voices (some browsers require this)
if ('speechSynthesis' in window) {
    window.speechSynthesis.addEventListener('voiceschanged', () => {
        console.log('Voices loaded:', window.speechSynthesis.getVoices().length);
    });
}
