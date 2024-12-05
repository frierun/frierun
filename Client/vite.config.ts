import {defineConfig, loadEnv, ConfigEnv} from 'vite'
import react from '@vitejs/plugin-react-swc'
import * as path from "node:path";

// https://vitejs.dev/config/
export default ({mode}: ConfigEnv) => {
    process.env = {...process.env, ...loadEnv(mode, process.cwd())};

    // import.meta.env.VITE_NAME available here with: process.env.VITE_NAME
    // import.meta.env.VITE_PORT available here with: process.env.VITE_PORT

    return defineConfig({
        plugins: [react()],
        resolve: {
            alias: {
                "@": path.resolve(__dirname, "./src"),
            }
        },
        server: {
            port: 5001,
            strictPort: true,
            proxy: {
                '/api': {
                    target: process.env.VITE_API_HOST,
                    changeOrigin: true,
                    secure: true
                }
            }
        }
    })
} 

