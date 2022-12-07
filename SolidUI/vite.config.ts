import { defineConfig } from 'vite'
import basicSsl from '@vitejs/plugin-basic-ssl'
import solidPlugin from 'vite-plugin-solid'
import UnoCSS from 'unocss/vite'
import { presetMini } from '@unocss/preset-mini'

export default defineConfig({
  plugins: [
    basicSsl(),
    solidPlugin(),
    UnoCSS({
      presets: [
        presetMini()
      ],
    }),
  ],
  server: {
    https: true,
    // port: 5173,
    proxy: {
      "/api": {
        target: "http://localhost:5077",
        xfwd: true
      },
      "/swagger": {
        target: "http://localhost:5077",
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
    // drop: ["console", "debugger"],
  },
});
