import {useCallback, useState} from "react";
import Button from "@/components/Button.tsx";
import Debug from "@/components/Debug";
import {Package} from "@/api/schemas";
import ContractForm from "@/components/contracts/ContractForm.tsx";
import useInstall from "@/hooks/useInstall.tsx";
import {Alternative, Contract, ContractList} from "@/types.ts";

type Props = {
    packageContract: Package;
    contracts: ContractList;
    alternatives: Alternative[];
    name: string;
    setError: (error: string | null) => void;
    refetch: (overrides: ContractList) => void;
}

export default function InstallForm({packageContract, contracts, alternatives, name, setError, refetch}: Props) {
    const [prefix, setPrefix] = useState(packageContract.prefix ?? '');
    const [overrides, setOverrides] = useState<ContractList>({});

    const {install, isPending: isInstallPending} = useInstall({
        packageContract,
        overrides,
        prefix,
        setError
    })

    const updateContract = useCallback((contractId: string, contract: Contract|null, isRefetch?: boolean) => {
        setOverrides(overrides => {
            const newOverrides = Object.fromEntries(Object.entries(overrides).filter(entry => entry[0] != contractId));

            if (contracts[contractId] !== contract && contract !== null) {
                newOverrides[contractId] = contract;
            }

            if (isRefetch) {
                refetch(newOverrides);
            }
            return newOverrides;
        });
    }, [contracts, refetch]);

    return (
        <>
            <div className={"lg:w-1/2 my-6 border-primary border-b-2 px-1"}>
                <div className={"flex justify-between items-center px-1"}>
                    <div className={"flex gap-2 mb-2"}>
                        <div className={"h-12 w-12 rounded flex-shrink-0"}>
                            <img
                                className={"rounded"}
                                alt={name}
                                src={packageContract.iconUrl ?? `https://cdn.jsdelivr.net/gh/selfhst/icons/png/${name}.png`}
                            />
                        </div>
                        <h1>
                            {name}
                        </h1>
                    </div>
                    <div>
                        <Button onClick={install} disabled={isInstallPending} type={"primary"}>
                            Install
                        </Button>
                    </div>
                </div>
                <div className={"my-3"}>
                    {packageContract.fullDescription ?? ''}
                </div>
            </div>
            <div className={"lg:w-1/2"}>
                <h2>Settings</h2>
                <div className={"grid xl:grid-cols-1 gap-3"}>
                    <div className={"card"}>
                        <label className={"inline-block w-48"}>
                            Application name:
                        </label>
                        <input value={prefix} onChange={e => {
                            setPrefix(e.target.value);
                        }}/>
                    </div>
                    {Object.entries(contracts).map(entry => (
                        <ContractForm
                            key={entry[0]}
                            contractId={entry[0]}
                            alternatives={alternatives}
                            updateContract={updateContract}
                            allContracts={contracts}
                        />
                    ))}
                </div>
                <div className={"mt-4 mb-10"}>
                    <Button onClick={install} disabled={isInstallPending} type={"primary"}>
                        Install
                    </Button>
                </div>
                <Debug contracts={contracts}/>
            </div>
        </>
    );
}