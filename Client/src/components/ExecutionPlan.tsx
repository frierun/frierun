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

    if (executionPlan.typeName === 'SelectorProvider') {
        return (
            <div className={'m-2'}>
                <label>
                    provider:
                    <select value={parameters.selected} onChange={e => setParameters({selected: e.target.value})}>
                        {executionPlan.children.map((child, index) => (
                            <option key={child.typeName} value={index}>{child.typeName}</option>
                        ))}
                    </select>
                </label>
                <div className={'m-2'}>
                    {executionPlan.children.map((child, index) => (
                        <div key={index} className={index.toString() === parameters.selected ? '' : 'hidden'}>
                            <ExecutionPlan
                                executionPlan={child}
                                onChange={
                                    (result) => setChildren((oldChildren) => oldChildren.map((c, i) => i === index ? result : c))
                                }
                            />
                        </div>
                    ))}
                </div>
            </div>
        );
    }

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