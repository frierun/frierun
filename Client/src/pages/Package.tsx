import useAxios from "axios-hooks";
import {Link, useParams} from "react-router-dom";
import PackageModel from "../models/Package";
import InstallForm from "../components/InstallForm.tsx";

export default function Package() {
    const {name} = useParams<{name: string}>();

    const [{data, loading, error}] = useAxios<PackageModel>(`/api/v1/packages/${name}`);
    
    if (loading) return <p>Loading...</p>
    if (error || !data) return <p>Error!</p>

    return (
        <>
            <Link to={`/`}>Back</Link>
            <h2 className="text-3xl font-bold underline">
                Install {name}
            </h2>
            <InstallForm pkg={data} />
        </>
    );
}