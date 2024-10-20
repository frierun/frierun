import {useEffect, useState} from "react";
import {
    ExecutionPlanRequest,
    ExecutionPlanRequestParameters,
    ExecutionPlanResponse,
} from "../api/schemas";

type Props = {
    executionPlan: ExecutionPlanResponse;
    onChange: (executionPlan: ExecutionPlanRequest) => void;
}

export default function ExecutionPlan({executionPlan, onChange}: Props) {
    const [parameters, setParameters] = useState<ExecutionPlanRequestParameters>(executionPlan.parameters);
    const [children, setChildren] = useState<ExecutionPlanRequest[]>(executionPlan.children);

    useEffect(() => {
        setParameters(executionPlan.parameters);
        setChildren(executionPlan.children);
    }, [executionPlan]);

    useEffect(() => {
        onChange({children, parameters});
    }, [children, parameters]);

    return (
        <div className={'m-2'}>
            <div>
                provider: {executionPlan.typeName}
            </div>
            {Object.entries(parameters).map((pair) => (
                <div key={pair[0]}>
                    <label>
                        {pair[0]}:
                        <input
                            type="text"
                            value={pair[1]}
                            onChange={
                                (e) => setParameters(oldParameters => ({
                                    ...oldParameters,
                                    [pair[0]]: e.target.value
                                }))
                            }
                        />
                    </label>
                </div>
            ))}
            <div className={'m-2'}>
                {executionPlan.children.map((child, index) => (
                    <ExecutionPlan
                        key={index}
                        executionPlan={child}
                        onChange={
                            (result) => setChildren((oldChildren) => oldChildren.map((c, i) => i === index ? result : c))
                    }
                    />
                ))}
            </div>
        </div>
    );
}