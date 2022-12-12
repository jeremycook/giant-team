import { defineConfig } from 'vite'
import basicSsl from '@vitejs/plugin-basic-ssl'
import solidPlugin from 'vite-plugin-solid'
import Unocss from 'unocss/vite'

export default defineConfig({
  plugins: [
    basicSsl(),
    solidPlugin(),
    Unocss('unocss.config.ts'),
  ],
  server: {
    https: true,
    // port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5077',
        xfwd: true
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
