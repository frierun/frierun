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
        softer: '#89c1ff',
        darker: '#324F86',
      },
      gray: {
        DEFAULT: '#F7F7F7',
        disabled: '#ccc',
        darker: '#999',
      },
      black: {
        DEFAULT: '#000',
      }
    },
    screens: {
      'xs': '410px',
      'sm': '640px',
      'md': '768px',
      'lg': '1024px',
      'xl': '1280px',
      'xxl': '1820px',
    },
    extend: {
    },
  },
  plugins: [],
}

