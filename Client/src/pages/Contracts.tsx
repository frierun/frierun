import Layout from "./Layout";
import {Link} from "react-router-dom";
import InstalledContracts from "@/components/InstalledContracts.tsx";

export default function Contracts() {
    return (
        <Layout>
            <Link to={`/`}>← Back</Link>
            <InstalledContracts />
        </Layout>
    );
}

