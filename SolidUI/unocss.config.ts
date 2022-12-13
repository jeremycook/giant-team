import presetAttributify from '@unocss/preset-attributify'
import type { VitePluginConfig } from '@unocss/vite'
import presetIcons from '@unocss/preset-icons'
import presetWind from '@unocss/preset-wind'

// Docs: https://github.com/unocss/unocss#readme

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
    preflights: [
      {
        getCSS: ({ theme }: any) => `
          * {
            border-color: ${theme.colors.gray?.[300] ?? 'red'};
          }
        `,
      }
    ],
    rules: [
      ['overflow-x-hidden', { 'overflow-x': 'hidden' }],
      ['position-right', { 'right': '0' }],

      // https://windicss.org/utilities/general/typography.html#text-overflow
      ['overflow-ellipsis', { 'text-overflow': 'ellipsis' }],
    ],
    shortcuts: {
      // 'border-main': 'border-gray-400',
      // 'bg-main': 'bg-gray-405',
      'button': 'p-button bg-sky border border-sky-600 color-white shadow rounded-1',
      'card': 'pxy bg-white border rounded-b shadow mt--1',
      'form-grid': 'grid grid-cols-1 md:grid-cols-[1fr_2fr] gap-2',
      'stack': 'flex flex-col bg-white border shadow rounded-1',
      'stack-item': 'px-2 py-1 max-w-sm overflow-x-hidden overflow-ellipsis whitespace-nowrap block',
      'p-button': 'px py-2',
      'text-ok': 'text-green-500',
      'text-error': 'text-red-500',
      'text-info': 'text-cyan-500',
    },
  }
}

export default createConfig()