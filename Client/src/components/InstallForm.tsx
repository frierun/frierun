import {useContext, useState} from "react";
import StateContext from "../providers/StateContext.tsx";
import {ParametersResponse} from "../api/schemas";
import {getGetApplicationsQueryKey} from "../api/endpoints/applications.ts";
import {usePostPackagesId} from "../api/endpoints/packages.ts";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";
import ResourceParameters from "./ResourceParameters.tsx";

type Props = {
    response: ParametersResponse;
}

export default function InstallForm({response}: Props) {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostPackagesId();
    const queryClient = useQueryClient()
    const navigate = useNavigate();

    const [name, setName] = useState(response.name);
    const [parameters, setParameters] = useState(response.parameters);

    const install = () => {
        mutateAsync({id: response.package.name, data: {name: name, parameters: parameters}})
            .then(waitForReady)
            .then(() => queryClient.invalidateQueries({queryKey: getGetApplicationsQueryKey()}))
            .then(() => navigate('/'));
    };

    return (
        <>
            <div className={'m-2'}>
                <div>
                    resource: Package
                </div>
                <div>
                    <label>
                        name:
                        <input
                            type="text"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                        />
                    </label>
                </div>
            </div>
            {response.package.resources.map((resource, index) => (
                <ResourceParameters
                    key={index}
                    resource={resource}
                    parameters={parameters[index]}
                    onParametersChange={(newParameters) =>
                        setParameters(parameters => parameters.map(
                            (p, i) => i === index ? newParameters : p
                        ))
                    }
                />
            ))}
            <button onClick={install} disabled={isPending}>
                Install
            </button>
        </>
    );
}