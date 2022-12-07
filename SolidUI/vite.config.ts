import UnocssPlugin from '@unocss/vite';
import basicSsl from '@vitejs/plugin-basic-ssl';
import { defineConfig } from 'vite';
import solidPlugin from 'vite-plugin-solid';

export default defineConfig({
  plugins: [
    basicSsl(),
    solidPlugin(),
    UnocssPlugin({
      // your config or in uno.config.ts
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
  },
  esbuild: {
    // TODO: Drop console and debugger code when building: https://esbuild.github.io/api/#drop
    // drop: ["console", "debugger"],
  },
});
