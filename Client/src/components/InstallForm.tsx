import {useContext} from "react";
import StateContext from "../providers/StateContext.tsx";
import {type GetPackagesIdParameters200Item} from "../api/schemas";
import {getGetApplicationsQueryKey} from "../api/endpoints/applications.ts";
import {usePostPackagesId} from "../api/endpoints/packages.ts";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";
import Button from "./Button.tsx";

type Props = {
    contracts: GetPackagesIdParameters200Item[];
    name: string;
}

export default function InstallForm({contracts, name}: Props) {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostPackagesId();
    const queryClient = useQueryClient()
    const navigate = useNavigate();
    
    const install = () => {
        mutateAsync({id: name})
            .then(waitForReady)
            .then(() => queryClient.invalidateQueries({queryKey: getGetApplicationsQueryKey()}))
            .then(() => navigate('/'));
    };

    return (
        <>
            <Button onClick={install} disabled={isPending}>
                Install
            </Button>
            <div>
                {contracts.map(contract => (
                    <div key={`${contract.type} ${contract.name}`}>
                        <p>{contract.type}</p>
                        <pre>{JSON.stringify(contract, null, 2)}</pre>
                    </div>
                ))}
            </div>
        </>
    );
}