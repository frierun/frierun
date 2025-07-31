import {useEffect, useState} from "react";
import {DockerApiConnection} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function DockerApiConnectionForm({contract, updateContract}: ContractProps<DockerApiConnection>) {
    const [path, setPath] = useState('');

    useEffect(() => {
        setPath(contract.path ?? '');
    }, [contract]);

    return (
        <BaseForm contract={contract} updateContract={updateContract}>
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Socket path:
                </label>
                <input
                    type="text"
                    value={path}
                    onChange={e => {
                        setPath(e.target.value);
                        updateContract({
                            ...contract,
                            path: e.target.value,
                        })
                    }}
                />
            </div>
        </BaseForm>
    );
}