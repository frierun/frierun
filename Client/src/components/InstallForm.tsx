import {useCallback, useContext, useState} from "react";
import StateContext from "@/providers/StateContext.tsx";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";
import Button from "@/components/Button.tsx";
import HttpEndpointForm from "@/components/contracts/HttpEndpointForm.tsx";
import PortEndpointForm from "@/components/contracts/PortEndpointForm.tsx";
import Debug from "@/components/Debug";
import ParameterForm from "@/components/contracts/ParameterForm.tsx";
import {GetPackagesIdPlan200Item, Package,} from "@/api/schemas";
import VolumeForm from "@/components/contracts/VolumeForm.tsx";
import SelectorForm from "@/components/contracts/SelectorForm.tsx";
import {usePostPackagesIdInstall} from "@/api/endpoints/packages.ts";
import {getGetApplicationsQueryKey} from "@/api/endpoints/applications.ts";

type Props = {
    contracts: GetPackagesIdPlan200Item[];
    name: string;
}

type Overrides = {
    [key: string]: Package['contracts']
}

export default function InstallForm({contracts, name}: Props) {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostPackagesIdInstall();
    const [error, setError] = useState<string | null>(null);
    const queryClient = useQueryClient()
    const navigate = useNavigate();

    const pkg = contracts.find(contract => contract.type === 'Package');
    const [prefix, setPrefix] = useState(pkg?.prefix ?? '');
    const [overrides, setOverrides] = useState<Overrides>({});

    const packageContract = contracts.find(contract => contract.type === 'Package');

    const install = async () => {
        setError(null);
        const result = await mutateAsync({
            id: name, data: {
                type: 'Package',
                name,
                prefix,
                tags: [],
                contracts: Object.entries(overrides).flatMap(([, value]) => value),
                
            }
        });

        if (result.status === 404) {
            setError("Package not found");
            return;
        }

        if (result.status === 409) {
            setError(`Couldn't install contract ${result.data.type}. Install the missing dependencies first.`);
            return;
        }

        await waitForReady();
        await queryClient.invalidateQueries({queryKey: getGetApplicationsQueryKey()});
        navigate('/');
    };

    const updateContracts = useCallback((contracts: Package['contracts']) => {
        const contract = contracts[0];
        const key = `${contract.type}:${contract.name}`;
        setOverrides(overrides => ({
                ...overrides,
                [key]: contracts
            })
        )
    }, []);

    const updateContract = useCallback((contract: Package['contracts'][0]) => {
        updateContracts([contract]);
    }, [updateContracts]);

    return (
        <>
            <div className={"text-red-error font-bold my-2"}>
                {error}
            </div>
            <div className={"lg:w-1/2 my-6 border-primary border-b-2 px-1"}>
                <div className={"flex justify-between items-center px-1"}>
                    <div className={"flex gap-2 mb-2"}>
                        <div className={"h-12 w-12 rounded flex-shrink-0"}>
                            <img src={`/packages/${name}.png`} className={"rounded"} alt={name}/>
                        </div>
                        <h1>
                            {name}
                        </h1>
                    </div>
                    <div>
                        <Button onClick={install} disabled={isPending} type={"primary"}>
                            Install
                        </Button>
                    </div>
                </div>
                <div className={"my-3"}>
                    {packageContract?.fullDescription ?? ''}
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
                        .filter(contract => contract.type === 'Selector')
                        .map(contract => (
                            <div key={`${contract.type}:${contract.name}`} className={"card"}>
                                <SelectorForm
                                    contract={contract}
                                    updateContract={updateContract}
                                />
                            </div>
                        ))
                    }
                    {contracts
                        .filter(contract => contract.type === 'HttpEndpoint')
                        .map(contract => (
                            <div key={`${contract.type}:${contract.name}`} className={"card"}>
                                <HttpEndpointForm
                                    contract={contract}
                                    contracts={contracts}
                                    updateContracts={updateContracts}
                                />
                            </div>
                        ))
                    }
                    {contracts
                        .filter(contract => contract.type === 'PortEndpoint')
                        .filter(contract => contracts.find(httpContract =>
                            httpContract.type === 'HttpEndpoint'
                            && httpContract.port === contract.port
                            && httpContract.container === contract.container
                        ) === undefined)
                        .map(contract => (
                            <div key={`${contract.type}:${contract.name}`} className={"card"}>
                                <PortEndpointForm
                                    contract={contract}
                                    updateContract={updateContract}
                                />
                            </div>
                        ))
                    }
                    {contracts
                        .filter(contract => contract.type === 'Volume')
                        .map(contract => (
                            <div key={`${contract.type}:${contract.name}`} className={"card"}>
                                <VolumeForm
                                    contract={contract}
                                    updateContract={updateContract}
                                />
                            </div>
                        ))
                    }

                </div>
                <div className={"mt-4 mb-10"}>
                    <Button onClick={install} disabled={isPending} type={"primary"}>
                        Install
                    </Button>
                </div>
                <Debug>
                    {contracts.map(contract => (
                        <div key={`${contract.type}:${contract.name}`}>
                            <p>{contract.type}</p>
                            <pre>{JSON.stringify(contract, null, 2)}</pre>
                        </div>
                    ))}
                </Debug>
            </div>
        </>
    );
}