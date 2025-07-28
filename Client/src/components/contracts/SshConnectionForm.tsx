import {useEffect, useState} from "react";
import {SshConnection,} from "@/api/schemas";

type Props = {
    contract: SshConnection;
    updateContract: (contract: SshConnection, isRefetch?: boolean) => void;
}

export default function SshConnectionForm({contract, updateContract}: Props) {
    const [host, setHost] = useState('');
    const [port, setPort] = useState(22);
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');

    useEffect(() => {
        setHost(contract.host ?? '');
        setPort(contract.port);
        setUsername(contract.username ?? '');
        setPassword(contract.password ?? '');
    }, [contract]);
    
    return (
        <div className="card">
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Ssh connection
                </label>
                {contract.name}
            </div>
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Host:
                </label>
                <input
                    type="text"
                    value={host}
                    onChange={e => {
                        setHost(e.target.value);
                        updateContract({
                            ...contract,
                            host: e.target.value,
                            port,
                            username,
                            password,
                        });
                    }}
                />
            </div>
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Port:
                </label>
                <input
                    type="text"
                    value={port}
                    onChange={e => {
                        let port = parseInt(e.target.value);
                        port = isNaN(port) ? 0 : port;
                        setPort(port);
                        updateContract({
                            ...contract,
                            host,
                            port,
                            username,
                            password
                        });
                    }}
                />
            </div>
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Username:
                </label>
                <input
                    type="text"
                    value={username}
                    onChange={e => {
                        setUsername(e.target.value);
                        updateContract({
                            ...contract,
                            host,
                            port,
                            username: e.target.value,
                            password
                        });
                    }}
                />
            </div>
            <div>
                <label className={"inline-block w-48"}>
                    Password:
                </label>
                <input
                    type="password"
                    value={password}
                    onChange={e => {
                        setPassword(e.target.value);
                        updateContract({
                            ...contract,
                            host,
                            port,
                            username,
                            password: e.target.value
                        });
                    }}
                />
            </div>
        </div>
    );
}