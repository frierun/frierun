import {useEffect, useState} from "react";
import {Volume} from "@/api/schemas";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

function VariantName(contract: Volume): string {
    if (contract.handler?.typeName === 'LocalPathHandler') {
        return "LocalPath";
    }
    
    const handlerName = contract.handler?.typeName.replace("Handler", "") ?? 'Unknown';
    return contract.id ? "Existing " + handlerName :  "New " + handlerName;
}

export default function VolumeForm({contractRef, contract, variants, updateContract}: ContractProps<Volume>) {
    const [value, setValue] = useState('');

    useEffect(() => {
        setValue((contract.handler?.typeName === 'LocalPathHandler' ? contract.localPath : contract.volumeName) ?? '');
    }, [contract]);

    return (
        <BaseForm
            contractRef={contractRef}
            contract={contract}
            variants={variants}
            variantName={VariantName}
            updateContract={updateContract}
        >
            {contract.handler?.typeName == 'VolumeHandler' &&
                (
                    <div>
                        <label className={"inline-block w-48"}>
                            Volume name:
                        </label>

                        <input
                            type="text"
                            value={value}
                            disabled={contract.id !== undefined}
                            onChange={e => {
                                setValue(e.target.value);
                                updateContract(contractRef, {
                                    ...contract,
                                    volumeName: e.target.value
                                });
                            }}
                        />
                    </div>
                )
            }
            {contract.handler?.typeName == 'LocalPathHandler' &&
                (
                    <div>
                        <label className={"inline-block w-48"}>
                            Local directory:
                        </label>
                        <input
                            type="text"
                            value={value}
                            onChange={e => {
                                setValue(e.target.value);
                                updateContract(contractRef, {
                                    ...contract,
                                    localPath: e.target.value
                                });
                            }}
                        />
                    </div>
                )
            }
        </BaseForm>
    );
}