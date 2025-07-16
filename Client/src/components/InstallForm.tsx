import {useCallback, useState} from "react";
import Button from "@/components/Button.tsx";
import PortEndpointForm from "@/components/contracts/PortEndpointForm.tsx";
import Debug from "@/components/Debug";
import ParameterForm from "@/components/contracts/ParameterForm.tsx";
import {Package} from "@/api/schemas";
import ContractForm, {Contract} from "@/components/contracts/ContractForm.tsx";
import useInstall from "@/hooks/useInstall.tsx";

type Props = {
    packageContract: Package;
    contracts: Contract[];
    alternatives: Contract[];
    name: string;
    setError: (error: string | null) => void;
    refetch: (overrides: Contract[]) => void;
}

export default function InstallForm({packageContract, contracts, alternatives, name, setError, refetch}: Props) {
    const [prefix, setPrefix] = useState(packageContract.prefix ?? '');
    const [overrides, setOverrides] = useState<Contract[]>([]);

    const {install, isPending: isInstallPending} = useInstall({
        packageContract,
        overrides,
        prefix,
        setError
    })

    const updateContract = useCallback((contract: Contract, isRefetch?: boolean) => {
        setOverrides(overrides => {
            const newOverrides = [
                ...overrides.filter(c => c.type !== contract.type || c.name !== contract.name),
            ]

            if (contracts.find(c => c.type === contract.type && c.name === contract.name) !== contract) {
                newOverrides.push(contract);
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
                    {contracts.map(contract => (
                        <ContractForm
                            key={`${contract.type}:${contract.name}`}
                            contract={contract}
                            alternatives={alternatives}
                            updateContract={updateContract}
                            allContracts={contracts}
                        />
                    ))}
                    {contracts
                        .filter(contract => contract.type === 'Parameter')
                        .map(contract => (
                            <div key={`${contract.type}:${contract.name}`} className={"card"}>
                                <ParameterForm
                                    contract={contract}
                                    updateContract={updateContract}
                                />
                            </div>
                        ))
                    }
                    {contracts
                        .filter(contract => contract.type === 'PortEndpoint')
                        .map(contract => (
                            <div key={`${contract.type}:${contract.name}`} className={"card"}>
                                <PortEndpointForm
                                    contract={contract}
                                    updateContract={updateContract}
                                />
                            </div>
                        ))
                    }
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