export default function (props: {}, ...children: []) {
    return <main role='main' class='site-card' {...props}>
        <div class='card'>
            {children}
        </div>
    </main>
};
