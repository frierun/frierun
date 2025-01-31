import {Link, useParams} from "react-router-dom";
import InstallForm from "../components/InstallForm.tsx";
import Layout from "@/pages/Layout";
import {useGetPackagesIdPlan} from "@/api/endpoints/packages.ts";

export default function Package() {
    const {name} = useParams<{ name: string }>();

    const {data, isPending, isError} = useGetPackagesIdPlan(name ?? "");

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    return (
        <Layout>
            <Link to={`/`}>← Back</Link>
            <InstallForm contracts={data.data} name={name ?? ""}/>
        </Layout>
    );
} 