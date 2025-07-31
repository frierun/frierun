import {useEffect, useState} from "react";
import {SshConnection,} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function SshConnectionForm({contract, updateContract}: ContractProps<SshConnection>) {
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
    
    const updateContractValues = (changed: Partial<SshConnection>) => {
        updateContract({
            ...contract,
            host,
            port,
            username,
            password,
            ...changed
        });
    }

    return (
        <BaseForm contract={contract} updateContract={updateContract}>
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Host:
                </label>
                <input
                    type="text"
                    value={host}
                    onChange={e => {
                        setHost(e.target.value);
                        updateContractValues({
                            host: e.target.value,
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
                        updateContractValues({
                            port,
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
                        updateContractValues({
                            username: e.target.value,
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
                        updateContractValues({
                            password: e.target.value
                        });
                    }}
                />
            </div>
        </BaseForm>
    );
}