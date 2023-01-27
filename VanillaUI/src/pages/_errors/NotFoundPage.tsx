import CardLayout from "../_ui/CardLayout"

export default function NotFoundPage(props: { href: string }) {
    return <CardLayout>
        <h1>Page Not Found</h1>
        <p>
            The page or resource you were looking for was not found at <strong>{props.href}</strong>.
        </p>
    </CardLayout>
}