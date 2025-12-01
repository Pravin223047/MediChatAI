/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html,cshtml}",
    "./**/*.cs",
    "./Pages/**/*.{razor,html}",
    "./Shared/**/*.{razor,html}",
    "./Layout/**/*.{razor,html}",
    "./Components/**/*.{razor,html}",
    "./wwwroot/index.html"
  ],
  darkMode: 'class', // Enable dark mode via class strategy
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: 'var(--primary-color, var(--color-primary, #6366f1))',
          50: 'var(--primary-50, #eef2ff)',
          100: 'var(--primary-100, #e0e7ff)',
          200: 'var(--primary-200, #c7d2fe)',
          300: 'var(--primary-300, #a5b4fc)',
          400: 'var(--primary-400, #818cf8)',
          500: 'var(--primary-color, var(--color-primary, #6366f1))',
          600: 'var(--primary-600, #4f46e5)',
          700: 'var(--primary-700, #4338ca)',
          800: 'var(--primary-800, #3730a3)',
          900: 'var(--primary-900, #312e81)',
        },
        accent: {
          DEFAULT: 'var(--accent-color, var(--color-accent, #8b5cf6))',
          50: '#faf5ff',
          100: '#f3e8ff',
          200: '#e9d5ff',
          300: '#d8b4fe',
          400: '#c084fc',
          500: 'var(--accent-color, var(--color-accent, #8b5cf6))',
          600: '#9333ea',
          700: '#7e22ce',
          800: '#6b21a8',
          900: '#581c87',
        },
      },
      backgroundColor: {
        'app': 'var(--color-background, #f8fafc)',
        'app-dark': 'var(--color-background-dark, #111827)',
      },
      textColor: {
        'app': 'var(--color-text, #1f2937)',
        'app-dark': 'var(--color-text-dark, #f9fafb)',
      },
      borderColor: {
        'theme': 'var(--color-border, #e5e7eb)',
        'theme-dark': 'var(--color-border-dark, #374151)',
      },
      borderRadius: {
        'theme': 'var(--border-radius, 0.5rem)',
      },
      fontSize: {
        'base': 'var(--base-font-size, 16px)',
      },
      spacing: {
        'unit': 'var(--spacing-unit, 1rem)',
      },
    },
  },
  plugins: [],
}