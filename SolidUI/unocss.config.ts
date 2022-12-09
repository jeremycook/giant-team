import presetAttributify from '@unocss/preset-attributify'
import type { VitePluginConfig } from '@unocss/vite'
import presetIcons from '@unocss/preset-icons'
import presetWind from '@unocss/preset-wind'

export function createConfig(): VitePluginConfig {
  return {
    envMode: 'build',
    theme: {
      fontFamily: {
        sans: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol"',
        mono: '"SF Mono", "Monaco", "Inconsolata", "Fira Mono", "Droid Sans Mono", "Source Code Pro", monospace',
      },
    },
    presets: [
      presetAttributify(),
      presetIcons({
        extraProperties: {
          'display': 'inline-block',
          // 'height': '1.2em',
          // 'width': '1.2em',
          'vertical-align': 'text-bottom',
        },
      }),
      presetWind(),
    ],
    shortcuts: {
      // 'b-main': 'border-gray-400 border-opacity-30',
      // 'bg-main': 'bg-gray-405',
      'button': 'p-button bg-sky border border-sky-600 color-white shadow rounded-1',
      'card': 'pxy bg-white border border-gray-300 rounded shadow',
      'form-grid': 'grid grid-cols-1 md:grid-cols-[1fr_2fr] gap-2',
      'p-button': 'px py-2',
      'text-ok': 'text-green-500',
      'text-error': 'text-red-500',
      'text-info': 'text-cyan-500',
    },
  }
}

export default createConfig()