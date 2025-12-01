// Theme.js - Dynamic theme application and system theme detection

/**
 * Detects if the user's system prefers dark mode
 */
window.detectSystemTheme = function() {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
};

/**
 * Applies a theme configuration to the document
 */
window.applyTheme = function(theme) {
    const root = document.documentElement;

    // Set theme mode class on HTML element
    root.classList.remove('light', 'dark');
    root.classList.add(theme.mode);

    // Apply color variables
    root.style.setProperty('--color-primary', theme.primaryColor);
    root.style.setProperty('--color-accent', theme.accentColor);
    root.style.setProperty('--color-background', theme.backgroundColor);
    root.style.setProperty('--color-text', theme.textColor);

    // Font size scaling
    const fontSizeMap = {
        'small': '14px',
        'medium': '16px',
        'large': '18px'
    };
    root.style.setProperty('--base-font-size', fontSizeMap[theme.fontSize] || '16px');

    // Border radius
    const borderRadiusMap = {
        'none': '0px',
        'small': '0.25rem',
        'medium': '0.5rem',
        'large': '1rem'
    };
    root.style.setProperty('--border-radius', borderRadiusMap[theme.borderRadius] || '0.5rem');

    // Compact mode - adjust spacing
    if (theme.compactMode) {
        root.classList.add('compact-mode');
        root.style.setProperty('--spacing-unit', '0.75rem');
    } else {
        root.classList.remove('compact-mode');
        root.style.setProperty('--spacing-unit', '1rem');
    }

    // High contrast mode
    if (theme.highContrast) {
        root.classList.add('high-contrast');
    } else {
        root.classList.remove('high-contrast');
    }

    // Animations
    if (!theme.enableAnimations) {
        root.classList.add('reduce-motion');
    } else {
        root.classList.remove('reduce-motion');
    }

    // Convert hex to RGB for Tailwind classes
    const hexToRgb = (hex) => {
        const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : null;
    };

    const primaryRgb = hexToRgb(theme.primaryColor);
    const accentRgb = hexToRgb(theme.accentColor);

    if (primaryRgb) {
        root.style.setProperty('--color-primary-rgb', `${primaryRgb.r}, ${primaryRgb.g}, ${primaryRgb.b}`);
    }

    if (accentRgb) {
        root.style.setProperty('--color-accent-rgb', `${accentRgb.r}, ${accentRgb.g}, ${accentRgb.b}`);
    }

    console.log('Theme applied:', theme);
};

/**
 * Listen for system theme changes
 */
window.listenForSystemThemeChanges = function(dotNetHelper) {
    if (window.matchMedia) {
        const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');

        darkModeQuery.addEventListener('change', (e) => {
            const isDark = e.matches;
            console.log('System theme changed to:', isDark ? 'dark' : 'light');

            // Notify Blazor component if dotNetHelper is provided
            if (dotNetHelper && dotNetHelper.invokeMethodAsync) {
                dotNetHelper.invokeMethodAsync('OnSystemThemeChanged', isDark ? 'dark' : 'light');
            }
        });
    }
};

/**
 * Get computed color value from CSS variable
 */
window.getThemeColor = function(variableName) {
    return getComputedStyle(document.documentElement).getPropertyValue(variableName).trim();
};

/**
 * Apply doctor-specific theme (independent of admin settings)
 * This function is used by doctors to set their personal theme preferences
 */
window.applyDoctorTheme = function(doctorTheme) {
    const root = document.documentElement;

    // Set theme mode class on HTML element
    root.classList.remove('light', 'dark');
    root.classList.add(doctorTheme.mode);

    // Apply color variables
    root.style.setProperty('--color-primary', doctorTheme.primaryColor);
    root.style.setProperty('--color-accent', doctorTheme.accentColor);

    // Convert hex to RGB for Tailwind classes
    const hexToRgb = (hex) => {
        const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : null;
    };

    const primaryRgb = hexToRgb(doctorTheme.primaryColor);
    const accentRgb = hexToRgb(doctorTheme.accentColor);

    if (primaryRgb) {
        root.style.setProperty('--color-primary-rgb', `${primaryRgb.r}, ${primaryRgb.g}, ${primaryRgb.b}`);
    }

    if (accentRgb) {
        root.style.setProperty('--color-accent-rgb', `${accentRgb.r}, ${accentRgb.g}, ${accentRgb.b}`);
    }

    console.log('Doctor theme applied:', doctorTheme);
};

/**
 * Apply patient-specific theme (independent of admin settings)
 * This function is used by patients to set their personal theme preferences
 */
window.applyPatientTheme = function(patientTheme) {
    console.log('[theme.js] applyPatientTheme called with:', patientTheme);

    const root = document.documentElement;

    // Set theme mode class on HTML element
    root.classList.remove('light', 'dark');
    root.classList.add(patientTheme.mode);
    console.log('[theme.js] Applied mode class:', patientTheme.mode);

    // Apply color variables
    root.style.setProperty('--color-primary', patientTheme.primaryColor);
    root.style.setProperty('--color-accent', patientTheme.accentColor);
    root.style.setProperty('--primary-color', patientTheme.primaryColor);
    root.style.setProperty('--accent-color', patientTheme.accentColor);
    console.log('[theme.js] Set CSS variables - Primary:', patientTheme.primaryColor, 'Accent:', patientTheme.accentColor);

    // Convert hex to RGB for Tailwind classes
    const hexToRgb = (hex) => {
        const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : null;
    };

    const primaryRgb = hexToRgb(patientTheme.primaryColor);
    const accentRgb = hexToRgb(patientTheme.accentColor);

    if (primaryRgb) {
        root.style.setProperty('--color-primary-rgb', `${primaryRgb.r}, ${primaryRgb.g}, ${primaryRgb.b}`);
        console.log('[theme.js] Set primary RGB:', `${primaryRgb.r}, ${primaryRgb.g}, ${primaryRgb.b}`);
    }

    if (accentRgb) {
        root.style.setProperty('--color-accent-rgb', `${accentRgb.r}, ${accentRgb.g}, ${accentRgb.b}`);
        console.log('[theme.js] Set accent RGB:', `${accentRgb.r}, ${accentRgb.g}, ${accentRgb.b}`);
    }

    console.log('[theme.js] Patient theme applied successfully!');
};

/**
 * Initialize theme on page load
 */
(function() {
    // Apply saved theme from localStorage if exists
    const savedTheme = localStorage.getItem('medichat_theme_config');
    if (savedTheme) {
        try {
            const theme = JSON.parse(savedTheme);
            // Resolve 'system' mode to actual light/dark
            if (theme.mode === 'system') {
                theme.mode = window.detectSystemTheme() ? 'dark' : 'light';
            }
            window.applyTheme(theme);
        } catch (e) {
            console.error('Failed to apply saved theme:', e);
        }
    }
})();
