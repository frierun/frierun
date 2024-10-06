import StateContext from "../providers/StateContext.tsx";
import {useContext} from "react";
import {getGetApplicationsQueryKey, useDeleteApplicationsId} from "../api/endpoints/applications.ts";
import {useQueryClient} from "@tanstack/react-query";

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
        <button
            disabled={isPending}
            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
            onClick={uninstall}
        >
            Uninstall
        </button>
    );
}