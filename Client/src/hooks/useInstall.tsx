import {getGetApplicationsQueryKey} from "@/api/endpoints/applications.ts";
import {useContext} from "react";
import StateContext from "@/providers/StateContext.tsx";
import {usePostPackagesIdInstall} from "@/api/endpoints/packages.ts";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";
import {ContractList} from "@/types.ts";

type Props = {
    packageName: string;
    overrides: ContractList;
    prefix: string;
    setError: (error: string | null) => void;
}

export default function useInstall({packageName, overrides, prefix, setError}: Props)
{
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostPackagesIdInstall();
    const queryClient = useQueryClient()
    const navigate = useNavigate();
    
    const install = async () => {
        setError(null);
        const result = await mutateAsync({
            id: packageName,
            data: {
                name: prefix,
                contracts: overrides,
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