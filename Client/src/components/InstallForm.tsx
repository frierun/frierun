import {useContext, useState} from "react";
import StateContext from "@/providers/StateContext.tsx";
import {getGetApplicationsQueryKey} from "@/api/endpoints/applications.ts";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";
import Button from "@/components/Button.tsx";
import {usePostPackagesIdInstall} from "@/api/endpoints/packages.ts";
import HttpEndpointForm from "@/components/contracts/HttpEndpointForm.tsx";
import PortEndpointForm from "@/components/contracts/PortEndpointForm.tsx";
import Debug from "@/components/Debug";
import ParameterForm from "@/components/contracts/ParameterForm.tsx";
import {GetPackagesIdPlan200Item, Package} from "@/api/schemas";
import VolumeForm from "@/components/contracts/VolumeForm.tsx";

type Props = {
    contracts: GetPackagesIdPlan200Item[];
    name: string;
    fullDescription: string;
}

type Overrides = {
    [key: string]: Package['contracts']
}

export default function InstallForm({contracts, name, fullDescription}: Props) {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostPackagesIdInstall();
    const queryClient = useQueryClient()
    const navigate = useNavigate();

    const pkg = contracts.find(contract => contract.Type === 'Package');
    const [prefix, setPrefix] = useState(pkg?.prefix ?? '');
    const [overrides, setOverrides] = useState<Overrides>({});

    const install = () => {
        mutateAsync({
            id: name, data: {
                Type: 'Package',
                name,
                prefix,
                tags: [],
                contracts: Object.entries(overrides).flatMap(([_, value]) => value),
            }
        })
            .then(waitForReady)
            .then(() => queryClient.invalidateQueries({queryKey: getGetApplicationsQueryKey()}))
            .then(() => navigate('/'));
    };

    const updateContracts = (contracts: Package['contracts']) => {
        const contract = contracts[0];
        const key = `${contract.Type} ${contract.name}`;
        setOverrides({
                ...overrides,
                [key]: contracts
            }
        )
    }

    const updateContract = (contract: Package['contracts'][0]) => updateContracts([contract]);

    return (
        <>
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
                    {fullDescription ?? ''}
                </div>
            </div>
            <div className={"lg:w-1/2"}>
                <h2>Settings</h2>
                <div className={"grid xl:grid-cols-1 gap-3"}>
                    <div className={"card"}>
                        <label className={"inline-block w-48"}>
                            Application name:
                        </label>
                        <input value={prefix} onChange={e => setPrefix(e.target.value)}/>
                    </div>
                    {contracts
                        .filter(contract => contract.Type === 'Parameter')
                        .map(contract => (
                            <div key={`${contract.Type} ${contract.name}`} className={"card"}>
                                <ParameterForm
                                    contract={contract}
                                    updateContract={updateContract}
                                />
                            </div>
                        ))
                    }
                    {contracts
                        .filter(contract => contract.Type === 'HttpEndpoint')
                        .map(contract => (
                            <div key={`${contract.Type} ${contract.name}`} className={"card"}>
                                <HttpEndpointForm
                                    contract={contract}
                                    contracts={contracts}
                                    updateContracts={updateContracts}
                                />
                            </div>
                        ))
                    }
                    {contracts
                        .filter(contract => contract.Type === 'PortEndpoint')
                        .filter(contract => contracts.find(httpContract =>
                            httpContract.Type === 'HttpEndpoint'
                            && httpContract.port === contract.port
                            && httpContract.containerName === contract.containerName
                        ) === undefined)
                        .map(contract => (
                            <div key={`${contract.Type} ${contract.name}`} className={"card"}>
                                <PortEndpointForm
                                    contract={contract}
                                    updateContract={updateContract}
                                />
                            </div>
                        ))
                    }
                    {contracts
                        .filter(contract => contract.Type === 'Volume')
                        .map(contract => (
                            <div key={`${contract.Type} ${contract.name}`} className={"card"}>
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
                        <div key={`${contract.Type} ${contract.name}`}>
                            <p>{contract.Type}</p>
                            <pre>{JSON.stringify(contract, null, 2)}</pre>
                        </div>
                    ))}
                </Debug>
            </div>
        </>
    );
}