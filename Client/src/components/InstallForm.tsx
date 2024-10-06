import {useContext, useEffect, useState} from "react";
import StateContext from "../providers/StateContext.tsx";
import {Package} from "../api/schemas";
import {getGetApplicationsQueryKey} from "../api/endpoints/applications.ts";
import {usePostPackagesId} from "../api/endpoints/packages.ts";
import {useQueryClient} from "@tanstack/react-query";
import {useNavigate} from "react-router-dom";

type Props = {
    defaultName: string;
    defaultPort: number;
    pkg: Package;
}

export default function InstallForm({pkg, defaultName, defaultPort}: Props) {
    const {waitForReady} = useContext(StateContext);
    const {mutateAsync, isPending} = usePostPackagesId();
    const queryClient = useQueryClient()
    const navigate = useNavigate();    
    
    const [name, setName] = useState(defaultName);
    const [port, setPort] = useState(defaultPort);

    useEffect(() => {
        setName(defaultName);
        setPort(defaultPort);
    }, [defaultName, defaultPort]);

    const install = () => {
        mutateAsync({id: pkg.name, data: {name, port}})
            .then(waitForReady)
            .then(() => queryClient.invalidateQueries({queryKey: getGetApplicationsQueryKey()}))
            .then(() => navigate('/'));
    };

    return (
        <>
            <div>
                <label>
                    Name:
                    <input type="text" value={name} onChange={(e) => setName(e.target.value)}/>
                </label>
            </div>
            <div>
                <label>
                    Port:
                    <input type="number" value={port} onChange={(e) => setPort(parseInt(e.target.value))}/>
                </label>
            </div>
            <button onClick={install} disabled={isPending}>
                Install
            </button>
        </>
    );
}