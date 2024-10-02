import useAxios from "axios-hooks";
import Package from "../models/Package.ts";
import {useEffect, useState} from "react";

type Props = {
    defaultName: string;
    defaultPort: number;
    pkg: Package;
}

export default function InstallForm({pkg, defaultName, defaultPort}: Props) {
    const [name, setName] = useState(defaultName);
    const [port, setPort] = useState(defaultPort);
    
    useEffect(() => {
        setName(defaultName);
        setPort(defaultPort);
    }, [defaultName, defaultPort]);

    const [{loading: sending}, send] = useAxios(
        {
            url: `/api/v1/packages/${pkg.name}`,
            method: 'POST',
            data: {
                name,
                port
            }
        },
        {manual: true}
    );

    const install = () => {
        send().catch(() => {
            console.log('Failed to install the package');
        });
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
            <button onClick={install} disabled={sending}>
                Install
            </button>
        </>
    );
}