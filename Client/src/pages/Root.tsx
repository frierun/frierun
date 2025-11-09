import Applications from "../components/Applications.tsx";
import Packages from "../components/Packages.tsx";
import Layout from "./Layout";
import {Link} from "react-router-dom";

export default function Root() {
    return (
        <Layout>
            <Link to="/contracts">Installed Contracts</Link>
            <Applications />
            <Packages />
        </Layout>
    );
}

