import {useContext, useState} from "react";
import StateContext from "../providers/StateContext.tsx";
import {ExecutionPlanRequest, ParametersResponse} from "../api/schemas";
import {getGetApplicationsQueryKey} from "../api/endpoints/applications.ts";
import {usePostPackagesId} from "../api/endpoints/packages.ts";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";
import ExecutionPlan from "./ExecutionPlan.tsx";

type Props = {
    response: ParametersResponse;
}

export default function InstallForm({response}: Props) {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostPackagesId();
    const queryClient = useQueryClient()
    const navigate = useNavigate();
    const [executionPlan, setExecutionPlan] = useState<ExecutionPlanRequest>();

    const install = () => {
        if (!executionPlan) {
            return;
        }
        
        mutateAsync({id: response.package.name, data: {executionPlan}})
            .then(waitForReady)
            .then(() => queryClient.invalidateQueries({queryKey: getGetApplicationsQueryKey()}))
            .then(() => navigate('/'));
    };

    return (
        <>
            <ExecutionPlan
                selector={response.executionPlan}
                onChange={(executionPlan) => setExecutionPlan(executionPlan)}
            />
            <button onClick={install} disabled={isPending}>
                Install
            </button>
        </>
    );
}