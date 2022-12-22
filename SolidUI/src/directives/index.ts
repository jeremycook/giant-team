import { input } from './input';

const directives = {
    input,
};

Object.entries(directives).forEach(([key, value]) => {
    (globalThis as any)[key] = value;
});
