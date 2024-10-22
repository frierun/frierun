import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5001,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://192.168.178.30:5000/',
        changeOrigin: true,
        secure: true
      }
    }
  }
})