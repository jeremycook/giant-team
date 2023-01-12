import { For, JSX, Match, Show, Switch } from "solid-js"
import { createMutable } from "solid-js/store"
import { DismissIcon } from "../helpers/icons";

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
    dismissed: boolean;
    countDown: number | null;
};

const notifications = createMutable<Notification[]>([]);

const activeNotifications = () => notifications.filter(o => !o.dismissed).reverse();

const push = (type: NotificationTypeEnum, content: JSX.Element) => {
    const notice = createMutable({ type, content, dismissed: false, countDown: Math.ceil(75 * (content?.toString().length ?? 100)) });

    setInterval(() => notice.countDown -= 1000, 1000);
    setTimeout(() => {
        if (notice.countDown !== null)
            notice.dismissed = true;
    }, notice.countDown);

    notifications.push(notice);
};

export const toast = {
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
    return <Show when={activeNotifications().length > 0}>
        <div class='fixed position-top position-right max-w-100% w-300px max-h-screen overflow-auto flex flex-col gap-2 p-2 bg-black bg-opacity-10 rounded-b shadow-xl'>
            <For each={activeNotifications()}>{notice => <>
                <div class='rounded animate-fade-in animate-duration-200'>
                    <div class='flex p-2 rounded-t text-white' classList={{
                        'bg-info': notice.type == NotificationType.Info,
                        'bg-ok': notice.type == NotificationType.Success,
                        'bg-warn': notice.type == NotificationType.Warning,
                        'bg-danger': notice.type == NotificationType.Error,
                    }}>
                        <button type='button' class='text-white' onclick={() => notice.dismissed = true}>
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
                        <Show when={notice.countDown ?? 0 > 0}>
                            <div>
                                {Math.round(notice.countDown! / 1000)}s
                            </div>
                        </Show>
                    </div>
                    <div class='grow p-2 overflow-auto bg-white bg-opacity-90'>
                        {notice.content}
                    </div>
                </div>
            </>}</For>
        </div>
    </Show>
}