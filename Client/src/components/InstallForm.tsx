import {useContext, useState} from "react";
import StateContext from "../providers/StateContext.tsx";
import {getGetApplicationsQueryKey} from "../api/endpoints/applications.ts";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";
import Button from "./Button.tsx";
import {GetPackagesIdPlan200Item, Package} from "../api/schemas";
import {usePostPackagesIdInstall} from "../api/endpoints/packages.ts";
import HttpEndpointForm from "./contracts/HttpEndpointForm.tsx";
import PortEndpointForm from "./contracts/PortEndpointForm.tsx";
import Debug from "./Debug";
import ParameterForm from "@/components/contracts/ParameterForm.tsx";

type Props = {
    contracts: GetPackagesIdPlan200Item[];
    name: string;
}

export default function InstallForm({contracts, name}: Props) {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostPackagesIdInstall();
    const queryClient = useQueryClient()
    const navigate = useNavigate();

    const pkg = contracts.find(contract => contract.Type === 'Package');
    const [prefix, setPrefix] = useState(pkg?.prefix ?? '');
    const [overrides, setOverrides] = useState<Package['contracts']>([]);

    const install = () => {
        mutateAsync({
            id: name, data: {
                Type: 'Package',
                name,
                prefix,
                contracts: overrides,
            }
        })
            .then(waitForReady)
            .then(() => queryClient.invalidateQueries({queryKey: getGetApplicationsQueryKey()}))
            .then(() => navigate('/'));
    };

    const updateContract = (contract: Package['contracts'][0]) => {
        setOverrides(
            overrides => overrides
                .filter(override => override.Type !== contract.Type || override.name !== contract.name)
                .concat(contract)
        )
    }

    return (
        <div>
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
                    ))}
                {contracts
                    .filter(contract => contract.Type === 'HttpEndpoint')
                    .map(contract => (
                        <div key={`${contract.Type} ${contract.name}`} className={"card"}>
                            <HttpEndpointForm
                                contract={contract}
                                updateContract={updateContract}
                            />
                        </div>
                    ))}
                {contracts
                    .filter(contract => contract.Type === 'PortEndpoint')
                    .map(contract => (
                        <div key={`${contract.Type} ${contract.name}`} className={"card"}>
                            <PortEndpointForm
                                contract={contract}
                                updateContract={updateContract}
                            />
                        </div>
                    ))}
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
    );
}