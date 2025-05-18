import InstallForm from "../components/InstallForm.tsx";
import {useGetPackagesIdPlan} from "@/api/endpoints/packages.ts";

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
        return <p>Error: Couldn&apos;t install contract {data.data.type}. Install the missing dependencies first.</p>;
    }
    
    return (
        <InstallForm
            contracts={data.data}
            name={name}
        />
    );
} 