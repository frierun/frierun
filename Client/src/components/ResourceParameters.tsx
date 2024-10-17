import {useEffect, useState} from "react";
import {ResourceDefinition, type StringStringKeyValuePair} from "../api/schemas";

type Props = {
    parameters: StringStringKeyValuePair[];
    resource: ResourceDefinition;
    onParametersChange: (parameters: StringStringKeyValuePair[]) => void;
}

export default function ResourceParameters({resource, parameters, onParametersChange}: Props) {
    const [currentParameters, setCurrentParameters] = useState<StringStringKeyValuePair[]>(parameters);

    useEffect(() => {
        setCurrentParameters(parameters);
    }, [parameters]);

    return (
        <div className={'m-2'}>
            <div>
                resource: {resource.Type as string}
            </div>
            {currentParameters.map((pair) => (
                <div key={pair.key}>
                    <label>
                        {pair.key}:
                        <input 
                            type="text"
                            value={pair.value ?? ""}
                            onChange={(e) => {
                                const newParameters = [
                                    ...currentParameters.filter((p) => p.key !== pair.key),
                                    {key: pair.key, value: e.target.value}
                                ];
                                setCurrentParameters(newParameters);
                                onParametersChange(newParameters);
                            }}
                        />
                    </label>
                </div>
            ))}
        </div>
    );
}