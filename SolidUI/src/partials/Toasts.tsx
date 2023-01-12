import { batch, createEffect, createSignal, For, JSX, Match, Show, Switch } from "solid-js"
import { createMutable } from "solid-js/store"
import { DismissIcon, InfoIcon } from "../helpers/icons";

type NotificationTypeEnum = 'info' | 'success' | 'warning' | 'error';

const NotificationType = {
    Info: 'info' as const,
    Success: 'success' as const,
    Warning: 'warning' as const,
    Error: 'error' as const,
};

type Notification = {
    type: NotificationTypeEnum;
    content: JSX.Element;
    read: boolean;
    autoDismissAfter?: Date;
};

const [show, setShow] = createSignal(false);
const [notifications, setNotifications] = createSignal<Notification[]>([]);
const unreadNotifications = () => notifications().filter(o => !o.read);
const reversedNotifications = () => [...notifications()].reverse();

export const toggleNotifications = () => {
    batch(() => {
        if (show()) {
            // Dismiss all
            unreadNotifications().forEach(n => n.read = true);
        }
        setShow(!show());
    })
};

createEffect(() => {
    if (unreadNotifications().length > 0) {
        setShow(true);
    }
})

const push = (type: NotificationTypeEnum, content: JSX.Element) => {
    const offset = Math.max(2000, Math.ceil(75 * (content?.toString().length ?? 0)));
    const autoDismissAfter = new Date(Date.now() + offset);

    const notice = createMutable({
        type,
        content,
        read: false,
        autoDismissAfter,
    });

    setTimeout(() => {
        if (notice.autoDismissAfter instanceof Date) {
            notice.read = true;
            if (unreadNotifications().length <= 0) {
                // Hide the whole tray if this was the last auto dismiss
                setShow(false);
            }
        }
    }, offset);

    setNotifications([...notifications(), notice]);

    if (notifications().length > 10) {
        setNotifications(notifications().slice(notifications().length - 10));
    }
};

export const toast = {
    show,
    push,
    info: (content: JSX.Element) => {
        push(NotificationType.Info, content);
    },
    success: (content: JSX.Element) => {
        push(NotificationType.Success, content);
    },
    warning: (content: JSX.Element) => {
        push(NotificationType.Warning, content);
    },
    error: (content: JSX.Element) => {
        push(NotificationType.Error, content);
    },
}

export function Toasts() {
    return <Show when={show()}>
        <div class='fixed position-top position-bottom position-right max-w-90% w-300px overflow-auto flex flex-col gap-2 p-2 bg-black bg-opacity-10 shadow-xl'
            onmouseenter={() => unreadNotifications().forEach(n => n.autoDismissAfter = undefined)}
            onclick={() => unreadNotifications().forEach(n => n.autoDismissAfter = undefined)}>
            <button type='button' onclick={() => toggleNotifications()}>
                Dismiss Notifications
            </button>
            <Show when={notifications().length <= 0}>
                <div class='rounded animate-fade-in animate-duration-200'>
                    <div class='grow p-2 overflow-auto bg-white bg-opacity-90'>
                        <InfoIcon /> No recent notifications.
                    </div>
                </div>
            </Show>
            <For each={reversedNotifications()}>{notice => <>
                <div class='rounded animate-fade-in animate-duration-200'
                    classList={{
                        'opacity-70': notice.read
                    }}>
                    <div class='flex p-2 text-white' classList={{
                        'bg-info': notice.type == NotificationType.Info,
                        'bg-ok': notice.type == NotificationType.Success,
                        'bg-warn': notice.type == NotificationType.Warning,
                        'bg-danger': notice.type == NotificationType.Error,
                    }}>
                        <button type='button' class='text-white' title='Mark as read'
                            onclick={() => notice.read = true}>
                            <DismissIcon />
                            <span class='sr-only'>Dismiss Notification</span>
                        </button>
                        <div class='grow mx-2'>
                            <Switch fallback={notice.type}>
                                <Match when={notice.type == NotificationType.Info}>
                                    Information
                                </Match>
                                <Match when={notice.type == NotificationType.Success}>
                                    Success
                                </Match>
                                <Match when={notice.type == NotificationType.Warning}>
                                    Warning
                                </Match>
                                <Match when={notice.type == NotificationType.Error}>
                                    Error
                                </Match>
                            </Switch>
                        </div>
                        <div>
                            {notice.read ? 'Read' : 'Unread'}
                        </div>
                    </div>
                    <div class='grow p-2 overflow-auto bg-white bg-opacity-90'>
                        {notice.content}
                    </div>
                </div>
            </>}</For>
        </div>
    </Show>
}