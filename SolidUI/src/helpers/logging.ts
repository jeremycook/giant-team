function warn(message: string, data?: { [key: string]: any } | any[]) {
    console.warn(message, data);
    // TODO: Log
}

function error(message: string, data?: { [key: string]: any } | any[]) {
    if (data) {
        if (data instanceof Array) {
            const pattern = /{[^{]+}/g;
            let i = 0;
            let namedData = {};
            const replacedMessage = message.replaceAll(pattern, (m) => {
                namedData = { ...namedData, [m]: data[i] };
                return data[i++];
            });
            console.error(replacedMessage, namedData, message);
        }
        else {
            const pattern = new RegExp('(' + Object.keys(data).map(key => '{' + key + '}').join('|') + ')', 'g');
            const replacedMessage = message.replaceAll(pattern, (m) => data[m]);
            console.error(replacedMessage, data, message);
        }
    }
    else {
        console.error(message);
    }
    // TODO: Upload to log
}

export const log = {
    error,
    warn,
}