
export default function (props: {}, ...children: []) {
    return <main role='main' class='site-main' {...props}>
        {children}
    </main>
};
