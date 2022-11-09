import { defineConfig } from 'vite';
import solidPlugin from 'vite-plugin-solid';
import UnocssPlugin from '@unocss/vite';

export default defineConfig({
  plugins: [
    solidPlugin(),
    UnocssPlugin({
      // your config or in uno.config.ts
    }),
  ],
  server: {
    port: 3000,
    proxy: {
      "/api": "http://localhost:5077",
      "/swagger": "http://localhost:5077",
    }
  },
  build: {
    target: 'esnext',
  },
  esbuild: {
    // Drop console and debugger code when building: https://esbuild.github.io/api/#drop
    drop: ["console", "debugger"],
  },
});
