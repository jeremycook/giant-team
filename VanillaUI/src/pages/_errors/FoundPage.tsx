import CardLayout from "../_ui/CardLayout"

export default function FoundPage(props: { href: string }) {
    return <CardLayout>
        <h1>Page Found</h1>
        <p>
            We believe that the page you are looking for can be found <a href={props.href}>here</a>.
        </p>
    </CardLayout>
}