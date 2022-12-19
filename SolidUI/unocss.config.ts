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
      // See: 
      //  https://github.com/unocss/unocss/blob/main/packages/preset-mini/src/_theme/colors.ts
      //  https://windicss.org/utilities/general/colors.html
      colors: {
        'primary': {
          DEFAULT: '#0ea5e9',
          50: '#f0f9ff',
          100: '#e0f2fe',
          200: '#bae6fd',
          300: '#7dd3fc',
          400: '#38bdf8',
          500: '#0ea5e9',
          600: '#0284c7',
          700: '#0369a1',
          800: '#075985',
          900: '#0c4a6e',
        },
        'disabled': {
          DEFAULT: '#6b7280',
          50: '#f9fafb',
          100: '#f3f4f6',
          200: '#e5e7eb',
          300: '#d1d5db',
          400: '#9ca3af',
          500: '#6b7280',
          600: '#4b5563',
          700: '#374151',
          800: '#1f2937',
          900: '#111827',
        },
      }
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
      ['position-left', { 'left': '0' }],
      ['position-top', { 'top': '0' }],
      ['position-bottom', { 'bottom': '0' }],

      // https://windicss.org/utilities/general/typography.html#text-overflow
      ['overflow-ellipsis', { 'text-overflow': 'ellipsis' }],
    ],
    shortcuts: [
      [/^paint-([a-z]*)$/, ([, c, n]) => `bg-${c} color-white border-${c}-600`],
      [/^paint-([a-z]+)-([0-9]{3})$/, ([, c, n]) => `bg-${c + (n ? ('-' + n) : '')} color-white border-${c}-${n ? (parseInt(n) + 100) : '600'}`],
      {
        'p-button': 'px py-2',

        'button': 'p-button border paint-primary rounded-1 shadow active:shadow-inset active:bg-primary-400',

        'card': 'pxy bg-white border rounded-b shadow',

        'form-grid': 'grid grid-cols-1 md:grid-cols-[1fr_2fr] gap-2',

        'oval': 'rounded rounded-500px',

        'stack': 'flex flex-col bg-white border shadow rounded-1',
        'stack-item': 'px-2 py-1 max-w-sm overflow-x-hidden overflow-ellipsis whitespace-nowrap block',

        'text-ok': 'text-green-500',
        'text-error': 'text-red-500',
        'text-info': 'text-cyan-500',
      }],
  }
}

export default createConfig()