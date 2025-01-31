import {Link, useParams} from "react-router-dom";
import InstallForm from "../components/InstallForm.tsx";
import Layout from "@/pages/Layout";
import {useGetPackagesIdPlan} from "@/api/endpoints/packages.ts";
import Button from "@/components/Button";

export default function Package() {
    const {name} = useParams<{ name: string }>();

    const {data, isPending, isError} = useGetPackagesIdPlan(name ?? "");

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    return (
        <Layout>
            <Link to={`/`}>← Back</Link>
            <div className={"flex lg:w-1/2 flex justify-between items-center my-6 border-primary border-b-2 px-1"}>
                <div className={"flex gap-2 mb-2"}>
                    <div className={"h-12 w-12 rounded flex-shrink-0"}>
                        <img src={`/packages/${name}.png`} className={"rounded"} alt={name}/>
                    </div>
                    <h1>
                        {name}
                    </h1>
                </div>
                <div>
                    <Button disabled={isPending} type={"primary"} disabled>
                        Install now
                    </Button>
                </div>
            </div>
            <InstallForm contracts={data.data} name={name ?? ""}/>
        </Layout>
    );
} 