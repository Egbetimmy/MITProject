import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/users': 'http://localhost:5000',
      '/products': 'http://localhost:5000',
      '/orders': 'http://localhost:5000',
      '/monitoring': 'http://localhost:5000',
      '/prediction': 'http://localhost:5000',
      '/api': 'http://localhost:5000'
    }
  }
})
