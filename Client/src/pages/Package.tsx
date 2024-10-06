import {Link, useParams} from "react-router-dom";
import InstallForm from "../components/InstallForm.tsx";
import {useGetPackagesIdParameters} from "../api/endpoints/packages.ts";

export default function Package() {
    const {name} = useParams<{name: string}>();
    
    const {data, isPending, isError} = useGetPackagesIdParameters(name ?? "");

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>
    
    return (
        <>
            <Link to={`/`}>Back</Link>
            <h2 className="text-3xl font-bold underline">
                Install {name}
            </h2>
            <InstallForm defaultName={data.data.name} defaultPort={data.data.port} pkg={data.data.package} />
        </>
    );
} 