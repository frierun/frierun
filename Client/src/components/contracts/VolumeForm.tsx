import {useEffect, useState} from "react";
import {Volume} from "@/api/schemas";
import {useGetVolumes} from "@/api/endpoints/volumes.ts";
import {ContractProps} from "@/components/contracts/ContractForm.tsx";
import BaseForm from "@/components/contracts/BaseForm.tsx";

function VariantName(contract: Volume): string {
    return contract.handler?.typeName.replace("Handler", "") ?? 'Unknown';
}

export default function VolumeForm({contract, variants, updateContract}: ContractProps<Volume>) {
    const [value, setValue] = useState('');
    const {data} = useGetVolumes();

    useEffect(() => {
        setValue((contract.handler?.typeName === 'LocalPathHandler' ? contract.localPath : contract.volumeName) ?? '');
    }, [contract]);

    return (
        <BaseForm
            contract={contract}
            variants={variants}
            variantName={VariantName}
            updateContract={updateContract}
        >
            {contract.handler?.typeName == 'ExistingVolumeHandler' &&
                (
                    <div>
                        <label className={"inline-block w-48"}>
                            Existing volume name:
                        </label>

                        <select
                            value={value}
                            onChange={e => {
                                setValue(e.target.value);
                                updateContract({
                                    ...contract,
                                    volumeName: e.target.value
                                });
                            }}
                        >
                            {data?.data.map(volume => (
                                <option key={volume} value={volume}>{volume}</option>
                            ))}
                        </select>

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
                                updateContract({
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