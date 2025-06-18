import {getGetApplicationsQueryKey} from "@/api/endpoints/applications.ts";
import {useContext} from "react";
import StateContext from "@/providers/StateContext.tsx";
import {usePostPackagesIdInstall} from "@/api/endpoints/packages.ts";
import {Package} from "@/api/schemas";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";
import {Contract} from "@/components/contracts/ContractForm.tsx";

type Props = {
    packageContract: Package;
    overrides: Contract[] ;
    prefix: string;
    setError: (error: string | null) => void;
}

export default function useInstall({packageContract, overrides, prefix, setError}: Props)
{
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostPackagesIdInstall();
    const queryClient = useQueryClient()
    const navigate = useNavigate();
    
    const install = async () => {
        setError(null);
        const result = await mutateAsync({
            id: packageContract.name,
            data: {
                ...packageContract,
                prefix,
                contracts: Object.entries(overrides).flatMap(([, value]) => value),
            }
        });

        if (result.status === 404) {
            setError("Package not found");
            return;
        }

        if (result.status === 409) {
            setError(`${result.data.message} ${result.data.solution}`);
            return;
        }

        const state = await waitForReady();
        if (state.error) {
            setError(`${state.error.message} ${state.error.solution}`);
            return;
        }
        await queryClient.invalidateQueries({queryKey: getGetApplicationsQueryKey()});
        navigate('/');
    };
    
    return {install, isPending};
}