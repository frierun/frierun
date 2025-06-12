import InstallForm from "../components/InstallForm.tsx";
import {usePostPackagesIdPlan} from "@/api/endpoints/packages.ts";
import {useEffect} from "react";

type Props = {
    name: string;
}

export default function PackageInstall({name}: Props) {
    const {data, isPending, isError, isIdle, mutate} = usePostPackagesIdPlan();

    useEffect(() => {
        mutate({
            id: name,
            data: {
                type: 'Package',
                name,
                tags: [],
                contracts: []
            }
        })
    }, [mutate, name]);
    
    if (isPending || isIdle) return <p>Loading...</p>
    if (isError) return <p>Error!</p>

    if (data.status === 404) {
        return <p>Error: package {name} not found</p>;
    }

    if (data.status === 409) {
        return <p>Error: {data.data.message} {data.data.solution}</p>;
    }

    return (
        <InstallForm
            contracts={data.data.contracts}
            name={name}
        />
    );
} 