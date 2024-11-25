import {useContext, useState} from "react";
import StateContext from "../providers/StateContext.tsx";
import {getGetApplicationsQueryKey} from "../api/endpoints/applications.ts";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";
import Button from "./Button.tsx";
import {GetPackagesIdPlan200Item, Package} from "../api/schemas";
import {usePostPackagesIdInstall} from "../api/endpoints/packages.ts";
import HttpEndpointForm from "./HttpEndpointForm.tsx";
import PortEndpointForm from "./PortEndpointForm.tsx";

type Props = {
    contracts: GetPackagesIdPlan200Item[];
    name: string;
}

export default function InstallForm({contracts, name}: Props) {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostPackagesIdInstall();
    const queryClient = useQueryClient()
    const navigate = useNavigate();

    const pkg = contracts.find(contract => contract.Type === 'Application');
    const [prefix, setPrefix] = useState(pkg?.prefix ?? '');
    const [overrides, setOverrides] = useState<Package['contracts']>([]);

    const install = () => {
        mutateAsync({
            id: name, data: {
                Type: 'Application',
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
                .filter(override => override.Type !== contract.Type && override.name !== contract.name)
                .concat(contract)
        )
    }

    return (
        <>
            <div>
                <label>
                    Application name:
                    <input value={prefix} onChange={e => setPrefix(e.target.value)}/>
                </label>
            </div>
            {contracts.filter(contract => contract.Type === 'HttpEndpoint').map(contract => (
                <div key={`${contract.Type} ${contract.name}`}>
                    <HttpEndpointForm
                        contract={contract}
                        updateContract={updateContract}
                    />
                </div>
            ))}
            {contracts.filter(contract => contract.Type === 'PortEndpoint').map(contract => (
                <div key={`${contract.Type} ${contract.name}`}>
                    <PortEndpointForm
                        contract={contract}
                        updateContract={updateContract}
                    />
                </div>
            ))}
            <Button onClick={install} disabled={isPending}>
                Install
            </Button>
            <div>
                {contracts.map(contract => (
                    <div key={`${contract.Type} ${contract.name}`}>
                        <p>{contract.Type}</p>
                        <pre>{JSON.stringify(contract, null, 2)}</pre>
                    </div>
                ))}
            </div>
        </>
    );
}