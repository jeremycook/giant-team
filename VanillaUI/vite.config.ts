import { defineConfig } from 'vite'
import basicSsl from '@vitejs/plugin-basic-ssl'

export default defineConfig({
  plugins: [
    // TODO: create/use a plugin that strips unused CSS variables like those in colors.css
    basicSsl(),
  ],
  server: {
    https: true,
    // port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5077',
        xfwd: true,
        configure: (proxy) => proxy.on('error', (err, req, res) => console.error(err)),
      },
      '/swagger': {
        target: 'http://localhost:5077',
        xfwd: true
      },
    }
  },
  build: {
    target: 'esnext',
    // polyfillDynamicImport: false,
  },
  esbuild: {
    // TODO: Drop console and debugger code when building: https://esbuild.github.io/api/#drop
    // drop: ['console', 'debugger'],
  },
});
