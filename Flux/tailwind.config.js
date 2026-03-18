/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html,cshtml}",
  ],
  theme: {
    extend: {
      colors: {
        primary: 'var(--color-primary, #3b82f6)',
        secondary: 'var(--color-secondary, #64748b)',
        background: 'var(--bg-main, #ffffff)',
        surface: 'var(--bg-surface, #f8fafc)',
        text: 'var(--text-main, #0f172a)'
      },
    },
  },
  plugins: [],
}