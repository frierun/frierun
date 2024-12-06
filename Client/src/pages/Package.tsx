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
            <div className={"flex gap-2 mb-2 mt-6"}>
                <div className={"h-12 w-12 rounded flex-shrink-0"}>
                    <img src={`/packages/${name}.png`} className={"rounded"} alt={name} />
                </div>
                <h1>
                    Install {name}
                </h1>
            </div>
            <InstallForm contracts={data.data} name={name ?? ""}/>
        </Layout>
);
} 