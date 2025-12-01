// Enhanced attachment handling functions

window.downloadFile = (url, filename) => {
    const link = document.createElement('a');
    link.href = url;
    link.download = filename || 'attachment';
    link.target = '_blank';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.previewAttachment = (url, type) => {
    if (type === 'pdf') {
        // Try to open PDF in new tab for better viewing
        window.open(url, '_blank');
    } else {
        // For other files, trigger download
        window.downloadFile(url);
    }
};

// Handle PDF iframe loading errors
window.handlePdfError = (iframe) => {
    const fallback = document.getElementById('pdf-fallback');
    if (fallback) {
        iframe.style.display = 'none';
        fallback.style.display = 'flex';
    }
};

// Scroll to specific message with highlight effect
window.scrollToMessage = (messageId) => {
    const messageElement = document.getElementById(`message-${messageId}`);
    if (messageElement) {
        messageElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
        
        // Add highlight effect with padding
        messageElement.style.transition = 'all 0.3s ease';
        messageElement.style.backgroundColor = 'rgba(59, 130, 246, 0.2)';
        messageElement.style.padding = '8px';
        messageElement.style.borderRadius = '8px';
        
        // Remove highlight after 2 seconds
        setTimeout(() => {
            messageElement.style.backgroundColor = 'transparent';
            messageElement.style.padding = '0';
        }, 2000);
    }
};

// Initialize PDF error handling
document.addEventListener('DOMContentLoaded', function() {
    const pdfIframes = document.querySelectorAll('iframe[src$=".pdf"]');
    pdfIframes.forEach(iframe => {
        iframe.addEventListener('error', () => handlePdfError(iframe));
    });
});