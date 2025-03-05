import InstallForm from "../components/InstallForm.tsx";
import {useGetPackagesIdPlan} from "@/api/endpoints/packages.ts";
import type {GetPackagesIdPlan200Item, GetPackagesIdPlan409} from "@/api/schemas";

type Props = {
    name: string;
}

export default function PackageInstall({name}: Props) {
    const {data, isPending, isError} = useGetPackagesIdPlan(name);

    if (isPending) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    if (data.status === 404) {
        return <p>Error: package {name} not found</p>;
    }
    
    if (data.status === 409) {
        const contract = data.data as GetPackagesIdPlan409;
        return <p>Error: Couldn't install contract {contract.Type}. Install the missing dependencies first.</p>;
    }
    
    if (data.status !== 200) {
        return <p>Error: unknown status code {data.status}</p>
    }

    return (
        <InstallForm
            contracts={data.data as GetPackagesIdPlan200Item[]}
            name={name ?? ""}
        />
    );
} 