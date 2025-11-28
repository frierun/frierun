import Button from "@/components/Button.tsx";
import {getGetContractsQueryKey, usePostContractsDiscover} from "@/api/endpoints/contracts.ts";
import {useContext} from "react";
import StateContext from "@/providers/StateContext.tsx";
import {useQueryClient} from "@tanstack/react-query";


export default function DiscoverButton() {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostContractsDiscover();
    const queryClient = useQueryClient()
    
    const discover = async () => {
        await mutateAsync();
        
        await waitForReady();
        await queryClient.invalidateQueries({queryKey: getGetContractsQueryKey()});
    }
    
    return (
        <Button onClick={discover} disabled={isPending} type={"primary"}>
            Discover
        </Button>
    );
}