import { BaseNode, h } from '../../helpers/h';
import { State, Pipe } from '../../helpers/Pipe';
import Icon, { IconType } from './Icon';

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
    private _state = new State<ToastMessage[]>([]);
    private _messagePipe = this._state.asArray();

    constructor() {
    }

    get messagesPipe() {
        return this._messagePipe;
    }

    push(message: ToastMessage) {
        this._state.value = [message, ...this._state.value];
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
    return h('div',
        toast.messagesPipe.filter(x => x.readPipe.project(read => !read)).group(x => x.readPipe).map(([read, items]) =>
            h('.toast-group',
                h('.toast-group-header', read ? 'Read' : 'Unread'),
                items.map(message =>
                    h('.toast.toast-type-' + message.type,
                        h('.toast-header',
                            h('button', x => x.on('click', _ => message.read = !message.read),
                                Icon(message.readPipe.project(read => read ? 'mail-read-16-regular' : 'mail-unread-16-regular') as Pipe<IconType>),
                                h('.sr-only', message.readPipe.project(read => read ? 'Mark message as unread' : 'Mark message as read') as BaseNode)
                            ),
                            message.type,
                        ),
                        h('.toast-content', message.content),
                    )
                )
            ) as BaseNode
        ),
    )
}
