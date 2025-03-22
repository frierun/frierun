import StateContext from "../providers/StateContext.tsx";
import {useContext} from "react";
import {getGetApplicationsQueryKey, useDeleteApplicationsName} from "../api/endpoints/applications.ts";
import {useQueryClient} from "@tanstack/react-query";
import Button from "./Button";

type Props = {
    name: string;
}

export default function Uninstall({name}: Props) {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = useDeleteApplicationsName();
    const queryClient = useQueryClient()
    
    const uninstall = () => {
        mutateAsync({name})
            .then(waitForReady)
            .then(() => queryClient.invalidateQueries({queryKey: getGetApplicationsQueryKey()}));
    }

    return (
        <Button type={"default"} onClick={uninstall} disabled={isPending}>Uninstall</Button>
    );
}