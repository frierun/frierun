import {useEffect, useState} from "react";
import {
    ExecutionPlanRequest,
    ExecutionPlanRequestParameters,
    type ExecutionPlanSelector,
} from "../api/schemas";

type Props = {
    selector: ExecutionPlanSelector;
    onChange: (executionPlan: ExecutionPlanRequest) => void;
}

export default function ExecutionPlan({selector, onChange}: Props) {
    const [selectedIndex, setSelectedIndex] = useState(selector.selectedIndex);
    const executionPlan = selector.children[selectedIndex];
    
    const [parameters, setParameters] = useState<ExecutionPlanRequestParameters>(executionPlan.parameters);
    const [children, setChildren] = useState<ExecutionPlanRequest[]>(
        () => Array(executionPlan.children.length).fill({children: [], parameters: {}, selectedIndex: 0})
    );

    useEffect(() => {
        setParameters(executionPlan.parameters);
        setChildren(Array(executionPlan.children.length).fill({children: [], parameters: {}, selectedIndex: 0}));
    }, [executionPlan]);

    useEffect(() => {
        onChange({selectedIndex, children, parameters});
    }, [children, parameters]);

    return (
        <div className={'m-2'}>
            <label>
                provider:
                <select value={parameters.selected} onChange={e => setSelectedIndex(parseInt(e.target.value))}>
                    {selector.children.map((child, index) => (
                        <option key={child.typeName} value={index}>{child.typeName}</option>
                    ))}
                </select>
            </label>
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
                        selector={child}
                        onChange={
                            (result) => setChildren((oldChildren) => oldChildren.map((c, i) => i === index ? result : c))
                        }
                    />
                ))}
            </div>
        </div>
    );
}