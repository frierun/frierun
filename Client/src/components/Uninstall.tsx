import StateContext from "../providers/StateContext.tsx";
import {useContext} from "react";
import {getGetApplicationsQueryKey, useDeleteApplicationsId} from "../api/endpoints/applications.ts";
import {useQueryClient} from "@tanstack/react-query";
import Button from "./Button";

type Props = {
    applicationId: string;
}

export default function Uninstall({applicationId}: Props) {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = useDeleteApplicationsId();
    const queryClient = useQueryClient()
    
    const uninstall = () => {
        mutateAsync({id: applicationId})
            .then(waitForReady)
            .then(() => queryClient.invalidateQueries({queryKey: getGetApplicationsQueryKey()}));
    }

    return (
        <Button type={"default"} onClick={uninstall} disabled={isPending}>Uninstall</Button>
    );
}