function warn(message: string, data?: Record<string, any>) {
    console.warn(message, data);
    // TODO: Log
}

function error(message: string, data?: Record<string, any>) {
    if (data) {
        const pattern = new RegExp('(' + Object.keys(data).map(o => '{' + data + '}').join('|') + ")");
        const replacedMessage = message.replaceAll(pattern, (m) => data[m]);
        console.error(replacedMessage, data);
    }
    else {
        console.error(message);
    }
    // TODO: Log
}

export const log = {
    error,
    warn,
}