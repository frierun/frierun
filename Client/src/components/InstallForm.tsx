import {useContext} from "react";
import StateContext from "../providers/StateContext.tsx";
import {type Contract} from "../api/schemas";
import {getGetApplicationsQueryKey} from "../api/endpoints/applications.ts";
import {usePostPackagesId} from "../api/endpoints/packages.ts";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";

type Props = {
    contracts: Contract[];
    name: string;
}

export default function InstallForm({name}: Props) {
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
            <button onClick={install} disabled={isPending}>
                Install
            </button>
        </>
    );
}