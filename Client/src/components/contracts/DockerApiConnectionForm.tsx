import {useEffect, useState} from "react";
import {DockerApiConnection} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function DockerApiConnectionForm(
    {
        contractRef,
        contract,
        updateContract,
        variants
    }: ContractProps<DockerApiConnection>
) {
    const [path, setPath] = useState('');

    useEffect(() => {
        setPath(contract.path ?? '');
    }, [contract]);
    
    return (
        <BaseForm contractRef={contractRef} contract={contract} updateContract={updateContract} variants={variants}>
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Socket path:
                </label>
                <input
                    type="text"
                    value={path}
                    disabled={contract.id !== undefined}
                    onChange={e => {
                        setPath(e.target.value);
                        updateContract(contractRef,
                            {
                                ...contract,
                                path: e.target.value,
                            }
                        );
                    }}
                />
            </div>
        </BaseForm>
    );
}