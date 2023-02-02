import { BaseNode, h } from '../../helpers/h';
import { StateCollection, State, Pipe } from '../../helpers/Pipe';
import Icon from './Icon';

enum ToastType {
    Info = 'Info',
    Success = 'Success',
    Warning = 'Warning',
    Error = 'Error',
};

export class ToastMessage {
    private _type: ToastType;
    private _content: BaseNode;
    private _readPipe: State<boolean>;
    private _created: Date;

    constructor(state: {
        content: BaseNode;
        type?: ToastType;
        read?: boolean;
    }) {
        this._type = state.type ?? ToastType.Info;
        this._content = state.content;
        this._readPipe = new State(state.read ?? false);
        this._created = new Date();
    }

    get type() {
        return this._type;
    }

    get content() {
        return this._content;
    }

    get read() {
        return this._readPipe.value;
    }
    set read(value: boolean) {
        this._readPipe.value = value;
    }

    get readPipe(): Pipe<boolean> {
        return this._readPipe;
    }

    get created() {
        return this._created;
    }
}

export class Toast {
    private _messages = new StateCollection<ToastMessage>();

    constructor() {
    }

    get messagesPipe() {
        return this._messages;
    }

    push(message: ToastMessage) {
        this._messages.value = [message, ...this._messages.value];
    }

    info(content: BaseNode) {
        this.push(new ToastMessage({ content, type: ToastType.Info }));
    }

    success(content: BaseNode) {
        this.push(new ToastMessage({ content, type: ToastType.Success }));
    }

    warning(content: BaseNode) {
        this.push(new ToastMessage({ content, type: ToastType.Warning }));
    }

    error(content: BaseNode) {
        this.push(new ToastMessage({ content, type: ToastType.Error }));
    }
}

export const toast = new Toast();

export function ToastUI(toast: Toast) {
    return toast.messagesPipe.map(message =>
        h('.toast.toast-type-' + message.type,
            h('.toast-header',
                h('button', x => x.on('click', _ => message.read = !message.read),
                    Icon(message.readPipe.project(read => read ? 'mail-read-16-regular' : 'mail-unread-16-regular')),
                    h('.sr-only', message.readPipe.project(read => read ? 'Read' : 'Unread'))
                ),
                message.type,
            ),
            h('.toast-content', message.content),
        )
    )
}
