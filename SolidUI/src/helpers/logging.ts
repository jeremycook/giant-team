function warn(message: string, data?: Record<string, any>) {
    console.warn(message, data);
    // TODO: Log
}

function error(message: string, data?: Record<string, any>) {
    console.error(message, data);
    // TODO: Log
}

export const log = {
    error,
    warn,
}