import {useEffect, useState} from "react";
import {CloudflareApiConnection} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

export default function CloudflareApiConnectionForm({contract, updateContract}: ContractProps<CloudflareApiConnection>) {
    const [token, setToken] = useState('');

    useEffect(() => {
        setToken(contract.token);
    }, [contract]);

    return (
        <BaseForm contract={contract} updateContract={updateContract}>
            <div className={"my-1.5"}>
                <label className={"inline-block w-48"}>
                    Token:
                </label>
                <input
                    type="text"
                    value={token}
                    onChange={e => {
                        setToken(e.target.value);
                        updateContract({
                            ...contract,
                            token: e.target.value,
                        })
                    }}
                />
            </div>
        </BaseForm>
    );
}