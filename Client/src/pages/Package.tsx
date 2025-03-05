import {Link, useParams} from "react-router-dom";
import Layout from "@/pages/Layout";
import PackageInstall from "@/components/PackageInstall.tsx";

export default function Package() {
    const {name} = useParams<{ name: string }>();
    
    return (
        <Layout>
            <Link to={`/`}>← Back</Link>
            <PackageInstall name={name ?? ""} />
        </Layout>
    );
} 