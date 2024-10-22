/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    colors: {
      transparent: 'transparent',
      primary: {
        DEFAULT: '#080A37',
        softer: '#181A51',
      },
      secondary: {
        DEFAULT: '#5C96D7',
        lighter: '#7CAFE7',
        darker: '#324F86',
      },
      gray: {
        DEFAULT: '#F7F7F7',
      },
      black: {
        DEFAULT: '#000',
      }
    },
    extend: {},
  },
  plugins: [],
}

