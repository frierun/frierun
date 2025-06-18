import InstallForm from "../components/InstallForm.tsx";
import {usePostPackagesIdPlan} from "@/api/endpoints/packages.ts";
import {useCallback, useEffect, useState} from "react";
import {Package} from "@/api/schemas";
import {Contract} from "@/components/contracts/ContractForm.tsx";

type Props = {
    name: string;
}

type Plan = {
    packageContract: Package;
    contracts: Contract[];
    alternatives: Contract[];
}

export default function PackageInstall({name}: Props) {
    const [error, setError] = useState<string | null>(null);
    const [plan, setPlan] = useState<Plan | null>(null);
    const {isPending, isIdle, mutateAsync} = usePostPackagesIdPlan();

    const refetch = useCallback(async (overrides: Contract[]) => {
        setError(null);
        const result = await mutateAsync({
            id: name,
            data: {
                type: 'Package',
                name,
                tags: [],
                contracts: overrides
            }
        })

        if (result.status === 404) {
            setError("Package not found");
            return;
        }

        if (result.status === 409) {
            setError(`${result.data.message} ${result.data.solution}`);
            return;
        }

        const packageContract = result.data.contracts.find(contract => contract.type === 'Package');
        if (!packageContract) {
            setError("Package has no contract");
            return;
        }

        setPlan({
            packageContract,
            contracts: result.data.contracts,
            alternatives: result.data.alternatives
        });
    }, [mutateAsync, name]);

    useEffect(() => {
        void refetch([]);
    }, [refetch]);

    if (plan == null && (isIdle || isPending)) {
        return <div className={"text-center my-6"}>Loading...</div>;
    }

    return (
        <>
            <div className={"text-red-error font-bold my-2"}>
                {error}
            </div>
            {plan && (
                <>
                    {isPending &&
                        <div
                            className="z-50 bg-gray opacity-50 absolute top-20 left-0 right-0 bottom-0 flex justify-center font-bold">
                            Please wait...
                        </div>
                    }                    
                    <InstallForm
                        packageContract={plan.packageContract}
                        contracts={plan.contracts}
                        alternatives={plan.alternatives}
                        name={name}
                        refetch={refetch}
                        setError={setError}
                    />
                </>
            )}
        </>
    );
} 