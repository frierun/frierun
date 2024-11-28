import Applications from "../components/Applications.tsx";
import Packages from "../components/Packages.tsx";
import Layout from "./Layout";

export default function Root() {
    return (
        <Layout>
            <Applications />
            <Packages />
        </Layout>
    );
}

