import MainLayout from "./MainLayout";

export default function (...children: []) {
    return (
        <MainLayout>
            <div class='card'>
                {children}
            </div>
        </MainLayout>
    );
};
